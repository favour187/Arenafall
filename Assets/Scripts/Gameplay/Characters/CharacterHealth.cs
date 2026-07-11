using UnityEngine;
using ArenaFall.Core;
using ArenaFall.Events;
using ArenaFall.Interfaces;

namespace ArenaFall.Gameplay.Characters
{
    /// <summary>
    /// Manages character health, shields, damage, and death.
    /// Implements IDamageable for universal damage system.
    /// </summary>
    [RequireComponent(typeof(PlayerCharacterController))]
    public class CharacterHealth : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _maxShield = 0f;

        [Header("Armor")]
        [SerializeField] private float _armorDamageReduction = 0f;
        [SerializeField] private float _armorDurability = 0f;

        [Header("Regeneration")]
        [SerializeField] private bool _canRegenerate = false;
        [SerializeField] private float _regenDelay = 5f;
        [SerializeField] private float _regenRate = 5f;

        [Header("Death")]
        [SerializeField] private GameObject _deathEffectPrefab;
        [SerializeField] private float _ragdollForce = 500f;

        [Header("Audio")]
        [SerializeField] private AudioClip _hurtSound;
        [SerializeField] private AudioClip _deathSound;

        // State
        private float _currentHealth;
        private float _currentShield;
        private float _currentArmorDurability;
        private float _regenTimer;
        private bool _isAlive = true;

        private PlayerCharacterController _controller;
        private int _teamId;
        private string _playerId;

        // Events
        public System.Action<float, float> OnHealthChanged;
        public System.Action<float, float> OnShieldChanged;
        public System.Action<DamageInfo> OnDamaged;
        public System.Action<GameObject> OnDied;

        // Properties
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public float CurrentShield => _currentShield;
        public float MaxShield => _maxShield;
        public bool IsAlive => _isAlive;
        public int TeamId => _teamId;
        public string PlayerId => _playerId;
        public float HealthPercent => _currentHealth / _maxHealth;
        public float ShieldPercent => _maxShield > 0 ? _currentShield / _maxShield : 0f;

        private void Awake()
        {
            _controller = GetComponent<PlayerCharacterController>();
            _currentHealth = _maxHealth;
            _currentShield = 0f;
            _currentArmorDurability = _armorDurability;
        }

        private void Update()
        {
            if (!_isAlive) return;

            // Health regeneration
            if (_canRegenerate && _currentHealth < _maxHealth)
            {
                if (_regenTimer > 0)
                {
                    _regenTimer -= Time.deltaTime;
                }
                else
                {
                    float regenAmount = _regenRate * Time.deltaTime;
                    _currentHealth = Mathf.Min(_currentHealth + regenAmount, _maxHealth);
                    OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
                }
            }
        }

        /// <summary>
        /// Apply damage to this character.
        /// </summary>
        public float TakeDamage(float amount, DamageType damageType, GameObject source)
        {
            if (!_isAlive) return 0f;

            // Apply armor reduction
            float damageReduction = _armorDamageReduction;
            if (_currentArmorDurability > 0)
            {
                float armorAbsorb = amount * damageReduction;
                _currentArmorDurability -= armorAbsorb;
                amount -= armorAbsorb;
                _currentArmorDurability = Mathf.Max(0, _currentArmorDurability);
            }

            // Apply shield first
            if (_currentShield > 0)
            {
                float shieldDamage = Mathf.Min(amount, _currentShield);
                _currentShield -= shieldDamage;
                amount -= shieldDamage;
                OnShieldChanged?.Invoke(_currentShield, _maxShield);
            }

            // Apply remaining damage to health
            float actualDamage = Mathf.Min(amount, _currentHealth);
            _currentHealth -= actualDamage;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            // Create damage info
            DamageInfo damageInfo = new DamageInfo(actualDamage, damageType, source, gameObject);

            // Trigger events
            _regenTimer = _regenDelay;
            OnDamaged?.Invoke(damageInfo);

            // Notify event bus
            EventBus.Raise(new PlayerHealthChangedEvent
            {
                PlayerId = _playerId,
                OldHealth = _currentHealth + actualDamage,
                NewHealth = _currentHealth,
                MaxHealth = _maxHealth
            });

            // Check for death
            if (_currentHealth <= 0)
            {
                Die(source);
            }

            return actualDamage;
        }

        /// <summary>
        /// Heal the character by a given amount.
        /// </summary>
        public void Heal(float amount)
        {
            if (!_isAlive) return;

            float oldHealth = _currentHealth;
            _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            EventBus.Raise(new PlayerHealthChangedEvent
            {
                PlayerId = _playerId,
                OldHealth = oldHealth,
                NewHealth = _currentHealth,
                MaxHealth = _maxHealth
            });
        }

        /// <summary>
        /// Apply shield to the character.
        /// </summary>
        public void ApplyShield(float amount)
        {
            if (!_isAlive) return;

            float oldShield = _currentShield;
            _currentShield = Mathf.Min(_currentShield + amount, _maxShield);
            OnShieldChanged?.Invoke(_currentShield, _maxShield);

            EventBus.Raise(new PlayerShieldChangedEvent
            {
                PlayerId = _playerId,
                OldShield = oldShield,
                NewShield = _currentShield,
                MaxShield = _maxShield
            });
        }

        /// <summary>
        /// Set the maximum shield value.
        /// </summary>
        public void SetMaxShield(float maxShield)
        {
            _maxShield = maxShield;
            _currentShield = Mathf.Min(_currentShield, _maxShield);
        }

        /// <summary>
        /// Set armor values.
        /// </summary>
        public void SetArmor(float damageReduction, float durability)
        {
            _armorDamageReduction = damageReduction;
            _armorDurability = durability;
            _currentArmorDurability = durability;
        }

        /// <summary>
        /// Kill the character instantly.
        /// </summary>
        public void Kill(GameObject killer)
        {
            Die(killer);
        }

        private void Die(GameObject killer)
        {
            if (!_isAlive) return;
            _isAlive = false;

            // Play effects
            if (_deathEffectPrefab != null)
            {
                Instantiate(_deathEffectPrefab, transform.position, Quaternion.identity);
            }

            // Play death sound
            if (_deathSound != null)
            {
                AudioSource.PlayClipAtPoint(_deathSound, transform.position);
            }

            // Notify event bus
            EventBus.Raise(new PlayerDiedEvent
            {
                PlayerId = _playerId,
                KillerId = killer != null ? killer.name : "Zone",
                DamageType = DamageType.Bullet,
                DeathPosition = transform.position
            });

            OnDied?.Invoke(killer);

            // Disable controller
            if (_controller != null)
            {
                _controller.enabled = false;
            }

            // Disable all colliders
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            Debug.Log($"[CharacterHealth] {gameObject.name} died");
        }

        /// <summary>
        /// Respawn the character.
        /// </summary>
        public void Respawn()
        {
            _currentHealth = _maxHealth;
            _currentShield = 0f;
            _isAlive = true;
            _regenTimer = _regenDelay;

            if (_controller != null)
            {
                _controller.enabled = true;
            }

            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = true;
            }

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnShieldChanged?.Invoke(_currentShield, _maxShield);
        }

        /// <summary>
        /// Initialize with player data.
        /// </summary>
        public void Initialize(string playerId, int teamId)
        {
            _playerId = playerId;
            _teamId = teamId;
            Respawn();
        }
    }
}
