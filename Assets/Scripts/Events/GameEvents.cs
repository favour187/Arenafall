using UnityEngine;
using ArenaFall.Core;
using ArenaFall.Interfaces;

namespace ArenaFall.Events
{
    // ── Game State Events ──
    
    public class GameStateChangedEvent : GameEvent
    {
        public GameState PreviousState { get; set; }
        public GameState NewState { get; set; }
    }

    public class GamePausedEvent : GameEvent
    {
        public bool IsPaused { get; set; }
    }

    // ── Match Events ──
    
    public class MatchStartedEvent : GameEvent
    {
        public int PlayerCount { get; set; }
        public string MapName { get; set; }
    }

    public class MatchEndedEvent : GameEvent
    {
        public string WinnerId { get; set; }
        public string WinnerName { get; set; }
        public int Placement { get; set; }
        public int Kills { get; set; }
        public int DamageDealt { get; set; }
        public float SurvivalTime { get; set; }
    }

    public class PlayerEliminatedEvent : GameEvent
    {
        public string PlayerId { get; set; }
        public string KillerId { get; set; }
        public string WeaponId { get; set; }
        public bool WasHeadshot { get; set; }
    }

    public class PlayerDownedEvent : GameEvent
    {
        public string PlayerId { get; set; }
        public string DownerId { get; set; }
    }

    public class PlayerRevivedEvent : GameEvent
    {
        public string PlayerId { get; set; }
        public string ReviverId { get; set; }
    }

    // ── Zone Events ──
    
    public class ZoneShrinkingEvent : GameEvent
    {
        public int Stage { get; set; }
        public float TimeUntilClose { get; set; }
    }

    public class ZoneShrunkEvent : GameEvent
    {
        public int Stage { get; set; }
    }

    // ── Player State Events ──
    
    public class PlayerHealthChangedEvent : GameEvent
    {
        public string PlayerId { get; set; }
        public float OldHealth { get; set; }
        public float NewHealth { get; set; }
        public float MaxHealth { get; set; }
    }

    public class PlayerShieldChangedEvent : GameEvent
    {
        public string PlayerId { get; set; }
        public float OldShield { get; set; }
        public float NewShield { get; set; }
        public float MaxShield { get; set; }
    }

    public class PlayerDiedEvent : GameEvent
    {
        public string PlayerId { get; set; }
        public string KillerId { get; set; }
        public DamageType DamageType { get; set; }
        public Vector3 DeathPosition { get; set; }
    }

    // ── Inventory Events ──
    
    public class InventoryChangedEvent : GameEvent
    {
        public string PlayerId { get; set; }
        public string ItemId { get; set; }
        public InventoryChangeType ChangeType { get; set; }
        public int Amount { get; set; }
    }

    public enum InventoryChangeType
    {
        Added,
        Removed,
        Used,
        Swapped
    }

    // ── Weapon Events ──
    
    public class WeaponFiredEvent : GameEvent
    {
        public string PlayerId { get; set; }
        public string WeaponId { get; set; }
        public Vector3 FirePoint { get; set; }
        public Vector3 AimPoint { get; set; }
        public bool IsAds { get; set; }
    }

    public class WeaponReloadedEvent : GameEvent
    {
        public string PlayerId { get; set; }
        public string WeaponId { get; set; }
        public bool WasEmpty { get; set; }
    }

    // ── Progression Events ──
    
    public class LevelUpEvent : GameEvent
    {
        public int NewLevel { get; set; }
        public int PreviousLevel { get; set; }
    }

    public class XPAddedEvent : GameEvent
    {
        public int Amount { get; set; }
        public int TotalXP { get; set; }
        public string Source { get; set; }
    }

    public class MissionCompletedEvent : GameEvent
    {
        public string MissionId { get; set; }
        public string MissionName { get; set; }
        public MissionReward[] Rewards { get; set; }
    }

    public class AchievementUnlockedEvent : GameEvent
    {
        public string AchievementId { get; set; }
        public string AchievementName { get; set; }
    }

    // ── Social Events ──
    
    public class PlayerJoinedLobbyEvent : GameEvent
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
    }

    public class PlayerLeftLobbyEvent : GameEvent
    {
        public string PlayerId { get; set; }
    }

    [System.Serializable]
    public struct MissionReward
    {
        public RewardType Type;
        public int Amount;
        public string ItemId;
    }

    public enum RewardType
    {
        XP,
        Credits,
        PremiumCurrency,
        Item,
        Skin,
        BattlePassXP
    }

    public enum GameState
    {
        Boot,
        Login,
        MainMenu,
        Lobby,
        Matchmaking,
        Loading,
        Playing,
        Spectating,
        Result,
        Settings
    }
}
