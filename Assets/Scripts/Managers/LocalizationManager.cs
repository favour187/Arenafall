using System.Collections.Generic;
using UnityEngine;
using ArenaFall.Core;

namespace ArenaFall.Managers
{
    /// <summary>
    /// Manages text localization for multiple languages.
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string _defaultLanguage = "en";
        [SerializeField] private TextAsset _localizationFile;

        private Dictionary<string, Dictionary<string, string>> _localizationData = new();
        private string _currentLanguage;

        public static LocalizationManager Instance { get; private set; }
        public string CurrentLanguage => _currentLanguage;
        public string[] AvailableLanguages { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register<LocalizationManager>(this);
        }

        private void Start()
        {
            LoadLocalizationData();
            SetLanguage(_defaultLanguage);
        }

        private void LoadLocalizationData()
        {
            if (_localizationFile == null)
            {
                Debug.LogWarning("[LocalizationManager] No localization file assigned");
                return;
            }

            // Parse CSV/JSON localization data
            string[] lines = _localizationFile.text.Split('\n');
            if (lines.Length < 2) return;

            // First line contains language headers
            string[] headers = lines[0].Trim().Split(',');
            AvailableLanguages = new string[headers.Length - 1];

            for (int i = 1; i < headers.Length; i++)
            {
                AvailableLanguages[i - 1] = headers[i].Trim();
                _localizationData[headers[i].Trim()] = new Dictionary<string, string>();
            }

            // Read key-value pairs per language
            for (int i = 1; i < lines.Length; i++)
            {
                string[] parts = lines[i].Trim().Split(',');
                if (parts.Length < 2) continue;

                string key = parts[0].Trim();
                for (int j = 1; j < parts.Length && j < headers.Length; j++)
                {
                    string lang = headers[j].Trim();
                    string value = parts[j].Trim();
                    _localizationData[lang][key] = value.Replace("\\n", "\n");
                }
            }

            Debug.Log($"[LocalizationManager] Loaded {_localizationData.Count} languages");
        }

        /// <summary>
        /// Set the current language.
        /// </summary>
        public void SetLanguage(string languageCode)
        {
            if (_localizationData.ContainsKey(languageCode))
            {
                _currentLanguage = languageCode;
                Debug.Log($"[LocalizationManager] Language set to {languageCode}");
            }
            else
            {
                Debug.LogWarning($"[LocalizationManager] Language {languageCode} not found, using default");
                _currentLanguage = _defaultLanguage;
            }
        }

        /// <summary>
        /// Get localized text for a key.
        /// </summary>
        public string GetText(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(_currentLanguage) || !_localizationData.ContainsKey(_currentLanguage))
            {
                return key;
            }

            var langData = _localizationData[_currentLanguage];
            if (langData.TryGetValue(key, out string text))
            {
                if (args.Length > 0)
                {
                    return string.Format(text, args);
                }
                return text;
            }

            // Fallback to English
            if (_currentLanguage != "en" && _localizationData.ContainsKey("en"))
            {
                var enData = _localizationData["en"];
                if (enData.TryGetValue(key, out string enText))
                {
                    return enText;
                }
            }

            return key;
        }

        /// <summary>
        /// Check if a language is available.
        /// </summary>
        public bool HasLanguage(string languageCode)
        {
            return _localizationData.ContainsKey(languageCode);
        }
    }
}
