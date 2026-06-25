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
            // LocalizationSettings must finish initializing before locales are queryable.
            yield return LocalizationSettings.InitializationOperation;

            LoadSavedLanguage();

            foreach (LanguageButton langBtn in _languageButtons)
            {
                langBtn.Button.onClick.AddListener(() => ChangeLanguage(langBtn.Locale));
            }
        }

        private void LoadSavedLanguage()
        {
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

            // Fall back to the device language, then to the first available locale if
            // the device language isn't in the project's locale list.
            Locale deviceLocale = LocalizationSettings.AvailableLocales.GetLocale(Application.systemLanguage);
            LocalizationSettings.SelectedLocale = deviceLocale != null
                ? deviceLocale
                : LocalizationSettings.AvailableLocales.Locales[0];
        }

        private void ChangeLanguage(Locale targetLocale)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
            PlayerPrefs.SetString(_languagePrefsString, targetLocale.Identifier.Code);
            PlayerPrefs.Save();
        }
    }
}