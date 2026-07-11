using UnityEngine;
using ArenaFall.Core;
using ArenaFall.Interfaces;
using ArenaFall.Gameplay.Characters;

namespace ArenaFall.Gameplay.Vehicles
{
    /// <summary>
    /// Controls vehicle movement, physics, and passenger management.
    /// Supports different vehicle types with unique handling.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour, IVehicle
    {
        [Header("Vehicle Settings")]
        [SerializeField] private string _vehicleId;
        [SerializeField] private string _vehicleName;
        [SerializeField] private VehicleType _vehicleType = VehicleType.Ground;
        [SerializeField] private float _maxSpeed = 30f;
        [SerializeField] private float _acceleration = 10f;
        [SerializeField] private float _brakingPower = 20f;
        [SerializeField] private float _turnSpeed = 90f;
        [SerializeField] private float _health = 500f;

        [Header("Seats")]
        [SerializeField] private Transform[] _seatPoints;
        [SerializeField] private int _driverSeatIndex = 0;

        [Header("Wheels (Ground)")]
        [SerializeField] private WheelCollider[] _wheelColliders;
        [SerializeField] private Transform[] _wheelMeshes;
        [SerializeField] private float _motorTorque = 1000f;
        [SerializeField] private float _steerAngle = 30f;

        [Header("Hover")]
        [SerializeField] private float _hoverHeight = 2f;
        [SerializeField] private float _hoverForce = 1000f;
        [SerializeField] private float _hoverDamping = 100f;

        [Header("Effects")]
        [SerializeField] private GameObject _explosionEffect;
        [SerializeField] private Light[] _headlights;
        [SerializeField] private AudioSource _engineSound;
        [SerializeField] private AudioClip _honkSound;

        // Components
        private Rigidbody _rb;
        private CharacterHealth _healthComponent;
        private Transform[] _passengers;
        private int _currentPassengerCount;
        private float _currentSpeed;
        private float _currentSteer;
        private float _currentThrottle;
        private bool _engineRunning;
        private bool _isOccupied;

        // Properties
        public string VehicleId => _vehicleId;
        public string VehicleName => _vehicleName;
        public VehicleType Type => _vehicleType;
        public float CurrentSpeed => _currentSpeed;
        public float MaxSpeed => _maxSpeed;
        public bool IsOccupied => _isOccupied;
        public int PassengerCount => _currentPassengerCount;
        public int MaxPassengers => _seatPoints.Length;
        public float Health => _health;
        public float MaxHealth => _health;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _passengers = new Transform[_seatPoints.Length];
            _healthComponent = gameObject.AddComponent<CharacterHealth>();
        }

        private void Start()
        {
            _engineRunning = true;
        }

        private void Update()
        {
            UpdateEngineSound();
            UpdateEffects();
        }

        private void FixedUpdate()
        {
            if (_driverSeatIndex >= _passengers.Length || _passengers[_driverSeatIndex] == null)
            {
                _isOccupied = false;
                return;
            }

            _isOccupied = true;

            switch (_vehicleType)
            {
                case VehicleType.Ground:
                    HandleGroundVehicle();
                    break;
                case VehicleType.Hover:
                    HandleHoverVehicle();
                    break;
                case VehicleType.Amphibious:
                    HandleAmphibiousVehicle();
                    break;
            }
        }

        private void HandleGroundVehicle()
        {
            // Get input from driver
            var driver = _passengers[_driverSeatIndex];
            if (driver == null) return;

            // Accelerate
            if (_currentThrottle > 0)
            {
                foreach (var wheel in _wheelColliders)
                {
                    wheel.motorTorque = _currentThrottle * _motorTorque;
                }
            }
            else
            {
                foreach (var wheel in _wheelColliders)
                {
                    wheel.motorTorque = 0;
                }
            }

            // Brake
            float brakeTorque = _currentThrottle < 0 ? _brakingPower : 0;
            foreach (var wheel in _wheelColliders)
            {
                wheel.brakeTorque = brakeTorque;
            }

            // Steer
            foreach (var wheel in _wheelColliders)
            {
                if (wheel.transform.localPosition.z > 0) // Front wheels
                {
                    wheel.steerAngle = _currentSteer * _steerAngle;
                }
            }

            // Update wheel meshes
            for (int i = 0; i < _wheelColliders.Length && i < _wheelMeshes.Length; i++)
            {
                Vector3 pos;
                Quaternion rot;
                _wheelColliders[i].GetWorldPose(out pos, out rot);
                _wheelMeshes[i].position = pos;
                _wheelMeshes[i].rotation = rot;
            }

            _currentSpeed = _rb.velocity.magnitude;
        }

        private void HandleHoverVehicle()
        {
            // Hover physics
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -Vector3.up, out hit, _hoverHeight * 2f))
            {
                float distance = hit.distance;
                float force = (_hoverHeight - distance) * _hoverForce - _rb.velocity.y * _hoverDamping;
                _rb.AddForce(Vector3.up * force);
            }

            // Movement
            Vector3 moveForce = transform.forward * _currentThrottle * _acceleration;
            _rb.AddForce(moveForce);

            // Rotation
            _rb.AddTorque(transform.up * _currentSteer * _turnSpeed * 0.1f);

            // Limit speed
            if (_rb.velocity.magnitude > _maxSpeed)
            {
                _rb.velocity = _rb.velocity.normalized * _maxSpeed;
            }

            _currentSpeed = _rb.velocity.magnitude;
        }

        private void HandleAmphibiousVehicle()
        {
            bool inWater = CheckInWater();
            if (inWater)
            {
                // Swimming physics
                Vector3 moveForce = transform.forward * _currentThrottle * _acceleration * 0.5f;
                _rb.AddForce(moveForce);
                _rb.AddTorque(transform.up * _currentSteer * _turnSpeed * 0.05f);
                
                // Buoyancy
                _rb.AddForce(Vector3.up * 9.81f * _rb.mass);
            }
            else
            {
                HandleGroundVehicle();
            }
        }

        private bool CheckInWater()
        {
            return Physics.Raycast(transform.position, Vector3.up, 0.5f, LayerMask.GetMask("Water"));
        }

        /// <summary>
        /// Try to enter the vehicle.
        /// </summary>
        public bool EnterVehicle(Transform player, int preferredSeat = -1)
        {
            if (_currentPassengerCount >= _seatPoints.Length) return false;

            int seatIndex = preferredSeat >= 0 && preferredSeat < _seatPoints.Length && 
                           _passengers[preferredSeat] == null ? 
                           preferredSeat : FindEmptySeat();

            if (seatIndex < 0) return false;

            _passengers[seatIndex] = player;
            player.SetParent(_seatPoints[seatIndex]);
            player.localPosition = Vector3.zero;
            player.localRotation = Quaternion.identity;
            _currentPassengerCount++;

            // Disable player movement
            var controller = player.GetComponent<PlayerCharacterController>();
            if (controller != null) controller.enabled = false;

            // Enable vehicle controls if driver
            if (seatIndex == _driverSeatIndex)
            {
                _isOccupied = true;
            }

            Debug.Log($"[Vehicle] {player.name} entered seat {seatIndex}");
            return true;
        }

        /// <summary>
        /// Exit the vehicle from a specific seat.
        /// </summary>
        public void ExitVehicle(Transform player)
        {
            for (int i = 0; i < _passengers.Length; i++)
            {
                if (_passengers[i] == player)
                {
                    _passengers[i] = null;
                    player.SetParent(null);
                    player.position = transform.position + transform.right * 2f + Vector3.up;
                    _currentPassengerCount--;

                    // Enable player movement
                    var controller = player.GetComponent<PlayerCharacterController>();
                    if (controller != null) controller.enabled = true;

                    if (i == _driverSeatIndex)
                    {
                        _isOccupied = false;
                        // Transfer driver to next passenger
                        for (int j = 0; j < _passengers.Length; j++)
                        {
                            if (_passengers[j] != null)
                            {
                                _passengers[_driverSeatIndex] = _passengers[j];
                                _passengers[j] = null;
                                _passengers[_driverSeatIndex].SetParent(_seatPoints[_driverSeatIndex]);
                                _passengers[_driverSeatIndex].localPosition = Vector3.zero;
                                break;
                            }
                        }
                    }

                    Debug.Log($"[Vehicle] {player.name} exited vehicle");
                    return;
                }
            }
        }

        /// <summary>
        /// Set vehicle input (called by driver).
        /// </summary>
        public void SetInput(float throttle, float steer)
        {
            _currentThrottle = Mathf.Clamp(throttle, -1f, 1f);
            _currentSteer = Mathf.Clamp(steer, -1f, 1f);
        }

        /// <summary>
        /// Honk the horn.
        /// </summary>
        public void Honk()
        {
            if (_honkSound != null && _engineSound != null)
            {
                _engineSound.PlayOneShot(_honkSound);
            }
        }

        private int FindEmptySeat()
        {
            for (int i = 0; i < _passengers.Length; i++)
            {
                if (_passengers[i] == null) return i;
            }
            return -1;
        }

        private void UpdateEngineSound()
        {
            if (_engineSound == null) return;
            float speedPercent = _currentSpeed / _maxSpeed;
            _engineSound.pitch = Mathf.Lerp(0.5f, 1.5f, speedPercent);
            _engineSound.volume = Mathf.Lerp(0.3f, 1f, speedPercent);
        }

        private void UpdateEffects()
        {
            if (_headlights != null)
            {
                foreach (var light in _headlights)
                {
                    light.enabled = _isOccupied && _engineRunning;
                }
            }
        }

        /// <summary>
        /// Apply damage to the vehicle.
        /// </summary>
        public void TakeDamage(float amount)
        {
            _health -= amount;
            if (_health <= 0)
            {
                DestroyVehicle();
            }
        }

        private void DestroyVehicle()
        {
            if (_explosionEffect != null)
            {
                Instantiate(_explosionEffect, transform.position, Quaternion.identity);
            }

            // Eject all passengers
            for (int i = _passengers.Length - 1; i >= 0; i--)
            {
                if (_passengers[i] != null)
                {
                    ExitVehicle(_passengers[i]);
                }
            }

            Destroy(gameObject, 0.5f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            if (_seatPoints != null)
            {
                foreach (var seat in _seatPoints)
                {
                    if (seat != null)
                    {
                        Gizmos.DrawWireSphere(seat.position, 0.3f);
                    }
                }
            }
        }
    }

    public enum VehicleType
    {
        Ground,
        Water,
        Hover,
        Amphibious
    }

    public interface IVehicle
    {
        string VehicleId { get; }
        string VehicleName { get; }
        VehicleType Type { get; }
        float CurrentSpeed { get; }
        float MaxSpeed { get; }
        bool IsOccupied { get; }
        int PassengerCount { get; }
        int MaxPassengers { get; }

        bool EnterVehicle(Transform player, int seat = -1);
        void ExitVehicle(Transform player);
        void SetInput(float throttle, float steer);
    }
}
