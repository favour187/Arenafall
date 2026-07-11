using UnityEngine;
using ArenaFall.Data;

namespace ArenaFall.Interfaces
{
    /// <summary>
    /// Interface for all inventory items.
    /// </summary>
    public interface IItem
    {
        /// <summary>Unique item identifier.</summary>
        string ItemId { get; }
        
        /// <summary>Display name of the item.</summary>
        string ItemName { get; }
        
        /// <summary>Item data reference.</summary>
        ItemData Data { get; }
        
        /// <summary>Current stack count.</summary>
        int StackCount { get; }
        
        /// <summary>Maximum stack size.</summary>
        int MaxStack { get; }
        
        /// <summary>Item category.</summary>
        ItemCategory Category { get; }
        
        /// <summary>Item rarity tier.</summary>
        ItemRarity Rarity { get; }
        
        /// <summary>Icon for UI display.</summary>
        Sprite Icon { get; }

        /// <summary>
        /// Use this item.
        /// </summary>
        /// <returns>Whether the item was successfully used.</returns>
        bool Use(GameObject user);
        
        /// <summary>
        /// Pick up this item.
        /// </summary>
        void Pickup(GameObject picker);
        
        /// <summary>
        /// Drop this item.
        /// </summary>
        void Drop(Vector3 position);
        
        /// <summary>
        /// Combine stacks.
        /// </summary>
        int AddToStack(int amount);
        
        /// <summary>
        /// Remove from stack.
        /// </summary>
        int RemoveFromStack(int amount);
    }

    /// <summary>
    /// Item categories for inventory organization.
    /// </summary>
    public enum ItemCategory
    {
        Weapon,
        Ammo,
        Attachment,
        Healing,
        Armor,
        Throwable,
        Backpack,
        KeyItem,
        Cosmetic
    }

    /// <summary>
    /// Rarity tiers for item quality.
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
