using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using ArenaFall.Core;
using ArenaFall.Events;

namespace ArenaFall.Managers
{
    /// <summary>
    /// Manages all audio playback, mixing, and dynamic music system.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Mixer")]
        [SerializeField] private AudioMixer _mainMixer;
        [SerializeField] private string _masterVolumeParam = "MasterVolume";
        [SerializeField] private string _musicVolumeParam = "MusicVolume";
        [SerializeField] private string _sfxVolumeParam = "SFXVolume";
        [SerializeField] private string _voiceVolumeParam = "VoiceVolume";

        [Header("Music")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _ambientSource;
        [SerializeField] private float _musicCrossfadeDuration = 2f;

        [Header("SFX")]
        [SerializeField] private int _sfxSourceCount = 16;
        [SerializeField] private AudioSource _sfxSourcePrefab;
        [SerializeField] private AudioSource _uiSource;

        [Header("Voice")]
        [SerializeField] private AudioSource _voiceSource;
        [SerializeField] private AudioSource _announcerSource;

        private AudioSource[] _sfxSources;
        private int _currentSfxSource;
        private readonly Dictionary<string, AudioClip> _cachedClips = new();
        private float _currentMusicVolume = 1f;

        // Music state
        private MusicState _currentMusicState = MusicState.Ambient;
        private float _combatIntensity = 0f;

        public static AudioManager Instance { get; private set; }
        public AudioMixer MainMixer => _mainMixer;

        private enum MusicState
        {
            Ambient,
            Exploration,
            Combat,
            Intense,
            Final
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register<AudioManager>(this);

            InitializeSFXSources();
        }

        private void Start()
        {
            EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventBus.Subscribe<PlayerEliminatedEvent>(OnPlayerEliminated);
        }

        private void InitializeSFXSources()
        {
            _sfxSources = new AudioSource[_sfxSourceCount];
            for (int i = 0; i < _sfxSourceCount; i++)
            {
                var source = Instantiate(_sfxSourcePrefab, transform);
                source.outputAudioMixerGroup = FindMixerGroup("SFX");
                _sfxSources[i] = source;
            }
        }

        private AudioMixerGroup FindMixerGroup(string name)
        {
            if (_mainMixer == null) return null;
            var groups = _mainMixer.FindMatchingGroups(name);
            return groups.Length > 0 ? groups[0] as AudioMixerGroup : null;
        }

        /// <summary>
        /// Play a sound effect at a world position.
        /// </summary>
        public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            var source = GetNextSFXSource();
            source.transform.position = position;
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.Play();
        }

        /// <summary>
        /// Play a sound effect attached to a transform.
        /// </summary>
        public void PlaySFXAttached(AudioClip clip, Transform parent, float volume = 1f, float pitch = 1f, bool loop = false)
        {
            if (clip == null) return;

            var source = GetNextSFXSource();
            source.transform.SetParent(parent);
            source.transform.localPosition = Vector3.zero;
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.loop = loop;
            source.Play();
        }

        /// <summary>
        /// Play a one-shot UI sound.
        /// </summary>
        public void PlayUISound(AudioClip clip, float volume = 1f)
        {
            if (clip == null || _uiSource == null) return;
            _uiSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Play a voice line.
        /// </summary>
        public void PlayVoiceLine(AudioClip clip, float volume = 1f)
        {
            if (clip == null || _voiceSource == null) return;
            _voiceSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Play an announcer line.
        /// </summary>
        public void PlayAnnouncer(AudioClip clip, float volume = 1f)
        {
            if (clip == null || _announcerSource == null) return;
            _announcerSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Set combat intensity for dynamic music (0-1).
        /// </summary>
        public void SetCombatIntensity(float intensity)
        {
            _combatIntensity = Mathf.Clamp01(intensity);
            UpdateMusicState();
        }

        private void UpdateMusicState()
        {
            MusicState newState;

            if (_combatIntensity >= 0.8f)
                newState = MusicState.Intense;
            else if (_combatIntensity >= 0.4f)
                newState = MusicState.Combat;
            else if (_combatIntensity >= 0.1f)
                newState = MusicState.Exploration;
            else
                newState = MusicState.Ambient;

            if (newState != _currentMusicState)
            {
                _currentMusicState = newState;
                Debug.Log($"[AudioManager] Music state: {newState}");
            }
        }

        private AudioSource GetNextSFXSource()
        {
            _currentSfxSource = (_currentSfxSource + 1) % _sfxSourceCount;
            return _sfxSources[_currentSfxSource];
        }

        /// <summary>
        /// Set volume levels from settings.
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            _mainMixer?.SetFloat(_masterVolumeParam, Mathf.Log10(Mathf.Clamp01(volume)) * 20f);
        }

        public void SetMusicVolume(float volume)
        {
            _mainMixer?.SetFloat(_musicVolumeParam, Mathf.Log10(Mathf.Clamp01(volume)) * 20f);
            _currentMusicVolume = volume;
        }

        public void SetSFXVolume(float volume)
        {
            _mainMixer?.SetFloat(_sfxVolumeParam, Mathf.Log10(Mathf.Clamp01(volume)) * 20f);
        }

        public void SetVoiceVolume(float volume)
        {
            _mainMixer?.SetFloat(_voiceVolumeParam, Mathf.Log10(Mathf.Clamp01(volume)) * 20f);
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            // Handle music transitions for game states
        }

        private void OnPlayerEliminated(PlayerEliminatedEvent evt)
        {
            // Play elimination sound
        }

        /// <summary>
        /// Play footsteps based on surface type.
        /// </summary>
        public void PlayFootstep(string surfaceType, Vector3 position, float volume = 0.5f)
        {
            // TODO: Load footstep sounds based on surface type
            // PlaySFX(footstepClip, position, volume);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventBus.Unsubscribe<PlayerEliminatedEvent>(OnPlayerEliminated);
        }
    }
}
