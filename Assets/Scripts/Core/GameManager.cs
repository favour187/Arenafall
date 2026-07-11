using UnityEngine;
using ArenaFall.Events;
using ArenaFall.Managers;

namespace ArenaFall.Core
{
    /// <summary>
    /// Main game manager that oversees the entire game lifecycle.
    /// Initializes all core systems and manages game state transitions.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private bool _initializeOnStart = true;
        [SerializeField] private GameObject[] _managerPrefabs;

        private GameState _currentState = GameState.Boot;
        private bool _initialized;

        public static GameManager Instance { get; private set; }
        public GameState CurrentState => _currentState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (_initializeOnStart)
            {
                InitializeGame();
            }
        }

        /// <summary>
        /// Initialize all game systems.
        /// </summary>
        public void InitializeGame()
        {
            if (_initialized) return;

            Debug.Log("[GameManager] Initializing Arena Fall...");

            // Initialize core services
            ServiceLocator.Initialize();
            ServiceLocator.Register<GameManager>(this);

            // Initialize EventBus (static, no registration needed)

            // Spawn manager prefabs
            if (_managerPrefabs != null)
            {
                foreach (var prefab in _managerPrefabs)
                {
                    if (prefab != null)
                    {
                        Instantiate(prefab, transform);
                    }
                }
            }

            _initialized = true;

            // Transition to main menu
            SetState(GameState.MainMenu);

            Debug.Log("[GameManager] Arena Fall initialized successfully!");
            
            EventBus.Raise(new GameStateChangedEvent 
            { 
                PreviousState = GameState.Boot, 
                NewState = GameState.MainMenu 
            });
        }

        /// <summary>
        /// Set the current game state.
        /// </summary>
        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;

            GameState previousState = _currentState;
            _currentState = newState;

            Debug.Log($"[GameManager] State changed: {previousState} -> {newState}");

            EventBus.Raise(new GameStateChangedEvent
            {
                PreviousState = previousState,
                NewState = newState
            });

            OnStateChanged(previousState, newState);
        }

        private void OnStateChanged(GameState previous, GameState current)
        {
            switch (current)
            {
                case GameState.MainMenu:
                    // Show main menu UI, load lobby scene
                    break;
                case GameState.Lobby:
                    // Setup lobby, invite friends
                    break;
                case GameState.Matchmaking:
                    // Start matchmaking
                    break;
                case GameState.Playing:
                    // Game is active
                    break;
                case GameState.Result:
                    // Show results
                    break;
            }
        }

        /// <summary>
        /// Start a match with the given parameters.
        /// </summary>
        public void StartMatch(int playerCount, string mapName)
        {
            SetState(GameState.Loading);
            // SceneLoader will handle the rest
        }

        /// <summary>
        /// End the current match.
        /// </summary>
        public void EndMatch()
        {
            SetState(GameState.Result);
        }

        /// <summary>
        /// Quit the application.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[GameManager] Quitting game...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Update()
        {
            // Global update loop for systems that need it
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_currentState == GameState.Playing)
                {
                    EventBus.Raise(new GamePausedEvent { IsPaused = true });
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Clear();
                EventBus.Clear();
                Instance = null;
            }
        }
    }
}
