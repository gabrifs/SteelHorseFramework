// NOTE: This script is setted up to work with the Unity Localization package

using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace SteelHorse.Framework.UI
{
    public class LanguageSwitcher : MonoBehaviour
    {
        [System.Serializable]
        public struct LanguageButton
        {
            public Button Button;
            public Locale Locale;
        }

        [SerializeField] private string _languagePrefsString = "SelectedLanguage";
        [SerializeField] private LanguageButton[] _languageButtons;

        private void Awake()
        {
            StartCoroutine(SetupLanguageButtons());
        }

        private IEnumerator SetupLanguageButtons()
        {
            // Wait for localization to initialize
            yield return LocalizationSettings.InitializationOperation;

            // Load saved language (or use device default)
            LoadSavedLanguage();

            // Setup button click events
            foreach (LanguageButton langBtn in _languageButtons)
            {
                langBtn.Button.onClick.AddListener(() => ChangeLanguage(langBtn.Locale));
            }
        }

        private void LoadSavedLanguage()
        {
            // Try to find saved locale
            string savedLangCode = PlayerPrefs.GetString(_languagePrefsString, "");
            if (!string.IsNullOrEmpty(savedLangCode))
            {
                Locale savedLocale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(savedLangCode));
                if(savedLocale != null)
                {
                    LocalizationSettings.SelectedLocale = savedLocale;
                    return;
                }
            }

            // Use device language OR first available locale
            Locale deviceLocale = LocalizationSettings.AvailableLocales.GetLocale(Application.systemLanguage);
            if(deviceLocale != null)
            {
                LocalizationSettings.SelectedLocale = deviceLocale;
            }
            else
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
            }
        }

        private void ChangeLanguage(Locale targetLocale)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
            PlayerPrefs.SetString(_languagePrefsString, targetLocale.Identifier.Code);
            PlayerPrefs.Save();
        }
    }
}