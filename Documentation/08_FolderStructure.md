# Arena Fall — Project Folder Structure

```
ArenaFall_Project/
├── Assets/
│   ├── Art/
│   │   ├── Characters/
│   │   │   ├── Female/
│   │   │   │   ├── Models/
│   │   │   │   ├── Textures/
│   │   │   │   ├── Materials/
│   │   │   │   └── Prefabs/
│   │   │   ├── Male/
│   │   │   │   ├── Models/
│   │   │   │   ├── Textures/
│   │   │   │   ├── Materials/
│   │   │   │   └── Prefabs/
│   │   │   ├── NPCs/
│   │   │   └── Skins/
│   │   ├── Weapons/
│   │   │   ├── AssaultRifles/
│   │   │   ├── SMGs/
│   │   │   ├── Shotguns/
│   │   │   ├── Snipers/
│   │   │   ├── LMGs/
│   │   │   ├── Pistols/
│   │   │   ├── Melee/
│   │   │   ├── Throwables/
│   │   │   ├── Attachments/
│   │   │   └── Ammo/
│   │   ├── Vehicles/
│   │   │   ├── ATV/
│   │   │   ├── Bike/
│   │   │   ├── Truck/
│   │   │   └── Hovercraft/
│   │   ├── Buildings/
│   │   │   ├── NexusTower/
│   │   │   ├── Factory/
│   │   │   ├── Warehouse/
│   │   │   ├── House/
│   │   │   ├── Bridge/
│   │   │   └── Interior/
│   │   ├── Environment/
│   │   │   ├── Nature/
│   │   │   │   ├── Trees/
│   │   │   │   ├── Bushes/
│   │   │   │   ├── Rocks/
│   │   │   │   └── Grass/
│   │   │   ├── Props/
│   │   │   └── Terrain/
│   │   ├── Skyboxes/
│   │   └── VFX/
│   ├── Animations/
│   │   ├── Characters/
│   │   ├── Weapons/
│   │   ├── Vehicles/
│   │   └── UI/
│   ├── AnimatorControllers/
│   │   ├── Character/
│   │   ├── Weapon/
│   │   └── Vehicle/
│   ├── Audio/
│   │   ├── Music/
│   │   ├── SFX/
│   │   ├── Voice/
│   │   └── Mixers/
│   ├── Materials/
│   │   ├── Characters/
│   │   ├── Weapons/
│   │   ├── Vehicles/
│   │   ├── Environment/
│   │   ├── Buildings/
│   │   └── UI/
│   ├── Shaders/
│   │   ├── Character/
│   │   ├── Weapon/
│   │   ├── Environment/
│   │   ├── UI/
│   │   └── PostProcessing/
│   ├── Textures/
│   │   ├── Characters/
│   │   ├── Weapons/
│   │   ├── Vehicles/
│   │   ├── Environment/
│   │   ├── Buildings/
│   │   └── UI/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs
│   │   │   ├── EventBus.cs
│   │   │   ├── ServiceLocator.cs
│   │   │   ├── PoolManager.cs
│   │   │   └── SceneLoader.cs
│   │   ├── Managers/
│   │   │   ├── GameStateManager.cs
│   │   │   ├── UIManager.cs
│   │   │   ├── AudioManager.cs
│   │   │   ├── InputManager.cs
│   │   │   ├── CameraManager.cs
│   │   │   ├── MatchManager.cs
│   │   │   ├── PlayerManager.cs
│   │   │   ├── WeaponManager.cs
│   │   │   ├── LootManager.cs
│   │   │   ├── VehicleManager.cs
│   │   │   ├── AIManager.cs
│   │   │   ├── StatsManager.cs
│   │   │   ├── ProgressionManager.cs
│   │   │   ├── SaveManager.cs
│   │   │   ├── SettingsManager.cs
│   │   │   ├── LocalizationManager.cs
│   │   │   ├── AnalyticsManager.cs
│   │   │   └── ZoneManager.cs
│   │   ├── Gameplay/
│   │   │   ├── Characters/
│   │   │   │   ├── CharacterController.cs
│   │   │   │   ├── CharacterAnimation.cs
│   │   │   │   ├── CharacterHealth.cs
│   │   │   │   ├── CharacterInventory.cs
│   │   │   │   └── CharacterAudio.cs
│   │   │   ├── Weapons/
│   │   │   │   ├── WeaponController.cs
│   │   │   │   ├── WeaponAmmo.cs
│   │   │   │   ├── WeaponAttachment.cs
│   │   │   │   ├── WeaponRecoil.cs
│   │   │   │   ├── WeaponAiming.cs
│   │   │   │   ├── Projectile.cs
│   │   │   │   ├── HitscanWeapon.cs
│   │   │   │   ├── ProjectileWeapon.cs
│   │   │   │   └── MeleeWeapon.cs
│   │   │   ├── Inventory/
│   │   │   │   ├── Inventory.cs
│   │   │   │   ├── InventorySlot.cs
│   │   │   │   ├── Backpack.cs
│   │   │   │   ├── LootItem.cs
│   │   │   │   ├── Pickup.cs
│   │   │   │   └── AmmoItem.cs
│   │   │   ├── Vehicles/
│   │   │   │   ├── VehicleController.cs
│   │   │   │   ├── VehicleSeat.cs
│   │   │   │   ├── VehicleMovement.cs
│   │   │   │   └── VehicleHealth.cs
│   │   │   ├── AI/
│   │   │   │   ├── AIController.cs
│   │   │   │   ├── AIStateMachine.cs
│   │   │   │   ├── AICombat.cs
│   │   │   │   ├── AIMovement.cs
│   │   │   │   ├── AILoot.cs
│   │   │   │   └── AISpawner.cs
│   │   │   ├── Zone/
│   │   │   │   ├── SafeZone.cs
│   │   │   │   ├── ZoneVisualizer.cs
│   │   │   │   └── ZoneDamage.cs
│   │   │   └── Match/
│   │   │       ├── MatchController.cs
│   │   │       ├── DropPod.cs
│   │   │       ├── SupplyDrop.cs
│   │   │       └── SpectatorMode.cs
│   │   ├── Networking/
│   │   │   ├── NetworkPlayer.cs
│   │   │   ├── NetworkWeapon.cs
│   │   │   ├── NetworkProjectile.cs
│   │   │   ├── NetworkLoot.cs
│   │   │   ├── NetworkVehicle.cs
│   │   │   ├── NetworkZone.cs
│   │   │   ├── NetworkVoice.cs
│   │   │   └── ReconnectionHandler.cs
│   │   ├── UI/
│   │   │   ├── MainMenu/
│   │   │   ├── HUD/
│   │   │   ├── Inventory/
│   │   │   ├── Minimap/
│   │   │   ├── Settings/
│   │   │   ├── Lobby/
│   │   │   ├── Result/
│   │   │   ├── Shop/
│   │   │   ├── BattlePass/
│   │   │   ├── Loadout/
│   │   │   └── Common/
│   │   ├── Audio/
│   │   │   ├── AudioZone.cs
│   │   │   ├── FootstepAudio.cs
│   │   │   └── WeaponAudio.cs
│   │   ├── Utilities/
│   │   │   ├── MathUtils.cs
│   │   │   ├── ExtensionMethods.cs
│   │   │   ├── Singleton.cs
│   │   │   └── StateMachine.cs
│   │   ├── Interfaces/
│   │   │   ├── IDamageable.cs
│   │   │   ├── IWeapon.cs
│   │   │   ├── IItem.cs
│   │   │   ├── IInventory.cs
│   │   │   ├── IPickupable.cs
│   │   │   ├── IUsable.cs
│   │   │   ├── IVehicle.cs
│   │   │   ├── IPoolable.cs
│   │   │   └── ISaveData.cs
│   │   ├── Events/
│   │   │   ├── GameEvents.cs
│   │   │   ├── WeaponEvents.cs
│   │   │   ├── PlayerEvents.cs
│   │   │   ├── UIEvents.cs
│   │   │   ├── NetworkEvents.cs
│   │   │   └── AudioEvents.cs
│   │   └── Data/
│   │       ├── WeaponData.cs
│   │       ├── ItemData.cs
│   │       ├── AttachmentData.cs
│   │       ├── CharacterData.cs
│   │       ├── VehicleData.cs
│   │       ├── LootTableData.cs
│   │       ├── MapData.cs
│   │       ├── SkinData.cs
│   │       ├── MissionData.cs
│   │       ├── PlayerSaveData.cs
│   │       └── SettingsData.cs
│   ├── Prefabs/
│   │   ├── Characters/
│   │   ├── Weapons/
│   │   ├── Buildings/
│   │   ├── Vehicles/
│   │   ├── Loot/
│   │   ├── Effects/
│   │   ├── UI/
│   │   └── Systems/
│   ├── Scenes/
│   │   ├── 01_Boot/
│   │   ├── 02_Login/
│   │   ├── 03_MainMenu/
│   │   ├── 04_Profile/
│   │   ├── 05_Lobby/
│   │   ├── 06_Customization/
│   │   ├── 07_Loadout/
│   │   ├── 08_TrainingGround/
│   │   ├── 09_Matchmaking/
│   │   ├── 10_GameMap/
│   │   ├── 11_ResultScreen/
│   │   ├── 12_Replay/
│   │   ├── 13_Settings/
│   │   └── 14_TestScene/
│   ├── UI/
│   │   ├── Fonts/
│   │   ├── Icons/
│   │   ├── Sprites/
│   │   ├── Templates/
│   │   └── Animations/
│   ├── Resources/
│   ├── StreamingAssets/
│   └── Addressables/
├── Packages/
├── ProjectSettings/
├── Documentation/
└── Tests/
    ├── Editor/
    ├── Runtime/
    └── PlayMode/
```
