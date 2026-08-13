using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using SteelHorse.Framework.UI;

namespace SteelHorse.Framework.Services.Options
{
    // Owns every player-facing setting's persistence (PlayerPrefs) and system application
    // (AudioMixer/QualitySettings/Screen), applying all of them once from Setup - which
    // GameManagers calls during its own Awake, so settings take effect on boot regardless
    // of whether the player ever opens an options screen. PlayerOptionsController is the
    // UI-only counterpart: it never touches PlayerPrefs or these systems directly, it just
    // reads/writes through this service.
    public class OptionsService : MonoBehaviour, IOptionsService
    {
        [Serializable]
        private struct VolumeChannel
        {
            public string Name;
            [Tooltip("Exposed parameter name on the AudioMixer")]
            public string MixerParameter;
            [Tooltip("PlayerPrefs key for persisting this value")]
            public string PrefsKey;
        }

        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private VolumeChannel[] _volumeChannels;

        [Header("Quality")]
        [SerializeField] private string _qualityPrefsKey = "QualityLevel";

        [Header("Resolution")]
        [SerializeField] private ResolutionOptions _resolutionOptions;
        [SerializeField] private string _resolutionPrefsKey = "ResolutionIndex";

        public event Action<int, int> ResolutionChanged;

        public int CurrentQualityIndex { get; private set; }

        public ResolutionSetting[] Resolutions { get; private set; }
        public int CurrentResolutionIndex { get; private set; }
        public Vector2Int CurrentResolution { get; private set; }

        public void Setup()
        {
            // Setup only ever runs once GameManagers.Awake has confirmed this is the
            // surviving singleton (not a short-lived duplicate destroyed on scene load),
            // so starting a coroutine here is safe.
            StartCoroutine(InitializeVolumesNextFrame());
            InitializeQuality();
            InitializeResolution();
        }

        private void OnApplicationQuit() => PlayerPrefs.Save();

        // AudioMixer.SetFloat calls made as early as Setup (itself called from
        // GameManagers.Awake) aren't reliably audible yet - same category of AudioMixer
        // init-order quirk MusicChannel's constructor works around by priming with
        // silence rather than a real value (see its comment). Waiting one frame lets the
        // mixer finish initializing before applying the player's actual saved volume, so
        // the first real audio isn't briefly at the wrong level.
        private IEnumerator InitializeVolumesNextFrame()
        {
            yield return null;

            // Not every game wires up a mixer - skip entirely if unwired.
            if (_mixer == null)
                yield break;

            foreach (var channel in _volumeChannels)
                ApplyVolume(channel.MixerParameter, PlayerPrefs.GetFloat(channel.PrefsKey, 1f));
        }

        public float GetVolume(string channelName) =>
            TryFindChannel(channelName, out var channel) ? PlayerPrefs.GetFloat(channel.PrefsKey, 1f) : 1f;

        public void SetVolume(string channelName, float linear)
        {
            if (!TryFindChannel(channelName, out var channel))
                return;

            ApplyVolume(channel.MixerParameter, linear);
            PlayerPrefs.SetFloat(channel.PrefsKey, linear);

            // PlayerPrefs.Save() is a synchronous disk flush - calling it on every tick of
            // a mouse drag (which fires this continuously) stutters the drag and the audio
            // along with it. Debounced so it still survives an abnormal exit (crash,
            // task-kill, some mobile suspend paths) shortly after the value settles,
            // instead of flushing to disk on every intermediate value.
            CancelInvoke(nameof(FlushPrefs));
            Invoke(nameof(FlushPrefs), 0.5f);
        }

        private void FlushPrefs() => PlayerPrefs.Save();

        private void ApplyVolume(string parameter, float linear)
        {
            if (_mixer == null)
                return;

            _mixer.SetFloat(parameter, linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f);
        }

        private bool TryFindChannel(string channelName, out VolumeChannel channel)
        {
            foreach (var candidate in _volumeChannels)
            {
                if (candidate.Name == channelName)
                {
                    channel = candidate;
                    return true;
                }
            }

            channel = default;
            return false;
        }

        private void InitializeQuality()
        {
            CurrentQualityIndex = PlayerPrefs.GetInt(_qualityPrefsKey, QualitySettings.GetQualityLevel());
            QualitySettings.SetQualityLevel(CurrentQualityIndex, true);
        }

        public void SetQuality(int index)
        {
            CurrentQualityIndex = index;
            QualitySettings.SetQualityLevel(index, true);
            PlayerPrefs.SetInt(_qualityPrefsKey, index);
            PlayerPrefs.Save();
        }

        private void InitializeResolution()
        {
            // Not every game exposes a resolution setting - skip entirely if unwired.
            if (_resolutionOptions == null)
                return;

            var nativeMonitorResolution = new Vector2Int(Display.main.systemWidth, Display.main.systemHeight);
            Resolutions = BuildResolutionsWithMonitorOption(_resolutionOptions.Resolutions, nativeMonitorResolution, out int monitorIndex);

            bool hasSaved = PlayerPrefs.HasKey(_resolutionPrefsKey);
            CurrentResolutionIndex = hasSaved ? PlayerPrefs.GetInt(_resolutionPrefsKey) : monitorIndex;

            if (!hasSaved)
            {
                // First time this runs on this machine: adopt the monitor's resolution as
                // the default so it doesn't silently revert to it every launch.
                PlayerPrefs.SetInt(_resolutionPrefsKey, CurrentResolutionIndex);
                PlayerPrefs.Save();
            }

            ApplyResolutionSetting(Resolutions[CurrentResolutionIndex]);
        }

        public void SetResolution(int index)
        {
            CurrentResolutionIndex = index;
            var setting = Resolutions[index];
            ApplyResolutionSetting(setting);

            PlayerPrefs.SetInt(_resolutionPrefsKey, index);
            PlayerPrefs.Save();

            ResolutionChanged?.Invoke((int)setting.Resolution.x, (int)setting.Resolution.y);
        }

        private void ApplyResolutionSetting(ResolutionSetting setting)
        {
            Screen.SetResolution((int)setting.Resolution.x, (int)setting.Resolution.y, Screen.fullScreenMode);
            CurrentResolution = new Vector2Int((int)setting.Resolution.x, (int)setting.Resolution.y);
        }

        // Drops any curated preset wider or taller than the player's native monitor
        // resolution - selecting an unsupported mode (e.g. 2560x1440 on a 1920x1080
        // display) would otherwise be possible. Then appends that native resolution as an
        // extra option when it isn't already one of the remaining entries, so they can
        // always run at their native resolution instead of being limited to whatever
        // presets fit, and still have at least one option if every preset got filtered
        // out. Both comparisons key off the same nativeMonitorResolution value so neither
        // can drift from the other. Read from Display.main.systemWidth/systemHeight (the
        // OS-reported native resolution) rather than Screen.currentResolution, which
        // reflects the game's own current display mode instead and would drift from the
        // real monitor ceiling once a resolution is applied in exclusive fullscreen.
        private static ResolutionSetting[] BuildResolutionsWithMonitorOption(ResolutionSetting[] configured, Vector2Int nativeMonitorResolution, out int monitorIndex)
        {
            var fitting = new List<ResolutionSetting>(configured.Length);
            foreach (var setting in configured)
            {
                if ((int)setting.Resolution.x <= nativeMonitorResolution.x && (int)setting.Resolution.y <= nativeMonitorResolution.y)
                    fitting.Add(setting);
            }

            for (int i = 0; i < fitting.Count; i++)
            {
                if ((int)fitting[i].Resolution.x == nativeMonitorResolution.x && (int)fitting[i].Resolution.y == nativeMonitorResolution.y)
                {
                    monitorIndex = i;
                    return fitting.ToArray();
                }
            }

            monitorIndex = fitting.Count;
            fitting.Add(new ResolutionSetting(new Vector2(nativeMonitorResolution.x, nativeMonitorResolution.y)));
            return fitting.ToArray();
        }
    }
}
