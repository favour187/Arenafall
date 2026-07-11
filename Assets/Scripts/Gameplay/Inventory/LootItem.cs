using UnityEngine;
using ArenaFall.Core;
using ArenaFall.Data;
using ArenaFall.Interfaces;

namespace ArenaFall.Gameplay.Inventory
{
    /// <summary>
    /// World pickup item that can be collected by players.
    /// Handles visual representation, highlighting, and interaction.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class LootItem : MonoBehaviour, IPickupable
    {
        [Header("Loot Configuration")]
        [SerializeField] private ItemData _itemData;
        [SerializeField] private int _amount = 1;
        [SerializeField] private LootRarity _rarity;

        [Header("Visuals")]
        [SerializeField] private Light _glowLight;
        [SerializeField] private ParticleSystem _glowParticles;
        [SerializeField] private MeshRenderer _itemRenderer;
        [SerializeField] private float _rotationSpeed = 30f;
        [SerializeField] private float _bobSpeed = 1f;
        [SerializeField] private float _bobHeight = 0.2f;

        [Header("Interaction")]
        [SerializeField] private float _pickupRadius = 2f;
        [SerializeField] private float _pickupTime = 0f;
        [SerializeField] private LayerMask _pickupLayer;

        private Transform _visualTransform;
        private Vector3 _startPosition;
        private bool _isHighlighted;
        private bool _canPickup = true;
        private float _spawnTime;

        // IPickupable
        public bool CanPickup => _canPickup;
        public string PickupPrompt => _itemData != null ? $"Press F to pick up {_itemData.itemName}" : "Pick up";
        public float PickupTime => _pickupTime;

        public ItemData ItemData => _itemData;
        public int Amount => _amount;

        private enum LootRarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }

        private void Awake()
        {
            var collider = GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = _pickupRadius / 2f;

            _visualTransform = transform.GetChild(0);
            _startPosition = _visualTransform != null ? _visualTransform.localPosition : Vector3.zero;
            _spawnTime = Time.time;
        }

        private void Start()
        {
            UpdateRarityVisuals();
        }

        private void Update()
        {
            if (_visualTransform == null) return;

            // Float/bob animation
            float bob = Mathf.Sin((Time.time - _spawnTime) * _bobSpeed) * _bobHeight;
            Vector3 pos = _startPosition;
            pos.y += bob;
            _visualTransform.localPosition = pos;

            // Rotation
            _visualTransform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Initialize the loot item with data.
        /// </summary>
        public void Initialize(ItemData data, int amount = 1)
        {
            _itemData = data;
            _amount = amount;

            // Update visual
            if (_itemRenderer != null && data != null)
            {
                // Set material, scale, etc. based on data
            }

            UpdateRarityVisuals();
        }

        private void UpdateRarityVisuals()
        {
            if (_itemData == null) return;

            Color rarityColor = GetRarityColor();
            
            if (_glowLight != null)
            {
                _glowLight.color = rarityColor;
                _glowLight.intensity = _isHighlighted ? 2f : 0.5f;
            }

            if (_glowParticles != null)
            {
                var main = _glowParticles.main;
                main.startColor = rarityColor;
            }
        }

        private Color GetRarityColor()
        {
            return _itemData.rarity switch
            {
                ItemRarity.Common => Color.white,
                ItemRarity.Uncommon => new Color(0.3f, 0.8f, 0.3f),
                ItemRarity.Rare => new Color(0.3f, 0.5f, 1f),
                ItemRarity.Epic => new Color(0.8f, 0.3f, 0.8f),
                ItemRarity.Legendary => new Color(1f, 0.6f, 0f),
                _ => Color.white
            };
        }

        /// <summary>
        /// Called when a player interacts with this pickup.
        /// </summary>
        public void OnInteract(GameObject interactor)
        {
            if (!_canPickup || _itemData == null) return;

            var inventory = interactor.GetComponent<Inventory>();
            if (inventory != null)
            {
                // Create item from data
                IItem item = CreateItemFromData(_itemData, _amount);
                if (inventory.AddItem(item))
                {
                    OnPickup(interactor);
                }
            }
        }

        /// <summary>
        /// Called when the pickup is collected.
        /// </summary>
        public void OnPickup(GameObject picker)
        {
            _canPickup = false;

            // Play effects
            if (_glowParticles != null)
            {
                _glowParticles.Stop();
            }

            // Play sound
            if (_itemData != null && _itemData.pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(_itemData.pickupSound, transform.position);
            }

            // Destroy or pool
            Destroy(gameObject, 0.2f);
        }

        /// <summary>
        /// Called when the pickup is dropped.
        /// </summary>
        public void OnDrop()
        {
            _canPickup = true;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Highlight the pickup for visibility.
        /// </summary>
        public void SetHighlight(bool highlighted)
        {
            _isHighlighted = highlighted;
            UpdateRarityVisuals();
        }

        /// <summary>
        /// Show pickup prompt UI.
        /// </summary>
        public void ShowPrompt(bool show)
        {
            // Would trigger UI event
        }

        private IItem CreateItemFromData(ItemData data, int amount)
        {
            if (data is WeaponData weaponData)
            {
                return new WeaponItem(weaponData);
            }
            else if (data is HealingItemData healingData)
            {
                return new HealingItem(healingData, amount);
            }
            else
            {
                return new GenericItem(data, amount);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && _canPickup)
            {
                SetHighlight(true);
                ShowPrompt(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SetHighlight(false);
                ShowPrompt(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _pickupRadius);
        }
    }
}
