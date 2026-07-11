using System.Collections.Generic;
using UnityEngine;

namespace ArenaFall.Data
{
    /// <summary>
    /// ScriptableObject defining loot tables for different loot tiers and zones.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLootTable", menuName = "Arena Fall/Loot/Loot Table")]
    public class LootTableData : ScriptableObject
    {
        [Header("General")]
        public string tableId;
        public string tableName;
        public LootZoneType zoneType;
        public LootTier tier;

        [Header("Entries")]
        public List<LootEntry> lootEntries = new();
        
        [Header("Spawning")]
        public int minSpawns = 1;
        public int maxSpawns = 3;
        public float spawnRadius = 2f;
        public bool spawnOnGround = true;

        [Header("Respawn")]
        public bool canRespawn = false;
        public float respawnTime = 60f;
    }

    [System.Serializable]
    public class LootEntry
    {
        public ItemData item;
        public int weight = 1;
        public int minAmount = 1;
        public int maxAmount = 1;
        public float spawnChance = 1.0f;

        // For weapons specifically
        public bool hasAttachments = false;
        public int minAttachments = 0;
        public int maxAttachments = 2;
        public List<AttachmentData> possibleAttachments = new();
    }

    public enum LootZoneType
    {
        LowTier,
        MediumTier,
        HighTier,
        GuaranteedSpawn,
        SupplyDrop,
        BossDrop
    }

    public enum LootTier
    {
        Tier1,
        Tier2,
        Tier3,
        Tier4,
        Tier5,
        Boss
    }
}
