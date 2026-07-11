using System.Collections.Generic;
using ArenaFall.Data;

namespace ArenaFall.Interfaces
{
    /// <summary>
    /// Interface for inventory systems.
    /// </summary>
    public interface IInventory
    {
        /// <summary>List of items in inventory.</summary>
        IReadOnlyList<IItem> Items { get; }
        
        /// <summary>Total slots available.</summary>
        int TotalSlots { get; }
        
        /// <summary>Used slots count.</summary>
        int UsedSlots { get; }
        
        /// <summary>Remaining free slots.</summary>
        int FreeSlots { get; }
        
        /// <summary>Whether the inventory is full.</summary>
        bool IsFull { get; }

        /// <summary>
        /// Add an item to the inventory.
        /// </summary>
        bool AddItem(IItem item);
        
        /// <summary>
        /// Remove an item from the inventory.
        /// </summary>
        bool RemoveItem(string itemId, int amount = 1);
        
        /// <summary>
        /// Check if an item exists in inventory.
        /// </summary>
        bool HasItem(string itemId);
        
        /// <summary>
        /// Get count of a specific item.
        /// </summary>
        int GetItemCount(string itemId);
        
        /// <summary>
        /// Clear all items from inventory.
        /// </summary>
        void Clear();
    }
}
