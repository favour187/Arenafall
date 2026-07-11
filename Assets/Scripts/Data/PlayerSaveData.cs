using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArenaFall.Data
{
    /// <summary>
    /// Complete player save data structure.
    /// Used for both local saves and cloud sync.
    /// </summary>
    [System.Serializable]
    public class PlayerSaveData
    {
        // Player Identity
        public string playerId;
        public string playerName;
        public DateTime createdDate;
        public DateTime lastLoginDate;
        public DateTime lastMatchDate;

        // Progression
        public int level = 1;
        public int xp = 0;
        public int prestigeLevel = 0;
        public int totalXP = 0;

        // Currency
        public int credits = 0;
        public int premiumCurrency = 0;

        // Career Stats
        public PlayerStats stats = new();

        // Inventory & Cosmetics
        public List<string> ownedCharacters = new() { "default_vanguard" };
        public List<string> ownedWeaponSkins = new();
        public List<string> ownedEmotes = new();
        public List<string> ownedSkins = new();
        public List<string> ownedBanners = new();

        // Loadouts
        public List<PlayerLoadout> loadouts = new();

        // Mission Progress
        public List<MissionProgress> dailyMissions = new();
        public List<MissionProgress> weeklyMissions = new();
        public List<string> completedAchievements = new();

        // Battle Pass
        public BattlePassProgress battlePass = new();

        // Settings
        public PlayerSettings settings = new();

        // Match History (recent)
        public List<MatchRecord> recentMatches = new();
    }

    [System.Serializable]
    public class PlayerStats
    {
        // Career
        public int matchesPlayed;
        public int wins;
        public int top10Placements;
        public int top5Placements;

        // Combat
        public int kills;
        public int deaths;
        public int assists;
        public int damageDealt;
        public int damageTaken;
        public int headshots;
        public int longestKill;
        public int revives;
        public int vehiclesDestroyed;

        // Survival
        public float totalPlayTime;
        public float longestSurvivalTime;
        public float totalDistanceTraveled;

        // Weapons
        public Dictionary<string, WeaponStats> weaponStats = new();
    }

    [System.Serializable]
    public class WeaponStats
    {
        public string weaponId;
        public int kills;
        public int deaths;
        public int shotsFired;
        public int shotsHit;
        public int headshots;
        public float damageDealt;
        public float accuracy;
    }

    [System.Serializable]
    public class PlayerLoadout
    {
        public string loadoutName = "Default";
        public string characterId = "default_vanguard";
        public string primaryWeaponId;
        public string secondaryWeaponId;
        public string meleeWeaponId = "combat_knife";
        public string throwableId = "frag_grenade";
        public List<string> primaryAttachments = new();
        public List<string> secondaryAttachments = new();
    }

    [System.Serializable]
    public class MissionProgress
    {
        public string missionId;
        public int currentProgress;
        public int targetProgress;
        public bool isCompleted;
        public bool isClaimed;
        public DateTime expiresAt;
    }

    [System.Serializable]
    public class BattlePassProgress
    {
        public int currentTier = 1;
        public int currentXP = 0;
        public int totalXP = 0;
        public bool isPremium = false;
        public List<int> claimedTiers = new();
        public List<int> claimedPremiumTiers = new();
    }

    [System.Serializable]
    public class PlayerSettings
    {
        // Audio
        public float masterVolume = 0.8f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 0.8f;
        public float voiceVolume = 0.7f;

        // Graphics
        public int qualityLevel = 2;
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public bool fullscreen = true;
        public int targetFramerate = 60;
        public bool vsync = false;
        public float brightness = 1.0f;

        // Controls
        public float lookSensitivity = 5.0f;
        public float aimSensitivity = 3.0f;
        public bool invertY = false;
        public bool toggleCrouch = false;
        public bool toggleSprint = false;
        public bool toggleAim = false;

        // Accessibility
        public float textScale = 1.0f;
        public bool colorblindMode = false;
        public ColorblindType colorblindType = ColorblindType.None;
        public bool subtitlesEnabled = false;
        public bool screenShakeEnabled = true;

        // Network
        public string region = "auto";
        public bool crossplayEnabled = true;
    }

    public enum ColorblindType
    {
        None,
        Protanopia,
        Deuteranopia,
        Tritanopia
    }

    [System.Serializable]
    public class MatchRecord
    {
        public string matchId;
        public string mapName;
        public string mode;
        public DateTime date;
        public int placement;
        public int kills;
        public int deaths;
        public int assists;
        public int damageDealt;
        public float survivalTime;
        public bool isWin;
        public int xpEarned;
        public int creditsEarned;
    }
}
