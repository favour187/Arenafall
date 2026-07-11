using UnityEngine;
using ArenaFall.Data;

namespace ArenaFall.Interfaces
{
    /// <summary>
    /// Interface for all weapon types.
    /// </summary>
    public interface IWeapon
    {
        /// <summary>Weapon display name.</summary>
        string WeaponName { get; }
        
        /// <summary>Weapon data reference.</summary>
        WeaponData Data { get; }
        
        /// <summary>Current ammo in magazine.</summary>
        int CurrentAmmo { get; }
        
        /// <summary>Total reserve ammo.</summary>
        int ReserveAmmo { get; }
        
        /// <summary>Current fire mode index.</summary>
        int CurrentFireMode { get; }
        
        /// <summary>Whether the weapon is currently firing.</summary>
        bool IsFiring { get; }
        
        /// <summary>Whether the weapon is reloading.</summary>
        bool IsReloading { get; }
        
        /// <summary>Whether the weapon is equipped and ready.</summary>
        bool IsReady { get; }

        /// <summary>
        /// Start firing the weapon.
        /// </summary>
        void StartFire();
        
        /// <summary>
        /// Stop firing the weapon.
        /// </summary>
        void StopFire();
        
        /// <summary>
        /// Reload the weapon.
        /// </summary>
        void Reload();
        
        /// <summary>
        /// Switch to next fire mode.
        /// </summary>
        void SwitchFireMode();
        
        /// <summary>
        /// Aim down sights.
        /// </summary>
        void StartAim();
        
        /// <summary>
        /// Stop aiming down sights.
        /// </summary>
        void StopAim();
        
        /// <summary>
        /// Equip the weapon (called when switching to this weapon).
        /// </summary>
        void Equip();
        
        /// <summary>
        /// Unequip the weapon (called when switching away).
        /// </summary>
        void Unequip();
        
        /// <summary>
        /// Add ammo to reserve.
        /// </summary>
        void AddAmmo(int amount);
        
        /// <summary>
        /// Get current weapon accuracy modifier.
        /// </summary>
        float GetAccuracy();
        
        /// <summary>
        /// Get current weapon damage modifier.
        /// </summary>
        float GetDamageModifier();
    }

    /// <summary>
    /// Fire mode types.
    /// </summary>
    public enum FireMode
    {
        Safety,
        Single,
        Burst,
        Auto
    }
}
