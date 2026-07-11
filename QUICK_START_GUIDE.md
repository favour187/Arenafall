# 🚀 Arena Fall — Quick Start Guide

## ⚡ ZERO SETUP — Just Create Project + Play

---

### 📦 Step 1: Create Unity Project (2 MINUTES)

1. Open **Unity Hub**
2. Click **New Project**
3. Select **Universal 3D (URP)** template
4. Name: **ArenaFall**
5. Click **Create**

---

### 📂 Step 2: Replace Files (1 MINUTE)

1. Close Unity Editor
2. Open your `ArenaFall` project folder
3. **DELETE** the `Assets` folder
4. **COPY** the entire `Assets` folder from this zip into your project
5. **COPY** the `Packages/` and `ProjectSettings/` folders too
6. Open the project in Unity Hub

---

### ⏳ Step 3: Wait for Import (2-3 MINUTES)

Unity will import everything. Wait for the spinning icon at bottom-right to stop.

---

### ▶️ Step 4: PRESS PLAY (0 EFFORT)

1. In **Project** window, go to `Assets/Scenes/01_Boot/`
2. Double-click **`Boot.unity`** to open it
3. Press **Play** ▶️

**That's it.** The game auto-generates everything:

| What happens | Who does it |
|-------------|-------------|
| Splash screen appears | ✅ `SceneAutoBuilder` |
| Auto-loads to Main Menu after 2s | ✅ `AutoSceneLoader` |
| Main Menu with PLAY, SOLO, DUOS, SQUADS, TRAINING buttons | ✅ `SceneAutoBuilder` |
| HUD with health, shields, ammo, minimap, crosshair | ✅ `SceneAutoBuilder` |
| Game Map with terrain, player, bots, loot | ✅ `SceneAutoBuilder` |
| Settings page with volume sliders | ✅ `SceneAutoBuilder` |
| Result screen with stats | ✅ `SceneAutoBuilder` |

### No manual setup required.
### No button references to assign.
### No scene configuration.
### Just hit Play.

---

### 🏗️ Step 7: Build for Platforms

#### Windows Build
1. **File > Build Settings**
2. Ensure all 14 scenes are checked
3. Target: **PC, Mac & Linux Standalone**
4. Architecture: **x86_64**
5. Click **Build**
6. Choose output folder → "ArenaFall_Win"

#### Android Build
1. **File > Build Settings**
2. Switch Platform: **Android**
3. **Player Settings:**
   - Bundle ID: `com.arenagames.arenafall`
   - Min API Level: 26
   - Target API Level: 31
4. Graphics APIs: **Vulkan** (remove OpenGL ES 2.0)
5. **Build Settings > Build**

---

## 🔧 Script References & GUID Setup

Because Unity uses GUIDs to link scripts to GameObjects, after importing you need to rebind the script references. **The Setup Wizard does this automatically.** If something still shows "Missing Script":

1. Open the scene/prefab
2. **Tools > Arena Fall > Quick Fix**
3. Click **"Fix Missing Script References"**
4. This scans all prefabs and scenes, removing broken references

---

## 🗺️ Scene Reference Guide

| Scene | What It Does | When To Open |
|-------|-------------|--------------|
| `01_Boot` | Bootstrap + splash → auto-loads MainMenu | **Play starts here** |
| `03_MainMenu` | Navigation hub with all menus | After boot |
| `05_Lobby` | Pre-match lobby with players | After queue pops |
| `08_TrainingGround` | Solo practice zone | From Main Menu |
| `10_GameMap` | **The actual battle royale match!** | After match starts |
| `11_ResultScreen` | Post-match stats + XP | Match ends |
| `13_Settings` | Audio/Video/Controls | From any menu |

---

## 🎮 Controls (Default)

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move | WASD | Left Stick |
| Look | Mouse | Right Stick |
| Fire | Left Click | RT |
| Aim | Right Click | LT |
| Reload | R | B (East) |
| Jump | Space | A (South) |
| Crouch | C / Ctrl | LB |
| Sprint | Shift | LS Click |
| Interact | F | X (West) |
| Inventory | Tab | Start |
| Map | M | Select |
| Ping | Middle Click | D-Pad Up |
| Healing | 4 | D-Pad Left |
| Weapon Slots | 1, 2, 3 | D-Pad |

---

## 📁 Key Files Reference

| What | Where |
|------|-------|
| All Game Systems | `Assets/Scripts/` (42 files) |
| Weapon Stats | `Assets/Resources/GameData/Weapons/` |
| Item Stats | `Assets/Resources/GameData/Items/` |
| Art Assets | `Assets/Art/` |
| UI Assets | `Assets/UI/Sprites/` |
| Generated Logo | `Assets/UI/Sprites/arena_fall_logo.png` |
| Main Menu Setup | `Assets/Scenes/03_MainMenu/` |
| HUD Controller | `Assets/Scripts/UI/HUD/HUDController.cs` |
| Player Controller | `Assets/Scripts/Gameplay/Characters/CharacterController.cs` |
| Weapon System | `Assets/Scripts/Gameplay/Weapons/WeaponController.cs` |
| Network Player | `Assets/Scripts/Networking/NetworkPlayer.cs` |
| Game Design Doc | `Documentation/01_GameDesignDocument.md` |

---

## ❗ Common Issues & Fixes

**"Missing Script" warnings on GameObjects**
→ **Tools > Arena Fall > Quick Fix > "Fix Missing Script References"**

**Scenes not in Build Settings**
→ Run **Setup Wizard** or manually add them in **File > Build Settings**

**Input not working**
→ Make sure **Input System** package is installed (Window > Package Manager)
→ Run **Setup Wizard** which creates the Input Action Asset

**No URP errors**
→ Install **Universal RP** from Package Manager
→ Then run **Setup Wizard**

**"Can't find ArenaFall namespace"**
→ Scripts haven't compiled yet. Wait for Unity to finish, or **Assets > Reimport All**

---

## 🎯 What's Ready vs Needs Your Input

### ✅ Fully Working (open and play)
- Boot sequence → auto-loads scenes
- Main Menu navigation
- Character controller (movement, jump, crouch, sprint, slide, vault, swim)
- Health/shield system with damage
- Weapon system (fire, reload, aim, ammo)
- Inventory system (pickup, drop, stack)
- Loot items with rarity colors
- Safe zone system (5-stage shrink)
- Audio manager with mixer
- Input manager with action maps
- Save/load system
- Camera system (third-person, aim zoom)
- AI bots (patrol, combat, loot)
- Vehicle system (enter/exit, drive)
- HUD display (health, shield, ammo, minimap)

### 🖼️ Needs You to Assign (Drag & Drop)
- **Art assets** → Drag generated PNGs into Sprite fields on WeaponData, ItemData assets
- **Weapon models** → Assign WeaponData weaponPrefab field
- **Audio clips** → Assign to WeaponData, ItemData fields (use free sounds or generate)
- **UI references** → Link MainMenuController button references in scene (drag from hierarchy)
- **Animation controllers** → Assign to character/weapon prefabs

### 🎵 External Content You'll Need
- **Audio clips** (footsteps, gunshots, UI sounds) — use free packs from Asset Store or generate
- **3D models** for weapons/characters — assign to prefab fields
- **Animations** — use Unity's built-in animation system or Mixamo

---

## 15-Minute Startup Checklist

- [ ] Create Unity Project (URP template)
- [ ] Replace Assets, Packages, ProjectSettings folders
- [ ] Wait for compilation
- [ ] Tools > Arena Fall > Run Full Auto Setup
- [ ] Tools > Arena Fall > Quick Fix > Run Full Validation
- [ ] Open 01_Boot scene
- [ ] Press Play ▶️
- [ ] 🎮 You're in the Main Menu!

---

**Enjoy building Arena Fall!** 
