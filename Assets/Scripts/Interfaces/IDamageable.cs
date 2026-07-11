using UnityEngine;

namespace ArenaFall.Interfaces
{
    /// <summary>
    /// Interface for objects that can take damage and be destroyed.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>Current health value.</summary>
        float CurrentHealth { get; }
        
        /// <summary>Maximum health value.</summary>
        float MaxHealth { get; }
        
        /// <summary>Current shield value.</summary>
        float CurrentShield { get; }
        
        /// <summary>Maximum shield value.</summary>
        float MaxShield { get; }
        
        /// <summary>Whether the object is alive.</summary>
        bool IsAlive { get; }
        
        /// <summary>Team ID for friendly fire detection.</summary>
        int TeamId { get; }

        /// <summary>
        /// Apply damage to this object.
        /// </summary>
        /// <param name="amount">Amount of damage to apply.</param>
        /// <param name="damageType">Type of damage.</param>
        /// <param name="source">GameObject that caused the damage.</param>
        /// <returns>Actual damage dealt after mitigation.</returns>
        float TakeDamage(float amount, DamageType damageType, GameObject source);
        
        /// <summary>
        /// Heal this object by a given amount.
        /// </summary>
        void Heal(float amount);
        
        /// <summary>
        /// Apply shield to this object.
        /// </summary>
        void ApplyShield(float amount);
        
        /// <summary>
        /// Kill this object instantly.
        /// </summary>
        void Kill(GameObject killer);
    }

    /// <summary>
    /// Types of damage in the game.
    /// </summary>
    public enum DamageType
    {
        Bullet,
        Melee,
        Explosion,
        Fire,
        Zone,
        Fall,
        Drowning,
        Vehicle
    }

    /// <summary>
    /// Damage information struct for event passing.
    /// </summary>
    public struct DamageInfo
    {
        public float Amount;
        public DamageType Type;
        public GameObject Source;
        public GameObject Instigator;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public bool IsHeadshot;
        public bool IsCritical;
        
        public DamageInfo(float amount, DamageType type, GameObject source, GameObject instigator)
        {
            Amount = amount;
            Type = type;
            Source = source;
            Instigator = instigator;
            HitPoint = Vector3.zero;
            HitNormal = Vector3.zero;
            IsHeadshot = false;
            IsCritical = false;
        }
    }
}
