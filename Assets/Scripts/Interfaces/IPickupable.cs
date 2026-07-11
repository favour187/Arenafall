using UnityEngine;

namespace ArenaFall.Interfaces
{
    /// <summary>
    /// Interface for objects that can be picked up from the world.
    /// </summary>
    public interface IPickupable
    {
        /// <summary>Whether this can be picked up.</summary>
        bool CanPickup { get; }
        
        /// <summary>Interaction prompt text.</summary>
        string PickupPrompt { get; }
        
        /// <summary>Time in seconds to pick up.</summary>
        float PickupTime { get; }

        /// <summary>
        /// Called when a player interacts with this pickup.
        /// </summary>
        void OnInteract(GameObject interactor);
        
        /// <summary>
        /// Called when the pickup is picked up.
        /// </summary>
        void OnPickup(GameObject picker);
        
        /// <summary>
        /// Called when the pickup is dropped.
        /// </summary>
        void OnDrop();
        
        /// <summary>
        /// Highlight the pickup for visibility.
        /// </summary>
        void SetHighlight(bool highlighted);
        
        /// <summary>
        /// Show pickup prompt UI.
        /// </summary>
        void ShowPrompt(bool show);
    }
}
