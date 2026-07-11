using UnityEngine;
using ArenaFall.Core;
using Cinemachine;

namespace ArenaFall.Managers
{
    /// <summary>
    /// Manages camera behavior including third-person follow, aiming, and transitions.
    /// Uses Cinemachine for smooth camera movement.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera _followCamera;
        [SerializeField] private CinemachineVirtualCamera _aimCamera;
        [SerializeField] private CinemachineVirtualCamera _spectatorCamera;
        [SerializeField] private Camera _mainCamera;

        [Header("Third Person Settings")]
        [SerializeField] private float _followDistance = 4f;
        [SerializeField] private float _followHeight = 2f;
        [SerializeField] private float _cameraSensitivity = 5f;
        [SerializeField] private Vector2 _verticalLookLimits = new Vector2(-30f, 60f);

        [Header("Aiming Settings")]
        [SerializeField] private float _aimFOV = 55f;
        [SerializeField] private float _normalFOV = 70f;
        [SerializeField] private float _fovTransitionSpeed = 8f;

        [Header("Effects")]
        [SerializeField] private float _landingBob = 0.1f;
        [SerializeField] private float _sprintFOVIncrease = 5f;

        // State
        private Transform _target;
        private Vector2 _lookAngles;
        private float _currentFOV;
        private bool _isAiming;
        private bool _isSprinting;

        public static CameraManager Instance { get; private set; }
        public Camera MainCamera => _mainCamera;
        public Vector2 LookAngles => _lookAngles;
        public float CurrentFOV => _currentFOV;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register<CameraManager>(this);

            if (_mainCamera == null)
                _mainCamera = Camera.main;

            _currentFOV = _normalFOV;
        }

        private void Start()
        {
            if (_mainCamera != null)
            {
                _mainCamera.fieldOfView = _normalFOV;
            }
        }

        private void Update()
        {
            HandleFOVTransition();
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            UpdateFollowCamera();
        }

        /// <summary>
        /// Set the target for the camera to follow.
        /// </summary>
        public void SetTarget(Transform target)
        {
            _target = target;

            if (_followCamera != null)
            {
                _followCamera.Follow = target;
                _followCamera.LookAt = target;
            }

            if (_aimCamera != null)
            {
                _aimCamera.Follow = target;
                _aimCamera.LookAt = target;
            }

            // Reset look angles
            if (target != null)
            {
                _lookAngles = target.eulerAngles;
            }
        }

        /// <summary>
        /// Set aiming state for FOV transition.
        /// </summary>
        public void SetAiming(bool aiming)
        {
            _isAiming = aiming;
        }

        /// <summary>
        /// Set sprinting state for FOV change.
        /// </summary>
        public void SetSprinting(bool sprinting)
        {
            _isSprinting = sprinting;
        }

        /// <summary>
        /// Add look input to camera rotation.
        /// </summary>
        public void AddLookInput(Vector2 lookInput)
        {
            _lookAngles.x += lookInput.x * _cameraSensitivity;
            _lookAngles.y += lookInput.y * _cameraSensitivity;
            _lookAngles.y = Mathf.Clamp(_lookAngles.y, _verticalLookLimits.x, _verticalLookLimits.y);
        }

        /// <summary>
        /// Set look angles directly.
        /// </summary>
        public void SetLookAngles(Vector2 angles)
        {
            _lookAngles = angles;
            _lookAngles.y = Mathf.Clamp(_lookAngles.y, _verticalLookLimits.x, _verticalLookLimits.y);
        }

        /// <summary>
        /// Get the forward direction of the camera.
        /// </summary>
        public Vector3 GetCameraForward()
        {
            return _mainCamera != null ? _mainCamera.transform.forward : Vector3.forward;
        }

        /// <summary>
        /// Get the right direction of the camera.
        /// </summary>
        public Vector3 GetCameraRight()
        {
            return _mainCamera != null ? _mainCamera.transform.right : Vector3.right;
        }

        private void UpdateFollowCamera()
        {
            if (_target == null) return;

            // Calculate camera position relative to target
            Quaternion rotation = Quaternion.Euler(_lookAngles.y, _lookAngles.x, 0);
            Vector3 offset = rotation * new Vector3(0, 0, -_followDistance);
            Vector3 targetPosition = _target.position + Vector3.up * _followHeight + offset;

            // Check for obstacles
            RaycastHit hit;
            if (Physics.Raycast(_target.position + Vector3.up * _followHeight, offset.normalized, out hit, _followDistance))
            {
                targetPosition = hit.point - offset.normalized * 0.3f;
            }

            if (_followCamera != null)
            {
                _followCamera.transform.position = targetPosition;
                _followCamera.transform.LookAt(_target.position + Vector3.up * _followHeight);
            }
        }

        private void HandleFOVTransition()
        {
            float targetFOV = _normalFOV;

            if (_isAiming)
                targetFOV = _aimFOV;
            else if (_isSprinting)
                targetFOV = _normalFOV + _sprintFOVIncrease;

            _currentFOV = Mathf.Lerp(_currentFOV, targetFOV, _fovTransitionSpeed * Time.deltaTime);

            if (_mainCamera != null)
            {
                _mainCamera.fieldOfView = _currentFOV;
            }
        }

        /// <summary>
        /// Switch to aiming camera mode.
        /// </summary>
        public void SwitchToAimCamera()
        {
            if (_followCamera != null) _followCamera.Priority = 0;
            if (_aimCamera != null) _aimCamera.Priority = 10;
            _isAiming = true;
        }

        /// <summary>
        /// Switch to follow camera mode.
        /// </summary>
        public void SwitchToFollowCamera()
        {
            if (_aimCamera != null) _aimCamera.Priority = 0;
            if (_followCamera != null) _followCamera.Priority = 10;
            _isAiming = false;
        }

        /// <summary>
        /// Switch to spectator camera.
        /// </summary>
        public void SwitchToSpectatorCamera(Transform target = null)
        {
            if (_spectatorCamera != null)
            {
                _spectatorCamera.Priority = 20;
                if (target != null)
                {
                    _spectatorCamera.Follow = target;
                    _spectatorCamera.LookAt = target;
                }
            }
        }

        /// <summary>
        /// Shake the camera for impact.
        /// </summary>
        public void ShakeCamera(float intensity, float duration)
        {
            // Camera shake logic
            StartCoroutine(ShakeRoutine(intensity, duration));
        }

        private System.Collections.IEnumerator ShakeRoutine(float intensity, float duration)
        {
            Vector3 originalPos = _mainCamera.transform.localPosition;
            float elapsed = 0;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;
                _mainCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _mainCamera.transform.localPosition = originalPos;
        }
    }
}
