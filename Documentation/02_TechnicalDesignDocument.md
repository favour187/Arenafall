# Arena Fall — Technical Design Document

## 1. Architecture Overview

### Core Architecture: ECS-Inspired Component System
The game uses a hybrid architecture combining:
- **GameObject/Component pattern** (Unity native)
- **ScriptableObject data system** for configurable content
- **Event-driven communication** via a central Event Bus
- **Service Locator pattern** for manager access
- **Dependency Injection** via Zenject/VContainer

### Layer Architecture

```
┌─────────────────────────────────────┐
│         Presentation Layer          │
│  UI, HUD, Menus, VFX, Audio        │
├─────────────────────────────────────┤
│         Gameplay Layer              │
│  Characters, Weapons, Vehicles      │
│  Inventory, Health, Loot            │
├─────────────────────────────────────┤
│         Networking Layer            │
│  Netcode, RPCs, State Sync         │
│  Matchmaking, Lobby                 │
├─────────────────────────────────────┤
│         Data Layer                  │
│  Save System, Config, Localization  │
│  Analytics, Cloud Save              │
├─────────────────────────────────────┤
│         Core Services               │
│  Event Bus, Pooling, Addressables   │
│  Input, Audio, Camera               │
└─────────────────────────────────────┘
```

---

## 2. Script Architecture

### Core Systems (Scripts/Core/)
- **GameManager.cs** — Main game state controller
- **EventBus.cs** — Central event system
- **ServiceLocator.cs** — Service registration/lookup
- **PoolManager.cs** — Object pooling
- **SceneLoader.cs** — Async scene management
- **CoroutineRunner.cs** — Global coroutine host

### Managers (Scripts/Managers/)
- **GameStateManager.cs** — Game state machine
- **UIManager.cs** — UI stack management
- **AudioManager.cs** — Audio playback control
- **InputManager.cs** — Input handling
- **CameraManager.cs** — Camera control
- **NetworkManager.cs** — Network connection
- **MatchManager.cs** — Match lifecycle
- **PlayerManager.cs** — Player management
- **WeaponManager.cs** — Weapon registry
- **LootManager.cs** — Loot spawning
- **VehicleManager.cs** — Vehicle registry
- **AIManager.cs** — Bot control
- **StatsManager.cs** — Statistics tracking
- **ProgressionManager.cs** — Level/XP
- **SaveManager.cs** — Save/load operations
- **SettingsManager.cs** — Game settings
- **LocalizationManager.cs** — Text localization
- **AnalyticsManager.cs** — Analytics events

### Interfaces (Scripts/Interfaces/)
- **IDamageable.cs** — Damage receiver contract
- **IWeapon.cs** — Weapon contract
- **IItem.cs** — Item contract
- **IInventory.cs** — Inventory contract
- **IPickupable.cs** — Pickup contract
- **IUsable.cs** — Usable item contract
- **IVehicle.cs** — Vehicle contract
- **IPlayerController.cs** — Player controller contract
- **INetworkSerializable.cs** — Network data contract
- **IPoolable.cs** — Poolable object contract
- **ISaveData.cs** — Save data contract
- **IStateMachine.cs** — State machine contract

### Data (Scripts/Data/)
- **WeaponData.cs** (ScriptableObject)
- **ItemData.cs** (ScriptableObject)
- **AttachmentData.cs** (ScriptableObject)
- **CharacterData.cs** (ScriptableObject)
- **VehicleData.cs** (ScriptableObject)
- **LootTableData.cs** (ScriptableObject)
- **MapData.cs** (ScriptableObject)
- **SkinData.cs** (ScriptableObject)
- **MissionData.cs** (ScriptableObject)

---

## 3. Network Architecture

### Transport Layer
- **Unity Netcode for GameObjects** (primary)
- **Unity Transport** (underlying protocol)
- **Relay Service** for NAT punchthrough
- **Lobby Service** for matchmaking

### Network Objects
- **PlayerController (NetworkBehaviour)**
- **Weapon (NetworkBehaviour)**
- **Projectile (NetworkBehaviour)**
- **Vehicle (NetworkBehaviour)**
- **LootItem (NetworkBehaviour)**
- **Pickup (NetworkBehaviour)**
- **SupplyDrop (NetworkBehaviour)**
- **ZoneController (NetworkBehaviour)**

### State Sync Strategy
| Data Type | Sync Method | Update Rate |
|-----------|------------|-------------|
| Position/Rotation | NetworkTransform | 20 Hz |
| Health/Shield | NetworkVariable | 10 Hz |
| Weapon State | RPC | On change |
| Inventory | NetworkVariable | 5 Hz |
| Animation | Custom | 10 Hz |
| Loot Spawns | NetworkVariable | On change |
| Zone State | NetworkVariable | 1 Hz |

### Authority Model
- **Server Authoritative** for all gameplay
- **Client Prediction** for movement
- **Server Reconciliation** for corrections
- **Lag Compensation** for hit detection

---

## 4. Save Data Structure

```json
{
  "playerId": "uuid-string",
  "playerName": "PlayerName",
  "level": 1,
  "xp": 0,
  "prestige": 0,
  "currency": {
    "credits": 0,
    "premium": 0
  },
  "stats": {
    "matchesPlayed": 0,
    "wins": 0,
    "top10": 0,
    "kills": 0,
    "deaths": 0,
    "damageDealt": 0,
    "damageTaken": 0,
    "headshots": 0,
    "revives": 0,
    "playTime": 0
  },
  "inventory": {
    "skins": [],
    "emotes": [],
    "characters": []
  },
  "loadouts": [
    {
      "name": "Default",
      "primary": null,
      "secondary": null,
      "melee": null,
      "throwable": null,
      "character": null
    }
  ],
  "settings": {
    "sensitivity": 5.0,
    "volume": 0.8,
    "graphics": "auto",
    "controls": "default"
  },
  "missions": {
    "daily": [],
    "weekly": [],
    "achievements": [],
    "battlePass": {
      "tier": 1,
      "xp": 0,
      "premium": false
    }
  },
  "lastLogin": "timestamp",
  "created": "timestamp"
}
```

---

## 5. Database Structure (Cloud)

### Collections
- **players** — Player profiles
- **matches** — Match history
- **leaderboards** — Ranked scores
- **analytics** — Game analytics
- **lobbies** — Active lobbies

---

## 6. Optimization Strategy

### GPU
- URP with forward rendering
- LOD Groups on all meshes
- GPU Instancing on shared materials
- Occlusion Culling baked
- Texture atlas for UI
- Mobile: Reduce texture sizes, disable MSAA

### CPU
- Object pooling for projectiles, effects, loot
- Job System for AI calculations
- Burst compiler for physics
- Async loading for all assets
- Batching for UI rendering

### Memory
- Addressables for asset streaming
- Texture streaming for large textures
- Unused asset unloading
- Pool limit enforcement

### Network
- Delta compression for state updates
- Interest management (relevancy)
- Adaptive tickrate based on player count
- Bandwidth limiting per connection

---

## 7. Build Pipeline

### Platforms
- **Windows:** DirectX 11/12, 64-bit
- **Android:** Vulkan/OpenGL ES 3.0, ARM64
- **iOS:** Metal, ARM64

### Build Steps
1. Asset bundle build (Addressables)
2. Script compilation
3. Platform switch
4. Build player
5. Post-process (stripping, symbols)
6. Distribution packaging
