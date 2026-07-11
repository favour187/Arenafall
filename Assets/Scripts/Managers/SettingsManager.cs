using UnityEngine;
using ArenaFall.Core;

namespace ArenaFall.Managers
{
    /// <summary>
    /// Manages game settings including graphics, audio, controls, and accessibility.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioManager _audioManager;

        private Data.PlayerSettings _currentSettings;

        public static SettingsManager Instance { get; private set; }
        public Data.PlayerSettings CurrentSettings => _currentSettings;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register<SettingsManager>(this);
        }

        private void Start()
        {
            LoadSettings();
        }

        /// <summary>
        /// Load settings from save data.
        /// </summary>
        public void LoadSettings()
        {
            var saveManager = ServiceLocator.Get<SaveManager>();
            if (saveManager?.CurrentSave != null)
            {
                _currentSettings = saveManager.CurrentSave.settings;
            }
            else
            {
                _currentSettings = new Data.PlayerSettings();
            }

            ApplySettings();
        }

        /// <summary>
        /// Apply all current settings.
        /// </summary>
        public void ApplySettings()
        {
            ApplyAudioSettings();
            ApplyGraphicsSettings();
            ApplyAccessibilitySettings();
        }

        public void ApplyAudioSettings()
        {
            if (_audioManager == null)
                _audioManager = ServiceLocator.Get<AudioManager>();

            if (_audioManager != null)
            {
                _audioManager.SetMasterVolume(_currentSettings.masterVolume);
                _audioManager.SetMusicVolume(_currentSettings.musicVolume);
                _audioManager.SetSFXVolume(_currentSettings.sfxVolume);
                _audioManager.SetVoiceVolume(_currentSettings.voiceVolume);
            }
        }

        public void ApplyGraphicsSettings()
        {
            QualitySettings.SetQualityLevel(_currentSettings.qualityLevel, true);
            QualitySettings.vSyncCount = _currentSettings.vsync ? 1 : 0;
            Application.targetFrameRate = _currentSettings.targetFramerate;

            if (Screen.currentResolution.width != _currentSettings.resolutionWidth ||
                Screen.currentResolution.height != _currentSettings.resolutionHeight)
            {
                Screen.SetResolution(
                    _currentSettings.resolutionWidth,
                    _currentSettings.resolutionHeight,
                    _currentSettings.fullscreen
                );
            }
        }

        public void ApplyAccessibilitySettings()
        {
            // Apply colorblind settings
            // Apply text scale
            // Apply subtitle settings
        }

        public void SetMasterVolume(float value)
        {
            _currentSettings.masterVolume = Mathf.Clamp01(value);
            ApplyAudioSettings();
            MarkDirty();
        }

        public void SetMusicVolume(float value)
        {
            _currentSettings.musicVolume = Mathf.Clamp01(value);
            ApplyAudioSettings();
            MarkDirty();
        }

        public void SetSFXVolume(float value)
        {
            _currentSettings.sfxVolume = Mathf.Clamp01(value);
            ApplyAudioSettings();
            MarkDirty();
        }

        public void SetQualityLevel(int level)
        {
            _currentSettings.qualityLevel = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
            ApplyGraphicsSettings();
            MarkDirty();
        }

        public void SetResolution(int width, int height, bool fullscreen)
        {
            _currentSettings.resolutionWidth = width;
            _currentSettings.resolutionHeight = height;
            _currentSettings.fullscreen = fullscreen;
            ApplyGraphicsSettings();
            MarkDirty();
        }

        public void SetSensitivity(float value)
        {
            _currentSettings.lookSensitivity = Mathf.Clamp(value, 0.1f, 20f);
            MarkDirty();
        }

        public void SetInvertY(bool invert)
        {
            _currentSettings.invertY = invert;
            MarkDirty();
        }

        public void SetColorblindMode(bool enabled, Data.ColorblindType type)
        {
            _currentSettings.colorblindMode = enabled;
            _currentSettings.colorblindType = type;
            ApplyAccessibilitySettings();
            MarkDirty();
        }

        private void MarkDirty()
        {
            var saveManager = ServiceLocator.Get<SaveManager>();
            if (saveManager != null)
            {
                saveManager.CurrentSave.settings = _currentSettings;
                saveManager.MarkDirty();
            }
        }
    }
}
