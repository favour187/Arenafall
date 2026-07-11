using UnityEngine;
using ArenaFall.Interfaces;

namespace ArenaFall.Data
{
    /// <summary>
    /// ScriptableObject data container for all item types.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "Arena Fall/Items/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("General")]
        public string itemId;
        public string itemName;
        [TextArea] public string description;
        public ItemCategory category;
        public ItemRarity rarity;
        public Sprite icon;
        public GameObject worldPrefab;
        public GameObject pickupPrefab;

        [Header("Stacking")]
        public int maxStack = 1;
        public bool canStack = false;

        [Header("Usage")]
        public bool isUsable = false;
        public float useTime = 1f;
        public bool consumeOnUse = true;

        [Header("Value")]
        public int baseValue = 0;
        public CurrencyType currencyType = CurrencyType.Credits;

        [Header("Audio")]
        public AudioClip pickupSound;
        public AudioClip useSound;
        public AudioClip dropSound;

        [Header("Visuals")]
        public Color rarityColor = Color.white;
        public Vector3 worldScale = Vector3.one;
    }

    /// <summary>
    /// Healing item specific data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewHealingItem", menuName = "Arena Fall/Items/Healing Item")]
    public class HealingItemData : ItemData
    {
        [Header("Healing Properties")]
        public float healthRestoreAmount = 25f;
        public float shieldRestoreAmount = 0f;
        public bool canRevive = false;
        public float interactionRange = 3f;
    }

    /// <summary>
    /// Armor item specific data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewArmor", menuName = "Arena Fall/Items/Armor")]
    public class ArmorData : ItemData
    {
        [Header("Armor Properties")]
        public float damageReduction = 0.2f;
        public float durability = 100f;
        public int armorTier = 1;
    }

    /// <summary>
    /// Backpack item specific data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBackpack", menuName = "Arena Fall/Items/Backpack")]
    public class BackpackData : ItemData
    {
        [Header("Backpack Properties")]
        public int extraSlots = 4;
        public int backpackTier = 1;
    }

    public enum CurrencyType
    {
        Credits,
        Premium,
        BattlePassXP
    }
}
