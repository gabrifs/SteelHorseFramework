using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SteelHorse.Framework.Services.Options;

namespace SteelHorse.Framework.UI
{
    // Pure UI layer over IOptionsService: wires Sliders/Dropdowns, reads initial values
    // from the service, and forwards player interaction back into it. Never touches
    // PlayerPrefs, AudioMixer, QualitySettings, or Screen directly - that's all
    // OptionsService's job, applied once at Setup (see its own comment) so settings take
    // effect on boot whether or not this screen is ever opened.
    public class PlayerOptionsController : MonoBehaviour
    {
        [Serializable]
        private struct VolumeEntry
        {
            public Slider Slider;
            [Tooltip("Channel name matching one of OptionsService's configured volume channels")]
            public string ChannelName;
        }

        [SerializeField] private VolumeEntry _masterVolume;
        [SerializeField] private VolumeEntry _sfxVolume;
        [SerializeField] private VolumeEntry _musicVolume;

        [Header("Quality")]
        [SerializeField] private TMP_Dropdown _qualityDropdown;

        [Header("Resolution")]
        [SerializeField] private TMP_Dropdown _resolutionDropdown;

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

        private void InitEntry(VolumeEntry entry)
        {
            // Lets a scene wire up only the sliders it actually has (e.g. no music
            // slider on a game with no music) instead of requiring every entry.
            if (entry.Slider == null || string.IsNullOrEmpty(entry.ChannelName))
                return;

            var service = GameManagers.Instance.Services.OptionsService;
            entry.Slider.SetValueWithoutNotify(service.GetVolume(entry.ChannelName));
            entry.Slider.onValueChanged.AddListener(v => GameManagers.Instance.Services.OptionsService.SetVolume(entry.ChannelName, v));
        }

        private void InitQualityDropdown()
        {
            // Not every game exposes a quality setting - skip entirely if unwired.
            if (_qualityDropdown == null)
                return;

            _qualityDropdown.ClearOptions();
            _qualityDropdown.AddOptions(new List<string>(QualitySettings.names));

            var service = GameManagers.Instance.Services.OptionsService;
            _qualityDropdown.SetValueWithoutNotify(service.CurrentQualityIndex);
            _qualityDropdown.onValueChanged.AddListener(service.SetQuality);
        }

        private void InitResolutionDropdown()
        {
            var service = GameManagers.Instance.Services.OptionsService;

            // Not every game exposes a resolution setting - skip entirely if unwired.
            if (_resolutionDropdown == null || service.Resolutions == null || service.Resolutions.Length == 0)
                return;

            _resolutionDropdown.ClearOptions();
            _resolutionDropdown.AddOptions(BuildResolutionLabels(service.Resolutions));

            _resolutionDropdown.SetValueWithoutNotify(service.CurrentResolutionIndex);
            _resolutionDropdown.onValueChanged.AddListener(service.SetResolution);
        }

        private static List<string> BuildResolutionLabels(ResolutionSetting[] resolutions)
        {
            var labels = new List<string>(resolutions.Length);
            foreach (var setting in resolutions)
                labels.Add($"{(int)setting.Resolution.x} x {(int)setting.Resolution.y}");

            return labels;
        }
    }
}
