using UnityEngine;
using UnityEngine.InputSystem;
using ArenaFall.Core;

namespace ArenaFall.Managers
{
    /// <summary>
    /// Manages input handling, action maps, and input profiles.
    /// Uses Unity's new Input System.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputActionAsset _inputActionAsset;

        private InputActionMap _gameplayMap;
        private InputActionMap _uiMap;
        private InputActionMap _menuMap;
        private string _currentMap = "Gameplay";

        public static InputManager Instance { get; private set; }

        // Exposed input values
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool IsJumping { get; private set; }
        public bool IsFiring { get; private set; }
        public bool IsAiming { get; private set; }
        public bool IsReloading { get; private set; }
        public float ScrollInput { get; private set; }
        public float Sensitivity { get; set; } = 5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register<InputManager>(this);

            InitializeInput();
        }

        private void InitializeInput()
        {
            if (_inputActionAsset == null)
            {
                Debug.LogError("[InputManager] No InputActionAsset assigned!");
                return;
            }

            _gameplayMap = _inputActionAsset.FindActionMap("Gameplay");
            _uiMap = _inputActionAsset.FindActionMap("UI");
            _menuMap = _inputActionAsset.FindActionMap("Menu");

            if (_gameplayMap == null)
            {
                _gameplayMap = _inputActionAsset.AddActionMap("Gameplay");
                SetupDefaultGameplayActions(_gameplayMap);
            }

            // Bind all gameplay input events
            BindGameplayInput();
        }

        private void SetupDefaultGameplayActions(InputActionMap map)
        {
            map.AddAction("Move", InputActionType.Value, "<Keyboard>/w", "<Keyboard>/s", "<Keyboard>/a", "<Keyboard>/d", "<Gamepad>/leftStick");
            map.AddAction("Look", InputActionType.PassThrough, "<Mouse>/delta", "<Gamepad>/rightStick");
            map.AddAction("Fire", InputActionType.Button, "<Mouse>/leftButton", "<Gamepad>/rightTrigger");
            map.AddAction("Aim", InputActionType.Button, "<Mouse>/rightButton", "<Gamepad>/leftTrigger");
            map.AddAction("Reload", InputActionType.Button, "<Keyboard>/r", "<Gamepad>/buttonEast");
            map.AddAction("Jump", InputActionType.Button, "<Keyboard>/space", "<Gamepad>/buttonSouth");
            map.AddAction("Crouch", InputActionType.Button, "<Keyboard>/c", "<Keyboard>/leftCtrl", "<Gamepad>/buttonNorth");
            map.AddAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift", "<Gamepad>/leftStickPress");
            map.AddAction("Interact", InputActionType.Button, "<Keyboard>/f", "<Gamepad>/buttonWest");
            map.AddAction("Inventory", InputActionType.Button, "<Keyboard>/tab", "<Gamepad>/start");
            map.AddAction("Map", InputActionType.Button, "<Keyboard>/m", "<Gamepad>/select");
            map.AddAction("Ping", InputActionType.Button, "<Keyboard>/mouse2", "<Gamepad>/dpadUp");
            map.AddAction("Scroll", InputActionType.Value, "<Mouse>/scroll", "<Gamepad>/dpad");
            map.AddAction("Slot1", InputActionType.Button, "<Keyboard>/1");
            map.AddAction("Slot2", InputActionType.Button, "<Keyboard>/2");
            map.AddAction("Slot3", InputActionType.Button, "<Keyboard>/3");
            map.AddAction("Slot4", InputActionType.Button, "<Keyboard>/4");
            map.AddAction("Slot5", InputActionType.Button, "<Keyboard>/5");
            map.AddAction("UseHeal", InputActionType.Button, "<Keyboard>/4", "<Gamepad>/dpadLeft");
        }

        private void BindGameplayInput()
        {
            if (_gameplayMap == null) return;

            _gameplayMap["Move"].performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            _gameplayMap["Move"].canceled += ctx => MoveInput = Vector2.zero;

            _gameplayMap["Look"].performed += ctx => LookInput = ctx.ReadValue<Vector2>() * (Sensitivity / 50f);
            _gameplayMap["Look"].canceled += ctx => LookInput = Vector2.zero;

            _gameplayMap["Fire"].performed += ctx => IsFiring = true;
            _gameplayMap["Fire"].canceled += ctx => IsFiring = false;

            _gameplayMap["Aim"].performed += ctx => IsAiming = true;
            _gameplayMap["Aim"].canceled += ctx => IsAiming = false;

            _gameplayMap["Sprint"].performed += ctx => IsSprinting = true;
            _gameplayMap["Sprint"].canceled += ctx => IsSprinting = false;

            _gameplayMap["Crouch"].performed += ctx => IsCrouching = !IsCrouching;

            _gameplayMap["Jump"].performed += ctx => IsJumping = true;
            _gameplayMap["Jump"].canceled += ctx => IsJumping = false;

            _gameplayMap["Reload"].performed += ctx => IsReloading = true;
            _gameplayMap["Reload"].canceled += ctx => IsReloading = false;

            _gameplayMap["Scroll"].performed += ctx => ScrollInput = ctx.ReadValue<float>();
            _gameplayMap["Scroll"].canceled += ctx => ScrollInput = 0f;
        }

        /// <summary>
        /// Switch to a specific input action map.
        /// </summary>
        public void SwitchToMap(string mapName)
        {
            _gameplayMap?.Disable();
            _uiMap?.Disable();
            _menuMap?.Disable();

            _currentMap = mapName;

            switch (mapName)
            {
                case "Gameplay":
                    _gameplayMap?.Enable();
                    break;
                case "UI":
                    _uiMap?.Enable();
                    break;
                case "Menu":
                    _menuMap?.Enable();
                    break;
            }
        }

        /// <summary>
        /// Enable or disable all input.
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            if (enabled)
                _gameplayMap?.Enable();
            else
                _gameplayMap?.Disable();
        }

        /// <summary>
        /// Update sensitivity value.
        /// </summary>
        public void UpdateSensitivity(float newSensitivity)
        {
            Sensitivity = Mathf.Clamp(newSensitivity, 0.1f, 20f);
        }

        private void OnDestroy()
        {
            if (_gameplayMap != null)
            {
                _gameplayMap["Move"].performed -= ctx => MoveInput = Vector2.zero;
                _gameplayMap["Look"].performed -= ctx => LookInput = Vector2.zero;
            }
        }
    }
}
