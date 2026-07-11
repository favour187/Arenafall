using System;
using System.Collections;
using UnityEngine;
using ArenaFall.Core;
using ArenaFall.Events;

namespace ArenaFall.Gameplay.Zone
{
    /// <summary>
    /// Manages the shrinking safe zone that forces players into closer combat.
    /// Implements multi-stage shrinking with damage outside the safe zone.
    /// </summary>
    public class SafeZone : MonoBehaviour
    {
        [Header("Zone Configuration")]
        [SerializeField] private int _totalStages = 5;
        [SerializeField] private float _initialRadius = 2000f;
        [SerializeField] private float _finalRadius = 50f;
        [SerializeField] private float _shrinkDuration = 180f;
        [SerializeField] private float _warningTime = 60f;

        [Header("Damage")]
        [SerializeField] private float _baseDamagePerSecond = 1f;
        [SerializeField] private float _damageIncreasePerStage = 1f;

        [Header("Visuals")]
        [SerializeField] private GameObject _zoneWallPrefab;
        [SerializeField] private Material _zoneMaterial;
        [SerializeField] private Color _warningColor = Color.yellow;
        [SerializeField] private Color _activeColor = Color.red;

        [Header("References")]
        [SerializeField] private Transform _zoneVisual;
        [SerializeField] private Light _zoneLight;

        // State
        private int _currentStage;
        private float _currentRadius;
        private float _targetRadius;
        private Vector3 _currentCenter;
        private Vector3 _targetCenter;
        private bool _isShrinking;
        private bool _isActive;
        private float _shrinkProgress;
        private float _warningTimer;
        private float _stageTimer;

        // Debug visualization
        [Header("Debug")]
        [SerializeField] private bool _showGizmos = true;

        // Properties
        public int CurrentStage => _currentStage;
        public float CurrentRadius => _currentRadius;
        public float TargetRadius => _targetRadius;
        public Vector3 CurrentCenter => _currentCenter;
        public bool IsShrinking => _isShrinking;
        public bool IsInFinalZone => _currentStage >= _totalStages;

        public static SafeZone Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeZone();
        }

        private void Update()
        {
            if (!_isActive) return;

            if (_isShrinking)
            {
                UpdateShrinking();
            }
            else
            {
                UpdateWarning();
            }

            UpdateZoneVisual();
        }

        private void InitializeZone()
        {
            _currentStage = 0;
            _currentRadius = _initialRadius;
            _currentCenter = Vector3.zero;
            _targetRadius = _initialRadius * 0.8f;
            _targetCenter = GenerateRandomPoint();
            _isActive = true;
            _warningTimer = _warningTime;

            UpdateZoneVisual();
            Debug.Log($"[SafeZone] Initialized: radius={_currentRadius}, center={_currentCenter}");
        }

        /// <summary>
        /// Start the next shrink stage.
        /// </summary>
        public void StartNextStage()
        {
            if (_currentStage >= _totalStages) return;

            _currentStage++;
            _isShrinking = true;
            _shrinkProgress = 0f;

            // Calculate target
            _targetCenter = GenerateRandomPoint();
            _targetRadius = CalculateTargetRadius();

            EventBus.Raise(new ZoneShrinkingEvent
            {
                Stage = _currentStage,
                TimeUntilClose = _shrinkDuration
            });

            Debug.Log($"[SafeZone] Stage {_currentStage}: Shrinking to radius {_targetRadius} at {_targetCenter}");
        }

        private void UpdateShrinking()
        {
            _shrinkProgress += Time.deltaTime / _shrinkDuration;
            _currentRadius = Mathf.Lerp(_currentRadius, _targetRadius, _shrinkProgress);
            _currentCenter = Vector3.Lerp(_currentCenter, _targetCenter, _shrinkProgress);

            if (_shrinkProgress >= 1f)
            {
                _isShrinking = false;
                _currentRadius = _targetRadius;
                _currentCenter = _targetCenter;
                _warningTimer = _warningTime;

                EventBus.Raise(new ZoneShrunkEvent
                {
                    Stage = _currentStage
                });

                Debug.Log($"[SafeZone] Stage {_currentStage} complete");

                // Check if more stages
                if (_currentStage >= _totalStages)
                {
                    _isActive = true; // Final zone stays
                }
            }
        }

        private void UpdateWarning()
        {
            _warningTimer -= Time.deltaTime;
            if (_warningTimer <= 0)
            {
                StartNextStage();
            }
        }

        /// <summary>
        /// Check if a position is inside the safe zone.
        /// </summary>
        public bool IsInSafeZone(Vector3 position)
        {
            Vector3 flatPos = new Vector3(position.x, 0, position.z);
            Vector3 flatCenter = new Vector3(_currentCenter.x, 0, _currentCenter.z);
            return Vector3.Distance(flatPos, flatCenter) <= _currentRadius;
        }

        /// <summary>
        /// Get the distance from a position to the safe zone edge.
        /// </summary>
        public float DistanceToZone(Vector3 position)
        {
            Vector3 flatPos = new Vector3(position.x, 0, position.z);
            Vector3 flatCenter = new Vector3(_currentCenter.x, 0, _currentCenter.z);
            float dist = Vector3.Distance(flatPos, flatCenter);
            return dist - _currentRadius;
        }

        /// <summary>
        /// Get the direction to the nearest safe zone edge.
        /// </summary>
        public Vector3 DirectionToZone(Vector3 position)
        {
            Vector3 flatPos = new Vector3(position.x, 0, position.z);
            Vector3 flatCenter = new Vector3(_currentCenter.x, 0, _currentCenter.z);
            return (flatCenter - flatPos).normalized;
        }

        /// <summary>
        /// Get the damage per second for being outside the zone.
        /// </summary>
        public float GetDamagePerSecond()
        {
            return _baseDamagePerSecond + (_currentStage * _damageIncreasePerStage);
        }

        /// <summary>
        /// Get a random point within the current safe zone.
        /// </summary>
        public Vector3 GetRandomPointInZone()
        {
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle * _currentRadius * 0.8f;
            return new Vector3(_currentCenter.x + randomDir.x, 0, _currentCenter.z + randomDir.y);
        }

        /// <summary>
        /// Get the zone progress (0-1) for UI display.
        /// </summary>
        public float GetZoneProgress()
        {
            return _isShrinking ? _shrinkProgress : 0f;
        }

        /// <summary>
        /// Generate a random target center within the current zone.
        /// </summary>
        private Vector3 GenerateRandomPoint()
        {
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float distance = UnityEngine.Random.Range(0f, _currentRadius * 0.5f);
            return new Vector3(
                _currentCenter.x + Mathf.Cos(angle) * distance,
                0,
                _currentCenter.z + Mathf.Sin(angle) * distance
            );
        }

        /// <summary>
        /// Calculate the target radius based on current stage.
        /// </summary>
        private float CalculateTargetRadius()
        {
            float t = (float)_currentStage / _totalStages;
            return Mathf.Lerp(_initialRadius * 0.6f, _finalRadius, t);
        }

        private void UpdateZoneVisual()
        {
            if (_zoneVisual != null)
            {
                float scale = _currentRadius * 2f / 10f; // Assuming base is 10 units
                _zoneVisual.localScale = new Vector3(scale, 1, scale);
                _zoneVisual.position = _currentCenter;
            }

            if (_zoneLight != null)
            {
                _zoneLight.color = _isShrinking ? _activeColor : _warningColor;
            }

            // Update zone wall material
            if (_zoneMaterial != null)
            {
                _zoneMaterial.SetColor("_Color", _isShrinking ? _activeColor : _warningColor);
                _zoneMaterial.SetVector("_Center", _currentCenter);
                _zoneMaterial.SetFloat("_Radius", _currentRadius);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showGizmos) return;

            // Current zone
            Gizmos.color = _isShrinking ? _activeColor : Color.green;
            Gizmos.DrawWireSphere(_currentCenter, _currentRadius);

            // Target zone
            if (_isShrinking)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_targetCenter, _targetRadius);
                Gizmos.DrawLine(_currentCenter, _targetCenter);
            }
        }
    }
}
