using UnityEngine;
using ArenaFall.Core;
using ArenaFall.Interfaces;
using ArenaFall.Managers;

namespace ArenaFall.Gameplay.Characters
{
    /// <summary>
    /// Controls player character movement, state, and physics.
    /// Handles running, walking, crouching, prone, jumping, sliding, and swimming.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CharacterHealth))]
    public class PlayerCharacterController : MonoBehaviour, IPlayerController
    {
        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 6f;
        [SerializeField] private float _sprintSpeed = 9f;
        [SerializeField] private float _crouchSpeed = 3f;
        [SerializeField] private float _proneSpeed = 1.5f;
        [SerializeField] private float _acceleration = 10f;
        [SerializeField] private float _deceleration = 10f;
        [SerializeField] private float _airControl = 0.3f;

        [Header("Jumping")]
        [SerializeField] private float _jumpHeight = 2f;
        [SerializeField] private float _gravity = -20f;
        [SerializeField] private int _maxJumps = 1;
        [SerializeField] private float _jumpBufferTime = 0.1f;

        [Header("Crouching")]
        [SerializeField] private float _crouchHeight = 1f;
        [SerializeField] private float _standingHeight = 2f;
        [SerializeField] private float _crouchTransitionSpeed = 8f;

        [Header("Sliding")]
        [SerializeField] private float _slideSpeed = 12f;
        [SerializeField] private float _slideDuration = 1.5f;
        [SerializeField] private float _slideCooldown = 1f;

        [Header("Vaulting")]
        [SerializeField] private float _vaultHeight = 1.5f;
        [SerializeField] private float _vaultDistance = 1f;
        [SerializeField] private LayerMask _vaultLayerMask;

        [Header("Swimming")]
        [SerializeField] private float _swimSpeed = 4f;
        [SerializeField] private float _swimSprintSpeed = 6f;
        [SerializeField] private float _buoyancy = 3f;

        [Header("Stamina")]
        [SerializeField] private float _maxStamina = 100f;
        [SerializeField] private float _staminaDrainRate = 15f;
        [SerializeField] private float _staminaRegenRate = 20f;
        [SerializeField] private float _staminaRegenDelay = 1f;

        // Components
        private CharacterController _charController;
        private CharacterHealth _health;
        private Transform _cameraTransform;
        private InputManager _input;

        // Movement state
        private Vector3 _moveVelocity;
        private Vector3 _verticalVelocity;
        private float _currentSpeed;
        private float _currentStamina;
        private int _jumpsRemaining;
        private float _jumpBufferTimer;
        private float _staminaRegenTimer;

        // State
        private MovementState _movementState = MovementState.Standing;
        private bool _isGrounded;
        private bool _isSliding;
        private bool _isVaulting;
        private bool _isSwimming;
        private float _slideTimer;
        private float _slideCooldownTimer;

        // Properties
        public Vector3 Position => transform.position;
        public Vector3 Velocity => _charController.velocity;
        public float CurrentSpeed => _currentSpeed;
        public float CurrentStamina => _currentStamina;
        public float MaxStamina => _maxStamina;
        public bool IsGrounded => _isGrounded;
        public bool IsMoving => _moveVelocity.magnitude > 0.1f;
        public bool IsSprinting => _input != null && _input.IsSprinting && _currentStamina > 0 && _isGrounded;
        public bool IsCrouching => _movementState == MovementState.Crouching;
        public MovementState State => _movementState;

        private void Awake()
        {
            _charController = GetComponent<CharacterController>();
            _health = GetComponent<CharacterHealth>();
            _input = ServiceLocator.Get<InputManager>() ?? InputManager.Instance;
            _currentStamina = _maxStamina;
            _jumpsRemaining = _maxJumps;
        }

        private void Start()
        {
            if (Camera.main != null)
                _cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (!_health.IsAlive) return;

            if (_input == null) _input = InputManager.Instance ?? ServiceLocator.Get<InputManager>();

            HandleInput();
            HandleMovement();
            HandleGravityAndJumping();
            HandleStamina();
            HandleCrouchTransition();
            ApplyMovement();
        }

        private void HandleInput()
        {
            if (_input == null) return;

            Vector2 moveInput = _input.MoveInput;
            Vector3 forward = _cameraTransform != null ? 
                Vector3.Scale(_cameraTransform.forward, new Vector3(1, 0, 1)).normalized : 
                transform.forward;
            Vector3 right = _cameraTransform != null ? 
                Vector3.Scale(_cameraTransform.right, new Vector3(1, 0, 1)).normalized : 
                transform.right;

            _moveVelocity = (forward * moveInput.y + right * moveInput.x).normalized;

            // Jump buffer
            if (_input.IsJumping)
            {
                _jumpBufferTimer = _jumpBufferTime;
            }
            else
            {
                _jumpBufferTimer -= Time.deltaTime;
            }

            // Slide input
            if (_input.IsCrouching && IsSprinting && _isGrounded && !_isSliding && _slideCooldownTimer <= 0)
            {
                StartSlide();
            }
        }

        private void HandleMovement()
        {
            // Determine target speed based on state
            float targetSpeed = _walkSpeed;

            if (_isSwimming)
            {
                targetSpeed = IsSprinting ? _swimSprintSpeed : _swimSpeed;
            }
            else if (_isSliding)
            {
                targetSpeed = _slideSpeed;
            }
            else if (_isVaulting)
            {
                targetSpeed = _walkSpeed;
            }
            else switch (_movementState)
            {
                case MovementState.Standing:
                    targetSpeed = IsSprinting ? _sprintSpeed : _walkSpeed;
                    break;
                case MovementState.Crouching:
                    targetSpeed = _crouchSpeed;
                    break;
                case MovementState.Prone:
                    targetSpeed = _proneSpeed;
                    break;
            }

            // Apply air control
            float control = _isGrounded ? 1f : _airControl;
            targetSpeed *= control;

            // Smooth acceleration/deceleration
            float accel = _isGrounded ? _acceleration : _acceleration * _airControl;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed * _moveVelocity.magnitude, 
                accel * Time.deltaTime);

            // Slide decay
            if (_isSliding)
            {
                _slideTimer -= Time.deltaTime;
                _currentSpeed = Mathf.Lerp(_slideSpeed, _walkSpeed, 1f - (_slideTimer / _slideDuration));
                if (_slideTimer <= 0)
                {
                    EndSlide();
                }
            }
        }

        private void HandleGravityAndJumping()
        {
            _isGrounded = _charController.isGrounded;

            if (_isGrounded && _verticalVelocity.y < 0)
            {
                _verticalVelocity.y = -2f;
                _jumpsRemaining = _maxJumps;
            }

            // Jump
            if (_jumpBufferTimer > 0 && _jumpsRemaining > 0 && !_isSwimming)
            {
                _jumpBufferTimer = 0;
                _verticalVelocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                _jumpsRemaining--;
            }

            // Swimming buoyancy
            if (_isSwimming)
            {
                _verticalVelocity.y = Mathf.Lerp(_verticalVelocity.y, _buoyancy, Time.deltaTime);
            }

            _verticalVelocity.y += _gravity * Time.deltaTime;
        }

        private void HandleStamina()
        {
            if (IsSprinting && _isGrounded)
            {
                _currentStamina -= _staminaDrainRate * Time.deltaTime;
                _staminaRegenTimer = _staminaRegenDelay;
            }
            else
            {
                if (_staminaRegenTimer > 0)
                {
                    _staminaRegenTimer -= Time.deltaTime;
                }
                else
                {
                    _currentStamina += _staminaRegenRate * Time.deltaTime;
                }
            }

            _currentStamina = Mathf.Clamp(_currentStamina, 0, _maxStamina);
        }

        private void HandleCrouchTransition()
        {
            float targetHeight = _movementState == MovementState.Crouching ? _crouchHeight : _standingHeight;
            float currentHeight = _charController.height;
            
            if (Mathf.Abs(currentHeight - targetHeight) > 0.01f)
            {
                _charController.height = Mathf.Lerp(currentHeight, targetHeight, 
                    _crouchTransitionSpeed * Time.deltaTime);
                
                // Adjust center position
                Vector3 center = _charController.center;
                center.y = _charController.height / 2f;
                _charController.center = center;
            }
        }

        private void ApplyMovement()
        {
            if (_moveVelocity.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(_moveVelocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 14f * Time.deltaTime);
            }
            else if (_cameraTransform != null && (_input != null && (_input.IsAiming || _input.IsFiring)))
            {
                Quaternion targetRot = Quaternion.Euler(0, _cameraTransform.eulerAngles.y, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 16f * Time.deltaTime);
            }

            Vector3 horizontalMove = _moveVelocity * _currentSpeed;
            Vector3 finalMove = horizontalMove + _verticalVelocity;
            _charController.Move(finalMove * Time.deltaTime);
        }

        private void StartSlide()
        {
            _isSliding = true;
            _slideTimer = _slideDuration;
            _movementState = MovementState.Crouching;
            _charController.height = _crouchHeight;
        }

        private void EndSlide()
        {
            _isSliding = false;
            _slideCooldownTimer = _slideCooldown;
            if (_input != null && _input.IsCrouching)
            {
                _movementState = MovementState.Crouching;
            }
            else
            {
                _movementState = MovementState.Standing;
            }
        }

        /// <summary>
        /// Attempt to vault over an obstacle.
        /// </summary>
        public bool TryVault()
        {
            if (_isVaulting) return false;

            RaycastHit hit;
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            
            if (Physics.Raycast(origin, transform.forward, out hit, _vaultDistance, _vaultLayerMask))
            {
                if (hit.collider.bounds.size.y <= _vaultHeight)
                {
                    StartCoroutine(VaultRoutine(hit));
                    return true;
                }
            }
            return false;
        }

        private System.Collections.IEnumerator VaultRoutine(RaycastHit hit)
        {
            _isVaulting = true;
            Vector3 startPos = transform.position;
            Vector3 endPos = hit.point + Vector3.up * hit.collider.bounds.size.y + transform.forward * 0.5f;
            float duration = 0.5f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            _isVaulting = false;
        }

        /// <summary>
        /// Set the movement state.
        /// </summary>
        public void SetMovementState(MovementState state)
        {
            _movementState = state;
        }

        /// <summary>
        /// Set swimming state.
        /// </summary>
        public void SetSwimming(bool swimming)
        {
            _isSwimming = swimming;
        }

        /// <summary>
        /// Teleport to a position.
        /// </summary>
        public void Teleport(Vector3 position)
        {
            _charController.enabled = false;
            transform.position = position;
            _charController.enabled = true;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // Handle water detection
            if (hit.gameObject.CompareTag("Water"))
            {
                _isSwimming = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                _isSwimming = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                _isSwimming = false;
            }
        }
    }

    public enum MovementState
    {
        Standing,
        Crouching,
        Prone,
        Sliding
    }

    public interface IPlayerController
    {
        Vector3 Position { get; }
        Vector3 Velocity { get; }
        float CurrentSpeed { get; }
        bool IsGrounded { get; }
        bool IsMoving { get; }
        bool IsSprinting { get; }
        MovementState State { get; }
    }
}
