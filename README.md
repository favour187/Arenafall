# ARENA FALL — Battle Royale Game

## Overview
**Arena Fall** is an original, commercial-quality multiplayer battle royale game built with Unity. 50-100 players battle in a vast sci-fi training facility, scavenging weapons and equipment while a shrinking energy field forces closer combat. The last player standing wins.

## Key Features
- **Original IP & Sci-Fi Compounds:** 7 distinct 3D Compound POIs including our new **Orbital Relay Station** with **Zero-Gravity Jump Pads (`LaunchPadBouncer`)** that propel players 28 meters upward into low-G air battles.
- **50-100 Player Lobbies:** Solo, Duos, and Squads modes powered by Unity Netcode for GameObjects (`port 7777`, `MaxPacketQueueSize = 1024`) with spatial grid delta partitioning (`gridX_gridZ`) and adaptive `30Hz/15Hz` distance throttling.
- **11 Unique Weapons & Stands:** Includes our new heavy **PC-90 Plasma Cannon (`pc90_plasma_cannon`)**, Assault Rifles, SMGs, Shotguns, Snipers, and Melee blades resting on interactive 3D compound weapon racks (`Spawn3DWeaponRack`).
- **5 Drivable Physics Vehicles:** Includes our new bipedal **Armored Mech Walker (`armored_mech_walker.png`)**, Ranger ATV, Sci-Fi Hovercraft (`anti-gravity spring physics`), Cyclone Recon Bike, and Heavy Armored Truck.
- **Modern AAA Battle Royale UI & Touch Controls:** Beveled dual-layer sci-fi buttons with neon side bars (`SideNeonStrip`) plus **Virtual On-Screen Mobile & iOS Touch Controls (`MobileTouchControls.cs`)** featuring virtual joysticks and metallic touch buttons (`[🔥 FIRE]`, `[🎯 AIM]`, `[🦘 JUMP]`, `[🔄 RELOAD]`, `[✋ INTERACT / DRIVE]`).
- **Instant Mobile Guest & Online Auth:** Play immediately with our one-tap **`⚡ PLAY AS GUEST`** mobile flow (`/api/v1/auth/guest`) or full email/password JWT accounts (`/api/v1/auth/login`).
- **Authoritative Backend Supervision Daemon:** Built-in Node.js supervisor (`BackendSupervisionService.js`) monitoring tick rates (`30.0 Hz`), memory consumption, and automated **24-Hour Shadow Bans** when `AntiCheatService.js` detects $\ge 3$ movement/combat anomalies.
- **Full Glassmorphic Web Portals:** Live Admin Dashboard (`/admin`) and Player Portal (`/`) with interactive catalog modals (`+ Add Item`, `Edit Season`), real-time Socket.IO telemetry, and complete Terms of Service & Privacy Policy overlays.

## Project Structure

```
Assets/
├── Art/                    # All visual assets
│   ├── Characters/         # Character models, textures, concepts
│   ├── Weapons/            # Weapon models and concepts
│   ├── Vehicles/           # Vehicle assets
│   ├── Buildings/          # Building prefabs
│   └── Environment/        # Terrain, nature, skyboxes
├── Scripts/                # All C# source code
│   ├── Core/               # Foundation systems (EventBus, ServiceLocator, Pooling)
│   ├── Managers/           # Game state, audio, input, save, match systems
│   ├── Gameplay/           # Characters, weapons, inventory, vehicles, AI, zone
│   ├── Networking/         # Network player, state sync, RPCs
│   ├── UI/                 # Main menu, HUD, inventory, settings
│   ├── Interfaces/         # All API contracts
│   ├── Events/             # Event type definitions
│   ├── Data/               # ScriptableObject data containers
│   └── Utilities/          # State machine, extensions
├── Prefabs/                # All prefab assets
├── Scenes/                 # 14 organized scenes
└── UI/                     # Fonts, icons, sprites, animations
```

## Architecture

### Core Systems
- **EventBus:** Type-safe publish/subscribe for decoupled communication
- **ServiceLocator:** Global service access with interface-based registration
- **PoolManager:** Object pooling for projectiles, effects, loot items
- **SceneLoader:** Async scene management with loading screens

### Data-Driven Design
All game content is configured via ScriptableObjects:
- `WeaponData` — Weapon stats, audio, visuals, attachments
- `ItemData` — Item properties and behaviors
- `CharacterData` — Character models and cosmetics
- `LootTableData` — Weighted loot distribution
- `VehicleData` — Vehicle physics and seating

### Event System
```csharp
// Subscribe
EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);

// Raise
EventBus.Raise(new PlayerDiedEvent { PlayerId = id, KillerId = killer });
```

## Scenes

| # | Scene | Purpose |
|---|-------|---------|
| 01 | Boot | Initialization, splash screen |
| 02 | Login | Player authentication |
| 03 | MainMenu | Navigation hub |
| 04 | Profile | Player stats and history |
| 05 | Lobby | Pre-match social space |
| 06 | Customization | Character and weapon skins |
| 07 | Loadout | Weapon and item selection |
| 08 | TrainingGround | Practice mode |
| 09 | Matchmaking | Queue for match |
| 10 | GameMap | Active gameplay |
| 11 | ResultScreen | Post-match summary |
| 12 | Replay | Match replay viewer |
| 13 | Settings | Audio, video, controls |
| 14 | TestScene | Development testing |

## Gameplay Systems

### Combat
- Hitscan and projectile weapons
- Headshot multiplier (2x damage)
- Damage falloff over distance
- Weapon attachments modify all stats
- Armor system with tiered damage reduction

### Movement
- Walk, sprint, crouch, prone
- Jump with buffer system
- Slide mechanic
- Vault over obstacles
- Swimming in water
- Stamina management

### Inventory
- 2 weapon slots + melee + throwables
- Tiered backpack (6/10/15 slots)
- Stackable items (ammo, healing)
- Quick-use healing items

### AI Bots
- Finite state machine (Idle, Patrol, Combat, Loot, Flee)
- Line-of-sight detection
- Weighted combat decisions
- Difficulty tiers

### Safe Zone
- 5-stage shrinking system
- Random circle placement
- Damage scales per stage
- Visual wall and indicator

## Technical Details

### Platforms
- Windows (DirectX 11/12)
- Android (Vulkan/OpenGL ES 3.0)
- iOS (Metal)

### Performance Targets
- PC: 60+ FPS at 1080p
- Mobile: 30+ FPS (optimized)

### Dependencies
- Universal Render Pipeline (URP)
- Unity Input System
- Addressables
- TextMeshPro
- Cinemachine
- Unity Netcode for GameObjects
- Shader Graph / VFX Graph
- AI Navigation
- Localization

## Development Status

### ✅ Completed
- Game Design Document (GDD)
- Technical Design Document
- Art, UI, and Audio Style Guides
- Complete folder structure
- Core architecture (EventBus, ServiceLocator)
- Object pooling system
- Scene management
- All interfaces defined
- Data ScriptableObjects (Weapons, Items, Attachments, Characters)
- Save/load system
- Character controller with full movement
- Health/shield system
- Weapon system with firing, reloading, aiming
- Inventory system with stacking
- Loot pickups with rarity visuals
- Vehicle system (ground, hover, amphibious)
- AI controller with state machine
- Safe zone manager
- Match manager
- Camera manager
- Audio manager with mixer
- Input manager with action maps
- Localization system
- Analytics system
- Network player setup
- Main menu UI
- HUD controller
- Settings manager
- Progression system
- Game logo and branding assets
- UI icon atlas
- Weapon icons
- HUD elements
- Character and environment concept art
- All documentation files

### 🚧 In Progress
- Full map terrain and building placement
- Complete weapon prefab library
- Character model rigging and animations
- Audio clip library
- Multiplayer testing and optimization
- Mobile optimization pass

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| Network latency | High | Client-side prediction, server reconciliation |
| Mobile performance | High | LODs, texture streaming, reduced quality |
| Asset production time | High | Procedural generation, automated workflows |
| Memory limits | Medium | Addressables, pooling, streaming |
| Anti-cheat | Medium | Server authority, validation checks |
| Balance issues | Medium | Data-driven design, rapid iteration |

## Credits
Developed autonomously using AI-assisted development tools.
Original concept, design, and implementation by Arena Games.
All assets are original and created for commercial use.
