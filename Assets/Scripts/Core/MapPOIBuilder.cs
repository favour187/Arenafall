using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArenaFall.Gameplay.Inventory;
using ArenaFall.Gameplay.Zone;

namespace ArenaFall.Core
{
    /// <summary>
    /// Constructs the complete 6-Compound Sci-Fi Battle Royale map architecture.
    /// Replaces flat terrain and primitive quads with multi-story compound POIs, vertical catwalks,
    /// defensive barriers, interior vaults, structured weapon racks, and high-res AI compound facades.
    /// </summary>
    public class MapPOIBuilder : MonoBehaviour
    {
        private static Dictionary<string, Texture2D> _poiTextureCache = new();

        public static void BuildFullBattleRoyaleMap(GameObject terrainRoot)
        {
            Debug.Log("[MapPOIBuilder] Constructing 6 Major Sci-Fi Compound POIs across the 4000x4000 Arena...");

            // Pre-load AI compound art for architectural facades & signage
            PreloadCompoundArt();

            // Compound 1: Nexus Tower (Central Hub - High Tier, Multi-story verticality)
            BuildNexusTowerCompound(new Vector3(2000, 0, 2000), terrainRoot.transform);

            // Compound 2: Industrial Factory (North-East - Catwalks & heavy cover warehouses)
            BuildIndustrialFactoryCompound(new Vector3(3200, 0, 3200), terrainRoot.transform);

            // Compound 3: Hydro Station (South-East - Water dam crossings & generator rooms)
            BuildHydroStationCompound(new Vector3(3200, 0, 800), terrainRoot.transform);

            // Compound 4: Frost Depots (North-West - Cryo bunkers & high density armories)
            BuildFrostDepotsCompound(new Vector3(800, 0, 3200), terrainRoot.transform);

            // Compound 5: Solar Fields (South-West - Wide open array panels & watchtowers)
            BuildSolarFieldsCompound(new Vector3(800, 0, 800), terrainRoot.transform);

            // Compound 6: Crash Site (Mid-West - Starship wreckage, crater cover & hazardous energy)
            BuildCrashSiteCompound(new Vector3(600, 0, 2000), terrainRoot.transform);

            // Build connecting highway network and tactical cover barriers between POIs
            BuildConnectingInfrastructure(terrainRoot.transform);

            Debug.Log("[MapPOIBuilder] ✓ 6 Compound POIs & infrastructure fully built with 3D architecture.");
        }

        private static void PreloadCompoundArt()
        {
            string[] names = { "nexus_tower", "industrial_factory", "hydro_station", "frost_depots", "solar_fields_landscape", "crash_site" };
            foreach (var n in names)
            {
                var tex = Resources.Load<Texture2D>($"Art/Buildings/{n}") ?? Resources.Load<Texture2D>($"Art/Environment/CrashSite/crash_site") ?? Resources.Load<Texture2D>($"Art/Environment/FrostDepots/frost_depots") ?? Resources.Load<Texture2D>($"Art/Environment/HydroStation/hydro_station") ?? Resources.Load<Texture2D>($"Art/Environment/SolarFields/solar_fields_landscape");
                if (tex != null) _poiTextureCache[n] = tex;
            }
        }

        // ─── 1. NEXUS TOWER COMPOUND (CENTER POI) ───────────────────
        private static void BuildNexusTowerCompound(Vector3 center, Transform parent)
        {
            var compound = new GameObject("[POI] Nexus Tower Compound");
            compound.transform.SetParent(parent);
            compound.transform.position = center;

            // Central Foundation Base (120x8x120m)
            CreateSciFiBox("NexusFoundation", compound.transform, new Vector3(0, 4, 0), new Vector3(120, 8, 120), new Color(0.12f, 0.18f, 0.28f));

            // Main Core Spire (30x60x30m)
            CreateSciFiBox("NexusSpire", compound.transform, new Vector3(0, 38, 0), new Vector3(34, 60, 34), new Color(0.08f, 0.12f, 0.2f));

            // Elevated Sniper Ring Balcony (80x3x80m at height 42m)
            CreateSciFiBox("SniperRing", compound.transform, new Vector3(0, 42, 0), new Vector3(86, 3, 86), new Color(0.15f, 0.25f, 0.4f));

            // Holographic Compound Signage Facade using nexus_tower.png
            CreateCompoundBillboard("NexusTowerFacade", compound.transform, new Vector3(0, 48, -18), new Vector3(32, 20, 1), "nexus_tower");

            // 4 Corner Defense Watchtowers with stair access
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                Vector3 towerPos = new Vector3(Mathf.Cos(angle) * 50f, 16, Mathf.Sin(angle) * 50f);
                CreateSciFiBox($"CornerTower_{i}", compound.transform, towerPos, new Vector3(14, 24, 14), new Color(0.18f, 0.28f, 0.42f));
                
                // Glowing cyan energy strips (#00D4FF)
                CreateSciFiBox($"CyanStrip_{i}", compound.transform, towerPos + new Vector3(0, 10, 0), new Vector3(14.5f, 1, 14.5f), new Color(0f, 0.83f, 1f));
            }

            // Interior Vault & High-Tier Weapon Racks
            Spawn3DWeaponRack(compound.transform, new Vector3(0, 9, 0), "sr25_longshot", true);
            Spawn3DWeaponRack(compound.transform, new Vector3(6, 9, 0), "a41_vanguard", true);
            Spawn3DWeaponRack(compound.transform, new Vector3(-6, 9, 0), "lmg80_storm", true);
        }

        // ─── 2. INDUSTRIAL FACTORY COMPOUND (NORTH-EAST POI) ────────
        private static void BuildIndustrialFactoryCompound(Vector3 center, Transform parent)
        {
            var compound = new GameObject("[POI] Industrial Factory Complex");
            compound.transform.SetParent(parent);
            compound.transform.position = center;

            // Factory Main Warehouse (100x25x70m)
            CreateSciFiBox("MainWarehouse", compound.transform, new Vector3(0, 12.5f, 0), new Vector3(100, 25, 70), new Color(0.16f, 0.2f, 0.26f));

            // Catwalk Bridges & Interior Corridors
            CreateSciFiBox("OverheadCatwalk", compound.transform, new Vector3(0, 16, 0), new Vector3(90, 2, 12), new Color(0.24f, 0.3f, 0.4f));

            // Factory Billboard using industrial_factory.png
            CreateCompoundBillboard("FactoryFacade", compound.transform, new Vector3(0, 26, -36), new Vector3(40, 15, 1), "industrial_factory");

            // Shipping Container Stack Barriers for tactical cover
            for (int x = -35; x <= 35; x += 35)
            {
                for (int z = -20; z <= 20; z += 20)
                {
                    CreateSciFiBox($"Container_{x}_{z}", compound.transform, new Vector3(x, 4.5f, z), new Vector3(14, 9, 6), new Color(0.7f, 0.3f, 0.1f));
                }
            }

            Spawn3DWeaponRack(compound.transform, new Vector3(0, 17.5f, 0), "a17_striker", false);
            Spawn3DWeaponRack(compound.transform, new Vector3(-20, 1.5f, 10), "sg20_devastator", false);
        }

        // ─── 3. HYDRO STATION COMPOUND (SOUTH-EAST POI) ─────────────
        private static void BuildHydroStationCompound(Vector3 center, Transform parent)
        {
            var compound = new GameObject("[POI] Hydro Energy Dam Station");
            compound.transform.SetParent(parent);
            compound.transform.position = center;

            // Dam Wall Structure (140x30x20m)
            CreateSciFiBox("DamWall", compound.transform, new Vector3(0, 15, 0), new Vector3(140, 30, 20), new Color(0.14f, 0.22f, 0.32f));

            // Turbine Generator Room
            CreateSciFiBox("TurbineHall", compound.transform, new Vector3(0, 8, -25), new Vector3(60, 16, 30), new Color(0.1f, 0.16f, 0.24f));

            // Bridge Bottleneck Checkpoint
            CreateSciFiBox("CheckPointBridge", compound.transform, new Vector3(0, 31, 0), new Vector3(140, 2, 12), new Color(0.25f, 0.35f, 0.5f));

            CreateCompoundBillboard("HydroFacade", compound.transform, new Vector3(0, 36, -11), new Vector3(45, 16, 1), "hydro_station");

            Spawn3DWeaponRack(compound.transform, new Vector3(0, 32.5f, 0), "sr40_eliminator", true);
            Spawn3DWeaponRack(compound.transform, new Vector3(10, 9, -25), "s9_viper", false);
        }

        // ─── 4. FROST DEPOTS COMPOUND (NORTH-WEST POI) ──────────────
        private static void BuildFrostDepotsCompound(Vector3 center, Transform parent)
        {
            var compound = new GameObject("[POI] Frost Cryo Depots");
            compound.transform.SetParent(parent);
            compound.transform.position = center;

            // 3 Bunker Domes / Depots
            for (int i = -1; i <= 1; i++)
            {
                Vector3 bunkerPos = new Vector3(i * 35f, 8f, 0);
                CreateSciFiBox($"CryoBunker_{i}", compound.transform, bunkerPos, new Vector3(28, 16, 40), new Color(0.7f, 0.8f, 0.9f));
                
                // Cyan glowing entrance arch
                CreateSciFiBox($"Arch_{i}", compound.transform, bunkerPos + new Vector3(0, -3, -20.5f), new Vector3(12, 10, 1), new Color(0f, 0.83f, 1f));
            }

            CreateCompoundBillboard("FrostFacade", compound.transform, new Vector3(0, 18, -21), new Vector3(36, 14, 1), "frost_depots");

            Spawn3DWeaponRack(compound.transform, new Vector3(0, 2, 0), "lmg60_suppressor", false);
            Spawn3DWeaponRack(compound.transform, new Vector3(-35, 2, 0), "sg12_breaker", false);
        }

        // ─── 5. SOLAR FIELDS COMPOUND (SOUTH-WEST POI) ──────────────
        private static void BuildSolarFieldsCompound(Vector3 center, Transform parent)
        {
            var compound = new GameObject("[POI] Solar Fields Array Complex");
            compound.transform.SetParent(parent);
            compound.transform.position = center;

            // Central Control Station
            CreateSciFiBox("ControlStation", compound.transform, new Vector3(0, 10, 0), new Vector3(40, 20, 40), new Color(0.18f, 0.25f, 0.38f));

            // Array of Tilted Solar Panels providing tactical cover
            for (int x = -50; x <= 50; x += 25)
            {
                for (int z = -50; z <= 50; z += 25)
                {
                    if (Mathf.Abs(x) < 20 && Mathf.Abs(z) < 20) continue;
                    var panel = CreateSciFiBox($"SolarPanel_{x}_{z}", compound.transform, new Vector3(x, 4f, z), new Vector3(18, 1, 10), new Color(0.05f, 0.15f, 0.35f));
                    panel.transform.localRotation = Quaternion.Euler(30, 0, 0);
                }
            }

            CreateCompoundBillboard("SolarFacade", compound.transform, new Vector3(0, 22, -21), new Vector3(30, 12, 1), "solar_fields_landscape");

            Spawn3DWeaponRack(compound.transform, new Vector3(0, 11, 0), "a23_phantom", false);
        }

        // ─── 6. CRASH SITE COMPOUND (MID-WEST POI) ──────────────────
        private static void BuildCrashSiteCompound(Vector3 center, Transform parent)
        {
            var compound = new GameObject("[POI] Starship Wreckage Crash Site");
            compound.transform.SetParent(parent);
            compound.transform.position = center;

            // Impact Crater Rim Ring
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                Vector3 rimPos = new Vector3(Mathf.Cos(angle) * 40f, 3f, Mathf.Sin(angle) * 40f);
                var rim = CreateSciFiBox($"CraterRim_{i}", compound.transform, rimPos, new Vector3(26, 6, 12), new Color(0.15f, 0.12f, 0.1f));
                rim.transform.localRotation = Quaternion.Euler(0, -i * 45f, 0);
            }

            // Tilted Starship Hull Wreckage
            var hull = CreateSciFiBox("StarshipHull", compound.transform, new Vector3(0, 12, 0), new Vector3(60, 22, 28), new Color(0.22f, 0.24f, 0.28f));
            hull.transform.localRotation = Quaternion.Euler(18, 35, -12);

            // Orange warning energy leaks (#FF6B35)
            CreateSciFiBox("EnergyLeak", compound.transform, new Vector3(0, 18, 0), new Vector3(4, 30, 4), new Color(1f, 0.42f, 0.21f));

            CreateCompoundBillboard("CrashFacade", compound.transform, new Vector3(0, 25, -15), new Vector3(32, 14, 1), "crash_site");

            Spawn3DWeaponRack(compound.transform, new Vector3(0, 3, 0), "energy_blade", true);
            Spawn3DWeaponRack(compound.transform, new Vector3(15, 3, 10), "s14_stinger", false);
        }

        // ─── CONNECTING INFRASTRUCTURE & COVER BARRIERS ─────────────
        private static void BuildConnectingInfrastructure(Transform parent)
        {
            var infra = new GameObject("[POI] Tactical Connecting Cover & Barriers");
            infra.transform.SetParent(parent);

            // Scatter concrete blast barriers and energy cover points between POIs
            for (int i = 0; i < 40; i++)
            {
                float x = Random.Range(600, 3400);
                float z = Random.Range(600, 3400);
                CreateSciFiBox($"BlastBarrier_{i}", infra.transform, new Vector3(x, 2, z), new Vector3(8, 4, 2), new Color(0.3f, 0.35f, 0.42f));
            }
        }

        // ─── ARCHITECTURAL HELPERS ──────────────────────────────────
        private static GameObject CreateSciFiBox(string name, Transform parent, Vector3 localPos, Vector3 size, Color color)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent);
            box.transform.localPosition = localPos;
            box.transform.localScale = size;

            var renderer = box.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Smoothness", 0.7f);
            mat.SetFloat("_Metallic", 0.5f);
            renderer.material = mat;
            return box;
        }

        private static void CreateCompoundBillboard(string name, Transform parent, Vector3 localPos, Vector3 size, string textureKey)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent);
            quad.transform.localPosition = localPos;
            quad.transform.localScale = size;

            var renderer = quad.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

            if (_poiTextureCache.TryGetValue(textureKey, out Texture2D tex) && tex != null)
            {
                mat.mainTexture = tex;
                mat.color = Color.white;
            }
            else
            {
                mat.color = new Color(0f, 0.83f, 1f);
            }
            renderer.material = mat;
        }

        private static void Spawn3DWeaponRack(Transform parent, Vector3 localPos, string weaponId, bool isHighTier)
        {
            var rack = new GameObject($"[RACK] {weaponId}_Spawner");
            rack.transform.SetParent(parent);
            rack.transform.localPosition = localPos;

            // Stand structure (Child 0 for LootItem visual transform)
            var stand = CreateSciFiBox("RackStand", rack.transform, new Vector3(0, 1, 0), new Vector3(3, 2, 1.5f), isHighTier ? new Color(1f, 0.72f, 0f) : new Color(0.2f, 0.3f, 0.4f));

            // Attach SphereCollider and LootItem script so players can pick it up
            var sphereCollider = rack.AddComponent<SphereCollider>();
            sphereCollider.radius = 3f;
            sphereCollider.isTrigger = true;

            var loot = rack.AddComponent<LootItem>();
            var itemData = ScriptableObject.CreateInstance<Data.ItemData>();
            itemData.itemId = weaponId;
            itemData.itemName = weaponId;
            loot.Initialize(itemData, 1);
        }
    }
}
