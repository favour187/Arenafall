using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArenaFall.Core;
using ArenaFall.Events;
using ArenaFall.Managers;

namespace ArenaFall.UI.MainMenu
{
    /// <summary>
    /// Controls the main menu screen and all its interactions.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _playPanel;
        [SerializeField] private GameObject _shopPanel;
        [SerializeField] private GameObject _battlePassPanel;
        [SerializeField] private GameObject _settingsPanel;

        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _battlePassButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _trainingButton;
        [SerializeField] private Button _quitButton;

        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Slider _xpSlider;
        [SerializeField] private TextMeshProUGUI _creditsText;
        [SerializeField] private TextMeshProUGUI _premiumText;

        [Header("Animations")]
        [SerializeField] private Animator _mainAnimator;
        [SerializeField] private string _showTrigger = "Show";
        [SerializeField] private string _hideTrigger = "Hide";

        private SaveManager _saveManager;
        private bool _isInitialized;

        private void Awake()
        {
            _saveManager = ServiceLocator.Get<SaveManager>();
        }

        private void Start()
        {
            InitializeMenu();
            BindButtons();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void InitializeMenu()
        {
            if (_saveManager != null && _saveManager.CurrentSave != null)
            {
                var save = _saveManager.CurrentSave;

                if (_playerNameText != null)
                    _playerNameText.text = save.playerName;

                if (_levelText != null)
                    _levelText.text = $"Level {save.level}";

                if (_xpSlider != null)
                {
                    int xpForNext = SaveManager.GetXPForLevel(save.level);
                    _xpSlider.maxValue = xpForNext;
                    _xpSlider.value = save.xp;
                }

                if (_creditsText != null)
                    _creditsText.text = save.credits.ToString("N0");

                if (_premiumText != null)
                    _premiumText.text = save.premiumCurrency.ToString("N0");
            }

            ShowPanel(_mainPanel);
            _isInitialized = true;
        }

        private void BindButtons()
        {
            if (_playButton != null)
                _playButton.onClick.AddListener(OnPlayClicked);

            if (_shopButton != null)
                _shopButton.onClick.AddListener(OnShopClicked);

            if (_battlePassButton != null)
                _battlePassButton.onClick.AddListener(OnBattlePassClicked);

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsClicked);

            if (_trainingButton != null)
                _trainingButton.onClick.AddListener(OnTrainingClicked);

            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnPlayClicked()
        {
            // Show play mode selection
            if (_mainPanel != null) _mainPanel.SetActive(false);
            if (_playPanel != null) _playPanel.SetActive(true);
        }

        public void OnSoloClicked()
        {
            // Start matchmaking for solo
            var gameManager = ServiceLocator.Get<GameManager>();
            if (gameManager != null)
            {
                gameManager.SetState(GameState.Matchmaking);
                // SceneLoader would transition to matchmaking scene
            }
        }

        public void OnDuosClicked()
        {
            // Start matchmaking for duos
        }

        public void OnSquadsClicked()
        {
            // Start matchmaking for squads
        }

        private void OnShopClicked()
        {
            ShowPanel(_shopPanel);
        }

        private void OnBattlePassClicked()
        {
            ShowPanel(_battlePassPanel);
        }

        private void OnSettingsClicked()
        {
            ShowPanel(_settingsPanel);
        }

        private void OnTrainingClicked()
        {
            var sceneLoader = FindObjectOfType<SceneLoader>();
            if (sceneLoader != null)
            {
                sceneLoader.LoadScene("08_TrainingGround");
            }
        }

        private void OnQuitClicked()
        {
            var gameManager = ServiceLocator.Get<GameManager>();
            gameManager?.QuitGame();
        }

        /// <summary>
        /// Show a specific panel and hide others.
        /// </summary>
        public void ShowPanel(GameObject panel)
        {
            if (_mainPanel != null) _mainPanel.SetActive(panel == _mainPanel);
            if (_playPanel != null) _playPanel.SetActive(panel == _playPanel);
            if (_shopPanel != null) _shopPanel.SetActive(panel == _shopPanel);
            if (_battlePassPanel != null) _battlePassPanel.SetActive(panel == _battlePassPanel);
            if (_settingsPanel != null) _settingsPanel.SetActive(panel == _settingsPanel);

            // Trigger animation
            if (_mainAnimator != null && panel != null)
            {
                _mainAnimator.SetTrigger(_showTrigger);
            }
        }

        public void OnBackToMainMenu()
        {
            ShowPanel(_mainPanel);
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            // Handle transitions away from main menu
        }
    }
}
