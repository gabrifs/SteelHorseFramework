using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace SteelHorse.Framework.UI
{
    public class PlayerOptionsController : MonoBehaviour
    {
        // Unity has no native event for resolution changes, so anything that needs to
        // react to the player picking a new resolution subscribes here instead of polling
        // Screen.width/height.
        public static event Action<int, int> ResolutionChanged;

        [Serializable]
        private struct VolumeEntry
        {
            public Slider Slider;
            [Tooltip("Exposed parameter name on the AudioMixer")]
            public string MixerParameter;
            [Tooltip("PlayerPrefs key for persisting this value")]
            public string PrefsKey;
        }

        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private VolumeEntry _masterVolume;
        [SerializeField] private VolumeEntry _sfxVolume;
        [SerializeField] private VolumeEntry _musicVolume;

        [Header("Quality")]
        [SerializeField] private TMP_Dropdown _qualityDropdown;
        [SerializeField] private string _qualityPrefsKey = "QualityLevel";

        [Header("Resolution")]
        [SerializeField] private TMP_Dropdown _resolutionDropdown;
        [SerializeField] private ResolutionOptions _resolutionOptions;
        [SerializeField] private string _resolutionPrefsKey = "ResolutionIndex";

        // Curated options from _resolutionOptions, plus the player's monitor resolution
        // appended if it wasn't already one of them. Indices into this array are what's
        // persisted to PlayerPrefs and what the dropdown's onValueChanged reports.
        private ResolutionSetting[] _resolutions;

        // Resolved in Start rather than Awake: AudioMixer.SetFloat calls made this
        // early aren't reliably audible yet (same category of AudioMixer init-order
        // quirk MusicChannel works around — see its constructor comment).
        private void Start()
        {
            InitEntry(_masterVolume);
            InitEntry(_sfxVolume);
            InitEntry(_musicVolume);
            InitQualityDropdown();
            InitResolutionDropdown();
        }

        private void OnDestroy()
        {
            RemoveEntryListener(_masterVolume);
            RemoveEntryListener(_sfxVolume);
            RemoveEntryListener(_musicVolume);

            if (_qualityDropdown != null)
                _qualityDropdown.onValueChanged.RemoveAllListeners();

            if (_resolutionDropdown != null)
                _resolutionDropdown.onValueChanged.RemoveAllListeners();
        }

        private static void RemoveEntryListener(VolumeEntry entry)
        {
            if (entry.Slider != null)
                entry.Slider.onValueChanged.RemoveAllListeners();
        }

        private void OnApplicationQuit() => PlayerPrefs.Save();

        private void InitEntry(VolumeEntry entry)
        {
            // Lets a scene wire up only the sliders it actually has (e.g. no music
            // slider on a game with no music) instead of requiring every entry.
            if (_mixer == null || entry.Slider == null)
                return;

            var linear = PlayerPrefs.GetFloat(entry.PrefsKey, 1f);
            entry.Slider.SetValueWithoutNotify(linear);
            ApplyVolume(entry.MixerParameter, linear);
            entry.Slider.onValueChanged.AddListener(v => OnVolumeChanged(entry, v));
        }

        private void OnVolumeChanged(VolumeEntry entry, float linear)
        {
            ApplyVolume(entry.MixerParameter, linear);
            PlayerPrefs.SetFloat(entry.PrefsKey, linear);
            // Flushed immediately (not just on OnApplicationQuit) so a change survives
            // an abnormal exit (crash, task-kill, some mobile suspend paths) instead of
            // silently reverting to a stale value on next launch.
            PlayerPrefs.Save();
        }

        private void ApplyVolume(string parameter, float linear) =>
            _mixer.SetFloat(parameter, linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f);

        private void InitQualityDropdown()
        {
            // Not every game exposes a quality setting - skip entirely if unwired.
            if (_qualityDropdown == null)
                return;

            _qualityDropdown.ClearOptions();
            _qualityDropdown.AddOptions(new List<string>(QualitySettings.names));

            int saved = PlayerPrefs.GetInt(_qualityPrefsKey, QualitySettings.GetQualityLevel());
            _qualityDropdown.SetValueWithoutNotify(saved);
            QualitySettings.SetQualityLevel(saved, true);

            _qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }

        private void OnQualityChanged(int index)
        {
            QualitySettings.SetQualityLevel(index, true);
            PlayerPrefs.SetInt(_qualityPrefsKey, index);
            PlayerPrefs.Save();
        }

        private void InitResolutionDropdown()
        {
            // Not every game exposes a resolution setting - skip entirely if unwired.
            if (_resolutionDropdown == null || _resolutionOptions == null)
                return;

            _resolutions = BuildResolutionsWithMonitorOption(_resolutionOptions.Resolutions, out int monitorIndex);

            _resolutionDropdown.ClearOptions();
            _resolutionDropdown.AddOptions(BuildResolutionLabels(_resolutions));

            bool hasSaved = PlayerPrefs.HasKey(_resolutionPrefsKey);
            int index = hasSaved ? PlayerPrefs.GetInt(_resolutionPrefsKey) : monitorIndex;

            if (!hasSaved)
            {
                // First time this runs on this machine: adopt the monitor's resolution as
                // the default so it doesn't silently revert to it every launch.
                PlayerPrefs.SetInt(_resolutionPrefsKey, index);
                PlayerPrefs.Save();
            }

            _resolutionDropdown.SetValueWithoutNotify(index);
            ApplyResolution(_resolutions[index]);

            _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

            // No separate "initial sync" anywhere else - listeners get their first
            // value from this call.
            NotifyResolutionChanged(_resolutions[index]);
        }

        private void OnResolutionChanged(int index)
        {
            var setting = _resolutions[index];
            ApplyResolution(setting);
            PlayerPrefs.SetInt(_resolutionPrefsKey, index);
            PlayerPrefs.Save();

            NotifyResolutionChanged(setting);
        }

        private static void ApplyResolution(ResolutionSetting setting) =>
            Screen.SetResolution((int)setting.Resolution.x, (int)setting.Resolution.y, Screen.fullScreenMode);

        private static void NotifyResolutionChanged(ResolutionSetting setting) =>
            ResolutionChanged?.Invoke((int)setting.Resolution.x, (int)setting.Resolution.y);

        private static List<string> BuildResolutionLabels(ResolutionSetting[] resolutions)
        {
            var labels = new List<string>(resolutions.Length);
            foreach (var setting in resolutions)
                labels.Add($"{(int)setting.Resolution.x} x {(int)setting.Resolution.y}");

            return labels;
        }

        // Appends the player's monitor resolution as an extra option when it isn't
        // already one of the curated entries, so they can always run at their native
        // resolution instead of being limited to whatever presets were configured.
        private static ResolutionSetting[] BuildResolutionsWithMonitorOption(ResolutionSetting[] configured, out int monitorIndex)
        {
            var monitor = Screen.currentResolution;

            for (int i = 0; i < configured.Length; i++)
            {
                if ((int)configured[i].Resolution.x == monitor.width && (int)configured[i].Resolution.y == monitor.height)
                {
                    monitorIndex = i;
                    return configured;
                }
            }

            var withMonitor = new ResolutionSetting[configured.Length + 1];
            configured.CopyTo(withMonitor, 0);
            withMonitor[configured.Length] = new ResolutionSetting(new Vector2(monitor.width, monitor.height));

            monitorIndex = configured.Length;
            return withMonitor;
        }
    }
}
