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
        [SerializeField] private string _qualityPrefsKey = "quality_level";

        private void Awake()
        {
            InitEntry(_masterVolume);
            InitEntry(_sfxVolume);
            InitEntry(_musicVolume);
            InitQualityDropdown();
        }

        private void OnDestroy()
        {
            _masterVolume.Slider.onValueChanged.RemoveAllListeners();
            _sfxVolume.Slider.onValueChanged.RemoveAllListeners();
            _musicVolume.Slider.onValueChanged.RemoveAllListeners();
            _qualityDropdown.onValueChanged.RemoveAllListeners();
        }

        private void OnApplicationQuit() => PlayerPrefs.Save();

        private void InitEntry(VolumeEntry entry)
        {
            var linear = PlayerPrefs.GetFloat(entry.PrefsKey, 1f);
            entry.Slider.SetValueWithoutNotify(linear);
            ApplyVolume(entry.MixerParameter, linear);
            entry.Slider.onValueChanged.AddListener(v => OnVolumeChanged(entry, v));
        }

        private void OnVolumeChanged(VolumeEntry entry, float linear)
        {
            ApplyVolume(entry.MixerParameter, linear);
            PlayerPrefs.SetFloat(entry.PrefsKey, linear);
        }

        private void ApplyVolume(string parameter, float linear) =>
            _mixer.SetFloat(parameter, linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f);

        private void InitQualityDropdown()
        {
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
        }
    }
}
