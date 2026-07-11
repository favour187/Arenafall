using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using ArenaFall.Core;
using ArenaFall.Managers;
using ArenaFall.UI.MainMenu;
using ArenaFall.UI.HUD;
using ArenaFall.Gameplay.Characters;
using ArenaFall.Gameplay.Weapons;
using ArenaFall.Gameplay.Inventory;
using ArenaFall.Gameplay.Zone;
using ArenaFall.Networking;

/// <summary>
/// AUTO-GENERATES all GameObjects when a scene loads.
/// Attach to Bootstrapper. No manual setup needed!
/// </summary>
public class SceneAutoBuilder : MonoBehaviour
{
    private static bool _initialized;
    private Dictionary<string, Sprite> _spriteCache = new();

    private void Awake()
    {
        if (_initialized) return;
        _initialized = true;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("[SceneAutoBuilder] Active — scenes will auto-generate on load");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SceneAutoBuilder] Building scene: {scene.name}");

        // Clear any existing auto-generated objects to avoid duplicates
        CleanupPreviousBuild(scene);

        // Build based on scene name
        switch (scene.name)
        {
            case "Boot":
                BuildBootScene(scene);
                break;
            case "MainMenu":
                BuildMainMenuScene(scene);
                break;
            case "Lobby":
                BuildLobbyScene(scene);
                break;
            case "GameMap":
                BuildGameMapScene(scene);
                break;
            case "TrainingGround":
                BuildTrainingScene(scene);
                break;
            case "ResultScreen":
                BuildResultScene(scene);
                break;
            case "Settings":
                BuildSettingsScene(scene);
                break;
            case "TestScene":
                BuildTestScene(scene);
                break;
            default:
                // Any other scene gets essentials
                BuildEssentialSystems(scene);
                break;
        }

        Debug.Log($"[SceneAutoBuilder] ✓ {scene.name} built successfully");
    }

    private void CleanupPreviousBuild(Scene scene)
    {
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            if (root.name.StartsWith("[AUTO]"))
            {
                DestroyImmediate(root);
            }
        }
    }

    // ─── BOOT SCENE ────────────────────────────────────────────
    private void BuildBootScene(Scene scene)
    {
        var bootObj = new GameObject("[AUTO] Boot Systems");
        bootObj.AddComponent<Bootstrapper>()._initializeOnAwake = true;
        
        // Spawn core managers
        var gameManager = new GameObject("[AUTO] GameManager");
        gameManager.AddComponent<GameManager>();
        gameManager.AddComponent<SaveManager>();
        gameManager.AddComponent<AudioManager>();
        gameManager.AddComponent<InputManager>();
        gameManager.AddComponent<CameraManager>();
        gameManager.AddComponent<SceneLoader>();
        gameManager.AddComponent<PoolManager>();
        gameManager.AddComponent<GameStateManager>();
        gameManager.AddComponent<SettingsManager>();
        gameManager.AddComponent<MatchManager>();
        gameManager.AddComponent<LootManager>();
        gameManager.AddComponent<ProgressionManager>();
        gameManager.AddComponent<LocalizationManager>();
        gameManager.AddComponent<AnalyticsManager>();
        gameManager.AddComponent<SafeZone>();
        
        // Event system
        var evt = new GameObject("[AUTO] EventSystem");
        evt.AddComponent<UnityEngine.EventSystems.EventSystem>();
        evt.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        // Show splash/loading
        CreateSplashScreen(scene);
    }

    private void CreateSplashScreen(Scene scene)
    {
        var canvas = CreateCanvas(scene, "SplashCanvas", 0);
        
        // Try to load the actual splash screen image from your AI art
        var splashTexture = Resources.Load<Texture2D>("UI/Sprites/splash_screen");
        Color bgColor = splashTexture != null ? Color.white : new Color(0.039f, 0.086f, 0.157f, 1);
        
        var bg = CreateImage(canvas.transform, "SplashBG", bgColor, 
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 1));
        
        if (splashTexture != null)
        {
            var img = bg.GetComponent<Image>();
            img.sprite = Sprite.Create(splashTexture, new Rect(0, 0, splashTexture.width, splashTexture.height), new Vector2(0.5f, 0.5f));
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            Debug.Log("[SceneAutoBuilder] Loaded splash screen art!");
        }
        
        // Try to load the actual logo from your AI art
        var logoTexture = Resources.Load<Texture2D>("UI/Sprites/arena_fall_logo");
        var logo = CreateImage(bg.transform, "Logo", Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var rt = logo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 180);
        rt.anchoredPosition = new Vector2(0, 0);
        
        if (logoTexture != null)
        {
            var logoImg = logo.GetComponent<Image>();
            logoImg.sprite = Sprite.Create(logoTexture, new Rect(0, 0, logoTexture.width, logoTexture.height), new Vector2(0.5f, 0.5f));
            logoImg.type = Image.Type.Simple;
            logoImg.preserveAspect = true;
            Debug.Log("[SceneAutoBuilder] Loaded logo art!");
        }
        
        // Loading text
        var loadingText = CreateText(bg.transform, "LoadingText", "INITIALIZING...", 24, Color.gray,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var lrt = loadingText.GetComponent<RectTransform>();
        lrt.anchoredPosition = new Vector2(0, 40);
        
        // Auto-transition to MainMenu after brief delay
        var loader = bg.AddComponent<AutoSceneLoader>();
        loader.sceneName = "MainMenu";
        loader.delay = 2f;
    }

    // ─── MAIN MENU ─────────────────────────────────────────────
    private void BuildMainMenuScene(Scene scene)
    {
        BuildEssentialSystems(scene);
        
        var canvas = CreateCanvas(scene, "MainMenuCanvas", 0);
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Try to load your AI-generated menu background
        var bgTexture = Resources.Load<Texture2D>("Art/Environment/skybox_concept");
        var logoTexture = Resources.Load<Texture2D>("UI/Sprites/arena_fall_logo");
        var iconAtlas = Resources.Load<Texture2D>("UI/Icons/ui_icons_atlas");
        
        // Dark background with AI art overlay
        var bg = CreateImage(canvas.transform, "Background", new Color(0.039f, 0.086f, 0.157f, 1),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 1));
        
        // Background image overlay — your AI art if available
        var bgOverlay = CreateImage(bg.transform, "BGEffects", new Color(1, 1, 1, 0.4f),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 1));
        if (bgTexture != null)
        {
            var bgImg = bgOverlay.GetComponent<Image>();
            bgImg.sprite = Sprite.Create(bgTexture, new Rect(0, 0, bgTexture.width, bgTexture.height), new Vector2(0.5f, 0.5f));
            bgImg.type = Image.Type.Simple;
            bgImg.preserveAspect = false;
            Debug.Log("[SceneAutoBuilder] MainMenu: Loaded background art!");
        }
        // Dark overlay for readability
        var darkOverlay = CreateImage(bg.transform, "DarkOverlay", new Color(0, 0, 0, 0.6f),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 1));
        
        // Logo — your AI generated logo
        var logo = CreateImage(bg.transform, "Logo", Color.white,
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var lrt = logo.GetComponent<RectTransform>();
        lrt.sizeDelta = new Vector2(500, 200);
        if (logoTexture != null)
        {
            var logoImg = logo.GetComponent<Image>();
            logoImg.sprite = Sprite.Create(logoTexture, new Rect(0, 0, logoTexture.width, logoTexture.height), new Vector2(0.5f, 0.5f));
            logoImg.type = Image.Type.Simple;
            logoImg.preserveAspect = true;
            Debug.Log("[SceneAutoBuilder] MainMenu: Loaded logo art!");
        }
        
        // Player info bar at top
        var infoBar = CreatePanel(bg.transform, "PlayerInfoBar", new Color(0.02f, 0.04f, 0.08f, 0.8f),
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 1));
        var irt = infoBar.GetComponent<RectTransform>();
        irt.sizeDelta = new Vector2(0, 50);
        irt.anchoredPosition = new Vector2(0, 0);
        
        var playerName = CreateText(infoBar.transform, "PlayerName", "VANGUARD-01", 18, Color.white,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        var nrt = playerName.GetComponent<RectTransform>();
        nrt.anchoredPosition = new Vector2(20, 0);
        
        var levelText = CreateText(infoBar.transform, "LevelText", "LVL 1", 14, new Color(0, 0.83f, 1, 1),
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        var lvrt = levelText.GetComponent<RectTransform>();
        lvrt.anchoredPosition = new Vector2(200, 0);
        
        var creditsText = CreateText(infoBar.transform, "CreditsText", "1,000", 14, new Color(1, 0.42f, 0.21f, 1),
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
        var crt = creditsText.GetComponent<RectTransform>();
        crt.anchoredPosition = new Vector2(-20, 0);
        
        // === MODERN BATTLE ROYALE MENU LAYOUT ===
        // Left side: Main navigation buttons (vertical stack)
        var leftPanel = CreatePanel(bg.transform, "NavPanel", new Color(0, 0, 0, 0),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var lpanelR = leftPanel.GetComponent<RectTransform>();
        lpanelR.anchoredPosition = new Vector2(-200, -30);
        lpanelR.sizeDelta = new Vector2(280, 360);
        
        // Play button — LARGE, primary
        CreateMenuButton(lpanelR.transform, "PlayButton", "PLAY", 28, ButtonStyle.Primary, 
            new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -10), new Vector2(260, 60), () => {
                SceneManager.LoadScene("Lobby");
            });
        
        // Mode buttons
        CreateMenuButton(lpanelR.transform, "SoloButton", "SOLO", 20, ButtonStyle.Secondary,
            new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -80), new Vector2(260, 50), () => {
                SceneManager.LoadScene("GameMap");
            });
        
        CreateMenuButton(lpanelR.transform, "DuosButton", "DUOS", 20, ButtonStyle.Secondary,
            new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -140), new Vector2(260, 50), null);
        
        CreateMenuButton(lpanelR.transform, "SquadsButton", "SQUADS", 20, ButtonStyle.Secondary,
            new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -200), new Vector2(260, 50), null);
        
        // Right side: Secondary actions
        var rightPanel = CreatePanel(bg.transform, "ActionPanel", new Color(0, 0, 0, 0),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var rpanelR = rightPanel.GetComponent<RectTransform>();
        rpanelR.anchoredPosition = new Vector2(200, -30);
        rpanelR.sizeDelta = new Vector2(280, 360);
        
        CreateMenuButton(rpanelR.transform, "TrainingButton", "TRAINING", 18, ButtonStyle.Tertiary,
            new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -10), new Vector2(260, 50), () => {
                SceneManager.LoadScene("TrainingGround");
            });
        
        CreateMenuButton(rpanelR.transform, "CustomizeButton", "CUSTOMIZE", 18, ButtonStyle.Tertiary,
            new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -70), new Vector2(260, 50), null);
        
        CreateMenuButton(rpanelR.transform, "LoadoutButton", "LOADOUT", 18, ButtonStyle.Tertiary,
            new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -130), new Vector2(260, 50), null);
        
        // Bottom bar
        var bottomBar = CreatePanel(bg.transform, "BottomBar", new Color(0.02f, 0.04f, 0.08f, 0.9f),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 0));
        var brt = bottomBar.GetComponent<RectTransform>();
        brt.sizeDelta = new Vector2(0, 60);
        
        CreateMenuButton(bottomBar.transform, "ShopButton", "SHOP", 16, ButtonStyle.Tertiary,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-100, 0), new Vector2(140, 40), null);
        
        CreateMenuButton(bottomBar.transform, "BattlePassButton", "BATTLE PASS", 16, ButtonStyle.Accent,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(100, 0), new Vector2(180, 40), null);
        
        CreateMenuButton(bottomBar.transform, "SettingsButton", "⚙", 20, ButtonStyle.Tertiary,
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-20, 0), new Vector2(50, 40), () => {
                SceneManager.LoadScene("Settings");
            });
        
        CreateMenuButton(bottomBar.transform, "QuitButton", "✕", 18, ButtonStyle.Danger,
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-80, 0), new Vector2(40, 40), () => {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            });
    }

    // ─── GAME MAP (Battle Royale) ──────────────────────────────
    private void BuildGameMapScene(Scene scene)
    {
        // Create terrain
        var terrainObj = new GameObject("[AUTO] Terrain");
        var terrain = terrainObj.AddComponent<Terrain>();
        var terrainData = new TerrainData();
        terrainData.size = new Vector3(4000, 200, 4000);
        terrainData.heightmapResolution = 512;
        terrain.terrainData = terrainData;
        
        var terrainCollider = terrainObj.AddComponent<TerrainCollider>();
        terrainCollider.terrainData = terrainData;
        
        // Directional light
        var lightObj = new GameObject("[AUTO] Directional Light");
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(0.9f, 0.85f, 0.75f);
        light.intensity = 1.2f;
        lightObj.transform.rotation = Quaternion.Euler(40, 120, 0);
        light.shadows = LightShadows.Soft;
        
        // Player spawn
        var playerObj = new GameObject("[AUTO] Player");
        playerObj.transform.position = new Vector3(2000, 5, 2000);
        
        // Character controller
        var cc = playerObj.AddComponent<CharacterController>();
        cc.height = 2;
        cc.radius = 0.4f;
        cc.center = new Vector3(0, 1, 0);
        
        var controller = playerObj.AddComponent<PlayerCharacterController>();
        var health = playerObj.AddComponent<CharacterHealth>();
        var inventory = playerObj.AddComponent<Inventory>();
        
        // Camera
        var camObj = new GameObject("[AUTO] MainCamera");
        var cam = camObj.AddComponent<Camera>();
        cam.fieldOfView = 70;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 5000;
        camObj.tag = "MainCamera";
        var audioListener = camObj.AddComponent<AudioListener>();
        
        // Camera manager setup
        var camManager = FindObjectOfType<CameraManager>();
        if (camManager != null)
        {
            camManager.SetTarget(playerObj.transform);
        }
        
        // Safe zone
        var zoneObj = new GameObject("[AUTO] SafeZone");
        zoneObj.AddComponent<SafeZone>();
        
        // Audio source for player
        var audioSource = playerObj.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1;
        
        // HUD
        BuildHUD(scene);
        
        // Spawn AI bots
        SpawnAIBots();
        
        // Spawn loot
        SpawnLoot();
        
        // Event system
        var evt = new GameObject("[AUTO] EventSystem");
        evt.AddComponent<UnityEngine.EventSystems.EventSystem>();
        evt.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    // ─── HUD ──────────────────────────────────────────────────
    private void BuildHUD(Scene scene)
    {
        var canvas = CreateCanvas(scene, "HUDCanvas", 10);
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        
        // === TOP BAR ===
        var topBar = CreatePanel(canvas.transform, "TopBar", new Color(0, 0, 0, 0.5f),
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 1));
        var tbrt = topBar.GetComponent<RectTransform>();
        tbrt.sizeDelta = new Vector2(0, 50);
        
        // Player count
        var pCount = CreateText(topBar.transform, "PlayerCount", "ALIVE: 60", 14, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        
        // Minimap (top right) — using your AI HUD art
        var minimap = CreatePanel(canvas.transform, "Minimap", new Color(0, 0, 0, 0.6f),
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        var mrt = minimap.GetComponent<RectTransform>();
        mrt.sizeDelta = new Vector2(180, 180);
        mrt.anchoredPosition = new Vector2(-15, -15);
        
        // Try loading the modern BR HUD background for minimap
        var hudTex = Resources.Load<Texture2D>("UI/Sprites/modern_br_hud");
        var hudElementsTex = Resources.Load<Texture2D>("UI/Sprites/hud_elements");
        
        // Minimap border — from your AI HUD elements
        var mmBorder = CreateImage(minimap.transform, "MinimapBorder", new Color(0, 0.83f, 1, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var brt2 = mmBorder.GetComponent<RectTransform>();
        brt2.sizeDelta = new Vector2(178, 178);
        if (hudElementsTex != null)
        {
            var borderImg = mmBorder.GetComponent<Image>();
            borderImg.sprite = Sprite.Create(hudElementsTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
            borderImg.type = Image.Type.Sliced;
            Debug.Log("[SceneAutoBuilder] HUD: Loaded minimap border art!");
        }
        else
        {
            mmBorder.GetComponent<Image>().fillCenter = false;
            mmBorder.GetComponent<Image>().type = Image.Type.Sliced;
        }
        
        // Compass bar (top center area)
        var compass = CreatePanel(canvas.transform, "CompassBar", new Color(0, 0, 0, 0.4f),
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        var crt2 = compass.GetComponent<RectTransform>();
        crt2.sizeDelta = new Vector2(400, 20);
        crt2.anchoredPosition = new Vector2(0, -60);
        
        // Compass text labels
        string[] dirs = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        for (int i = 0; i < 8; i++)
        {
            float xPos = -175 + (i * 50);
            var dirText = CreateText(compass.transform, dirs[i], dirs[i], 10, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            var drt = dirText.GetComponent<RectTransform>();
            drt.anchoredPosition = new Vector2(xPos, 0);
        }
        
        // === BOTTOM CENTER ===
        // Health panel
        var healthPanel = CreatePanel(canvas.transform, "HealthPanel", new Color(0, 0, 0, 0),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var hprt = healthPanel.GetComponent<RectTransform>();
        hprt.sizeDelta = new Vector2(350, 100);
        hprt.anchoredPosition = new Vector2(0, 30);
        
        // Health bar
        var healthBarBg = CreateImage(healthPanel.transform, "HealthBarBG", new Color(0.2f, 0.2f, 0.2f, 0.6f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var hbrt = healthBarBg.GetComponent<RectTransform>();
        hbrt.sizeDelta = new Vector2(200, 14);
        hbrt.anchoredPosition = new Vector2(0, 15);
        
        var healthBar = CreateImage(healthBarBg.transform, "HealthBarFill", new Color(0.27f, 0.85f, 0.27f, 1),
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        var hfrt = healthBar.GetComponent<RectTransform>();
        hfrt.sizeDelta = new Vector2(200, 14);
        hfrt.GetComponent<Image>().type = Image.Type.Filled;
        hfrt.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;
        
        var hpText = CreateText(healthBarBg.transform, "HPText", "100", 12, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        
        // Shield bar
        var shieldBarBg = CreateImage(healthPanel.transform, "ShieldBarBG", new Color(0.2f, 0.2f, 0.2f, 0.6f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var sbrt = shieldBarBg.GetComponent<RectTransform>();
        sbrt.sizeDelta = new Vector2(200, 10);
        sbrt.anchoredPosition = new Vector2(0, 0);
        
        var shieldBar = CreateImage(shieldBarBg.transform, "ShieldBarFill", new Color(0, 0.83f, 1, 1),
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        var sfrt = shieldBar.GetComponent<RectTransform>();
        sfrt.sizeDelta = new Vector2(200, 10);
        sfrt.GetComponent<Image>().type = Image.Type.Filled;
        sfrt.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;
        
        // Weapon display — shows your AI weapon art!
        var weaponPanel = CreatePanel(healthPanel.transform, "WeaponDisplay", new Color(0, 0, 0, 0.5f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var wprt = weaponPanel.GetComponent<RectTransform>();
        wprt.sizeDelta = new Vector2(320, 70);
        wprt.anchoredPosition = new Vector2(0, -50);
        
        // Weapon icon — using your AI assault rifle art
        var weaponIcon = CreateImage(weaponPanel.transform, "WeaponIcon", Color.white,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        var wirt = weaponIcon.GetComponent<RectTransform>();
        wirt.sizeDelta = new Vector2(80, 50);
        wirt.anchoredPosition = new Vector2(10, 0);
        var weaponArtTex = Resources.Load<Texture2D>("Art/Weapons/AssaultRifles/a17_striker");
        if (weaponArtTex != null)
        {
            var wImg = weaponIcon.GetComponent<Image>();
            wImg.sprite = Sprite.Create(weaponArtTex, new Rect(0, 0, weaponArtTex.width, weaponArtTex.height), new Vector2(0.5f, 0.5f));
            wImg.type = Image.Type.Simple;
            wImg.preserveAspect = true;
        }
        
        var weaponName = CreateText(weaponPanel.transform, "WeaponName", "A-17 STRIKER", 16, Color.white,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        var wnrt = weaponName.GetComponent<RectTransform>();
        wnrt.anchoredPosition = new Vector2(100, 10);
        
        var ammoText = CreateText(weaponPanel.transform, "AmmoText", "30 / 120", 22, Color.white,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        var art2 = ammoText.GetComponent<RectTransform>();
        art2.anchoredPosition = new Vector2(100, -15);
        
        // === RIGHT SIDE ===
        // Kill feed
        var killFeed = CreatePanel(canvas.transform, "KillFeed", new Color(0, 0, 0, 0),
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        var kfrt = killFeed.GetComponent<RectTransform>();
        kfrt.sizeDelta = new Vector2(300, 200);
        kfrt.anchoredPosition = new Vector2(-210, -80);
        
        // Crosshair (center) — using your AI crosshair asset
        var crosshairPanel = CreatePanel(canvas.transform, "Crosshair", new Color(0, 0, 0, 0),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var crt3 = crosshairPanel.GetComponent<RectTransform>();
        crt3.sizeDelta = new Vector2(48, 48);
        
        // Try loading your AI crosshair sprite
        var crosshairTex = Resources.Load<Texture2D>("UI/Sprites/crosshairs");
        if (crosshairTex != null)
        {
            var crossImg = crosshairPanel.AddComponent<Image>();
            crossImg.sprite = Sprite.Create(crosshairTex, new Rect(0, 0, 48, 48), new Vector2(0.5f, 0.5f));
            crossImg.type = Image.Type.Simple;
            crossImg.color = new Color(0, 0.83f, 1, 0.9f);
            Debug.Log("[SceneAutoBuilder] HUD: Loaded crosshair art!");
        }
        else
        {
            // Fallback: procedural crosshair
            // Center dot
            var dot = CreateImage(crosshairPanel.transform, "CenterDot", new Color(0, 0.83f, 1, 0.8f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            var drt2 = dot.GetComponent<RectTransform>();
            drt2.sizeDelta = new Vector2(3, 3);
            
            // Cross lines
            for (int i = 0; i < 4; i++)
            {
                var line = CreateImage(crosshairPanel.transform, $"Line{i}", new Color(0, 0.83f, 1, 0.7f),
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                var lrt = line.GetComponent<RectTransform>();
                float x = i < 2 ? 5 : 0;
                float y = i >= 2 ? 5 : 0;
                float w = i < 2 ? 10 : 2;
                float h = i < 2 ? 2 : 10;
                if (i == 1) x = -8;
                if (i == 3) y = -8;
                lrt.anchoredPosition = new Vector2(x, y);
                lrt.sizeDelta = new Vector2(w, h);
            }
        }
        
        // Interaction prompt (center bottom)
        var interact = CreatePanel(canvas.transform, "InteractPrompt", new Color(0, 0, 0, 0.6f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var irt2 = interact.GetComponent<RectTransform>();
        irt2.sizeDelta = new Vector2(300, 40);
        irt2.anchoredPosition = new Vector2(0, -80);
        interact.SetActive(false);
        
        var interactText = CreateText(interact.transform, "InteractText", "Press [F] to pick up", 14, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        
        // Add HUD Controller component to canvas
        var hudController = canvas.AddComponent<HUDController>();
    }

    // ─── LOBBY ─────────────────────────────────────────────────
    private void BuildLobbyScene(Scene scene)
    {
        BuildEssentialSystems(scene);
        
        var canvas = CreateCanvas(scene, "LobbyCanvas", 0);
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Try loading terrain or nexus tower as lobby background
        var lobbyBgTex = Resources.Load<Texture2D>("Art/Environment/terrain_concept");
        var bg = CreateImage(canvas.transform, "Background", new Color(0.039f, 0.086f, 0.157f, 1),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 1));
        if (lobbyBgTex != null)
        {
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = Sprite.Create(lobbyBgTex, new Rect(0, 0, lobbyBgTex.width, lobbyBgTex.height), new Vector2(0.5f, 0.5f));
            bgImg.type = Image.Type.Simple;
            bgImg.preserveAspect = false;
        }
        var lobbyOverlay = CreateImage(bg.transform, "Overlay", new Color(0, 0, 0, 0.5f),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 1));
        
        // Lobby title
        var title = CreateText(bg.transform, "Title", "MATCH LOBBY", 32, new Color(0, 0.83f, 1, 1),
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        var trt = title.GetComponent<RectTransform>();
        trt.anchoredPosition = new Vector2(0, -30);
        
        // Player list panel
        var playerList = CreatePanel(bg.transform, "PlayerList", new Color(0, 0, 0, 0.3f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var plrt = playerList.GetComponent<RectTransform>();
        plrt.sizeDelta = new Vector2(400, 300);
        plrt.anchoredPosition = new Vector2(0, 30);
        
        // Player entries
        for (int i = 1; i <= 4; i++)
        {
            var entry = CreatePanel(playerList.transform, $"Player{i}", new Color(0.1f, 0.15f, 0.25f, 0.5f),
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            var entryRt = entry.GetComponent<RectTransform>();
            entryRt.sizeDelta = new Vector2(360, 50);
            entryRt.anchoredPosition = new Vector2(0, -10 - ((i - 1) * 55));
            
            CreateText(entry.transform, "Name", i == 1 ? "YOU" : $"PLAYER {i}", 16, 
                i == 1 ? new Color(0, 0.83f, 1, 1) : Color.white,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
            
            if (i == 1)
            {
                var ready = CreateText(entry.transform, "Status", "READY", 12, new Color(0.27f, 0.85f, 0.27f, 1),
                    new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
                var rrt = ready.GetComponent<RectTransform>();
                rrt.anchoredPosition = new Vector2(-15, 0);
            }
        }
        
        // Start button
        CreateMenuButton(bg.transform, "StartButton", "START MATCH", 22, ButtonStyle.Primary,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 40), new Vector2(280, 55), () => {
                SceneManager.LoadScene("GameMap");
            });
        
        // Back
        CreateMenuButton(bg.transform, "BackButton", "← BACK", 16, ButtonStyle.Tertiary,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(15, -15), new Vector2(100, 35), () => {
                SceneManager.LoadScene("MainMenu");
            });
    }

    // ─── TRAINING ──────────────────────────────────────────────
    private void BuildTrainingScene(Scene scene)
    {
        // Same as game map but simpler - ground plane
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "[AUTO] Ground";
        ground.transform.localScale = new Vector3(50, 1, 50);
        ground.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        ground.GetComponent<MeshRenderer>().material.color = new Color(0.3f, 0.35f, 0.3f);
        
        // Player
        var playerObj = new GameObject("[AUTO] Player");
        playerObj.transform.position = new Vector3(0, 1, 0);
        var cc = playerObj.AddComponent<CharacterController>();
        cc.height = 2;
        cc.radius = 0.4f;
        cc.center = new Vector3(0, 1, 0);
        playerObj.AddComponent<PlayerCharacterController>();
        playerObj.AddComponent<CharacterHealth>();
        playerObj.AddComponent<Inventory>();
        
        // Target dummies
        for (int i = 0; i < 5; i++)
        {
            var dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dummy.name = $"[AUTO] TargetDummy_{i}";
            dummy.transform.position = new Vector3(10 + (i * 5), 1, 5);
            dummy.transform.localScale = new Vector3(0.5f, 1, 0.5f);
            var renderer = dummy.GetComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = i % 2 == 0 ? new Color(1, 0.2f, 0.2f) : new Color(1, 0.6f, 0);
            
            // Target health
            var targetHealth = dummy.AddComponent<CharacterHealth>();
            
            // Add a little rotate animation
            dummy.AddComponent<RotateAnimation>();
        }
        
        // Camera
        var camObj = new GameObject("[AUTO] MainCamera");
        var cam = camObj.AddComponent<Camera>();
        cam.fieldOfView = 70;
        camObj.tag = "MainCamera";
        camObj.AddComponent<AudioListener>();
        
        var camManager = FindObjectOfType<CameraManager>();
        if (camManager != null) camManager.SetTarget(playerObj.transform);
        
        // HUD
        BuildHUD(scene);
        
        // Event system
        var evt = new GameObject("[AUTO] EventSystem");
        evt.AddComponent<UnityEngine.EventSystems.EventSystem>();
        evt.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        // Back button
        var canvas = CreateCanvas(scene, "UICanvas", 1);
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        
        CreateMenuButton(canvas.transform, "BackButton", "← MAIN MENU", 16, ButtonStyle.Tertiary,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(15, -15), new Vector2(140, 35), () => {
                SceneManager.LoadScene("MainMenu");
            });
    }

    // ─── RESULT SCREEN ──────────────────────────────────────────
    private void BuildResultScene(Scene scene)
    {
        BuildEssentialSystems(scene);
        
        var canvas = CreateCanvas(scene, "ResultCanvas", 0);
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Result screen background with AI art
        var resultBgTex = Resources.Load<Texture2D>("Art/Environment/skybox_concept");
        var bg = CreateImage(canvas.transform, "Background", new Color(0.039f, 0.086f, 0.157f, 1),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 1));
        if (resultBgTex != null)
        {
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = Sprite.Create(resultBgTex, new Rect(0, 0, resultBgTex.width, resultBgTex.height), new Vector2(0.5f, 0.5f));
            bgImg.type = Image.Type.Simple;
            bgImg.preserveAspect = false;
        }
        var resultOverlay = CreateImage(bg.transform, "Overlay", new Color(0, 0, 0, 0.7f),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 1));
        
        // Result header
        var header = CreateText(bg.transform, "Header", "MATCH RESULTS", 28, new Color(1, 0.42f, 0.21f, 1),
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        var hrt = header.GetComponent<RectTransform>();
        hrt.anchoredPosition = new Vector2(0, -40);
        
        // Placement
        var placement = CreateText(bg.transform, "Placement", "#1", 64, new Color(0, 0.83f, 1, 1),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var prt = placement.GetComponent<RectTransform>();
        prt.anchoredPosition = new Vector2(0, 60);
        
        var placementLabel = CreateText(bg.transform, "PlacementLabel", "PLACEMENT", 14, Color.gray,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var plrt2 = placementLabel.GetComponent<RectTransform>();
        plrt2.anchoredPosition = new Vector2(0, 20);
        
        // Stats grid
        string[] stats = { "KILLS", "DAMAGE", "SURVIVED", "SCORE" };
        string[] values = { "5", "1,240", "12:30", "1,850" };
        for (int i = 0; i < 4; i++)
        {
            var statPanel = CreatePanel(bg.transform, $"Stat_{i}", new Color(0.1f, 0.15f, 0.25f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            var srt = statPanel.GetComponent<RectTransform>();
            srt.sizeDelta = new Vector2(120, 80);
            srt.anchoredPosition = new Vector2(-190 + (i * 125), -30);
            
            CreateText(statPanel.transform, "Value", values[i], 28, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            
            CreateText(statPanel.transform, "Label", stats[i], 10, Color.gray,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        }
        
        // XP earned
        var xpText = CreateText(bg.transform, "XPEarned", "+ 850 XP", 20, new Color(0.27f, 0.85f, 0.27f, 1),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var xrt = xpText.GetComponent<RectTransform>();
        xrt.anchoredPosition = new Vector2(0, -130);
        
        // Buttons
        CreateMenuButton(bg.transform, "LobbyButton", "PLAY AGAIN", 20, ButtonStyle.Primary,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(-80, 40), new Vector2(200, 50), () => {
                SceneManager.LoadScene("GameMap");
            });
        
        CreateMenuButton(bg.transform, "MenuButton", "MAIN MENU", 16, ButtonStyle.Secondary,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(110, 40), new Vector2(200, 50), () => {
                SceneManager.LoadScene("MainMenu");
            });
    }

    // ─── SETTINGS ────────────────────────────────────────────────
    private void BuildSettingsScene(Scene scene)
    {
        BuildEssentialSystems(scene);
        
        var canvas = CreateCanvas(scene, "SettingsCanvas", 0);
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        
        var settingsBgTex = Resources.Load<Texture2D>("Art/Environment/terrain_concept");
        var bg = CreateImage(canvas.transform, "Background", new Color(0.039f, 0.086f, 0.157f, 1),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 1));
        if (settingsBgTex != null)
        {
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = Sprite.Create(settingsBgTex, new Rect(0, 0, settingsBgTex.width, settingsBgTex.height), new Vector2(0.5f, 0.5f));
            bgImg.type = Image.Type.Simple;
        }
        var settingsOverlay = CreateImage(bg.transform, "Overlay", new Color(0, 0, 0, 0.6f),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 1));
        
        var title = CreateText(bg.transform, "Title", "SETTINGS", 28, new Color(0, 0.83f, 1, 1),
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        var trt2 = title.GetComponent<RectTransform>();
        trt2.anchoredPosition = new Vector2(0, -30);
        
        // Audio section
        CreateSettingsSlider(bg.transform, "MasterVolume", "MASTER VOLUME", 0.8f, 0, new Vector2(0, -80));
        CreateSettingsSlider(bg.transform, "MusicVolume", "MUSIC VOLUME", 0.7f, 1, new Vector2(0, -130));
        CreateSettingsSlider(bg.transform, "SFXVolume", "SFX VOLUME", 0.8f, 2, new Vector2(0, -180));
        
        // Sensitivity
        CreateSettingsSlider(bg.transform, "Sensitivity", "LOOK SENSITIVITY", 0.5f, 3, new Vector2(0, -240));
        
        // Back
        CreateMenuButton(bg.transform, "BackButton", "← BACK", 18, ButtonStyle.Tertiary,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 40), new Vector2(200, 50), () => {
                SceneManager.LoadScene("MainMenu");
            });
    }

    // ─── TEST SCENE ──────────────────────────────────────────────
    private void BuildTestScene(Scene scene)
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "[AUTO] Ground";
        ground.transform.localScale = new Vector3(20, 1, 20);
        ground.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        
        // Player spawn
        var playerObj = new GameObject("[AUTO] Player");
        playerObj.transform.position = new Vector3(0, 1, 0);
        var cc = playerObj.AddComponent<CharacterController>();
        cc.height = 2;
        cc.radius = 0.4f;
        cc.center = new Vector3(0, 1, 0);
        playerObj.AddComponent<PlayerCharacterController>();
        
        // Test cubes
        for (int i = 0; i < 5; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"[AUTO] TestCube_{i}";
            cube.transform.position = new Vector3(-3 + (i * 1.5f), 0.5f, 3);
            cube.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.red, Color.blue, i / 4f);
        }
        
        // Camera
        var camObj = new GameObject("[AUTO] MainCamera");
        var cam = camObj.AddComponent<Camera>();
        cam.fieldOfView = 70;
        camObj.tag = "MainCamera";
        camObj.AddComponent<AudioListener>();
        camObj.transform.position = new Vector3(0, 3, -5);
        camObj.transform.LookAt(Vector3.zero);
        
        var camManager = FindObjectOfType<CameraManager>();
        if (camManager != null) camManager.SetTarget(playerObj.transform);
        
        // Event system
        var evt = new GameObject("[AUTO] EventSystem");
        evt.AddComponent<UnityEngine.EventSystems.EventSystem>();
        evt.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    // ─── AI BOTS ──────────────────────────────────────────────
    private void SpawnAIBots()
    {
        // Try loading your AI bot character art
        var botTex = Resources.Load<Texture2D>("Art/Characters/NPCs/ai_bot");
        var maleTex = Resources.Load<Texture2D>("Art/Characters/Male/male_character_front");
        var femaleTex = Resources.Load<Texture2D>("Art/Characters/Female/female_character_front");
        
        for (int i = 0; i < 10; i++)
        {
            var bot = new GameObject($"[AUTO] AIBot_{i}");
            float x = Random.Range(500, 3500);
            float z = Random.Range(500, 3500);
            bot.transform.position = new Vector3(x, 1, z);
            
            var cc = bot.AddComponent<CharacterController>();
            cc.height = 2;
            cc.radius = 0.4f;
            cc.center = new Vector3(0, 1, 0);
            
            bot.AddComponent<CharacterHealth>();
            
            // Display your AI character art as a billboard sprite
            var billboard = GameObject.CreatePrimitive(PrimitiveType.Quad);
            billboard.name = "CharacterArt";
            billboard.transform.SetParent(bot.transform);
            billboard.transform.localPosition = new Vector3(0, 1.5f, 0);
            billboard.transform.localScale = new Vector3(1.5f, 2, 1);
            
            var renderer = billboard.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            
            Texture2D charTex = null;
            if (i < 3 && botTex != null) charTex = botTex;
            else if (i < 6 && maleTex != null) charTex = maleTex;
            else if (femaleTex != null) charTex = femaleTex;
            
            if (charTex != null)
            {
                mat.mainTexture = charTex;
                mat.color = Color.white;
                renderer.material = mat;
                // Make it face the camera (billboard)
                billboard.AddComponent<BillboardToCamera>();
            }
            else
            {
                renderer.material.color = new Color(0.8f, 0.2f, 0.2f);
            }
            
            // AI controller needs NavMesh
            // bot.AddComponent<AIController>();
        }
        Debug.Log("[SceneAutoBuilder] Spawned 10 AI bots with character art");
    }

    // ─── LOOT ──────────────────────────────────────────────────
    private void SpawnLoot()
    {
        // Available weapon IDs matching your AI art filenames
        string[] weaponArtIds = { 
            "a17_striker", "a23_phantom", "a41_vanguard", 
            "s9_viper", "s14_stinger", 
            "sg12_breaker", "sg20_devastator",
            "sr25_longshot", "sr40_eliminator",
            "lmg60_suppressor", "lmg80_storm",
            "p25_sidearm", "p38_heavy",
            "combat_knife", "energy_blade", "impact_staff"
        };
        
        for (int i = 0; i < 30; i++)
        {
            var loot = GameObject.CreatePrimitive(PrimitiveType.Quad);
            loot.name = $"[AUTO] LootItem_{i}";
            float x = Random.Range(200, 3800);
            float z = Random.Range(200, 3800);
            loot.transform.position = new Vector3(x, 1.5f, z);
            loot.transform.localScale = new Vector3(1, 1, 1);
            
            // Try loading your actual weapon art for this loot item
            string weaponId = weaponArtIds[Random.Range(0, weaponArtIds.Length)];
            var weaponTex = Resources.Load<Texture2D>($"Art/Weapons/AssaultRifles/{weaponId}");
            if (weaponTex == null) weaponTex = Resources.Load<Texture2D>($"Art/Weapons/SMGs/{weaponId}");
            if (weaponTex == null) weaponTex = Resources.Load<Texture2D>($"Art/Weapons/Shotguns/{weaponId}");
            if (weaponTex == null) weaponTex = Resources.Load<Texture2D>($"Art/Weapons/Snipers/{weaponId}");
            if (weaponTex == null) weaponTex = Resources.Load<Texture2D>($"Art/Weapons/LMGs/{weaponId}");
            if (weaponTex == null) weaponTex = Resources.Load<Texture2D>($"Art/Weapons/Pistols/{weaponId}");
            if (weaponTex == null) weaponTex = Resources.Load<Texture2D>($"Art/Weapons/Melee/{weaponId}");
            if (weaponTex == null) weaponTex = Resources.Load<Texture2D>($"Art/Weapons/Throwables/frag_grenade");
            
            var renderer = loot.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (weaponTex != null)
            {
                mat.mainTexture = weaponTex;
                renderer.material = mat;
                Debug.Log($"[SceneAutoBuilder] Loot: Loaded weapon art {weaponId}");
            }
            else
            {
                renderer.material.color = RandomColor();
            }
            
            loot.AddComponent<BoxCollider>();
            loot.AddComponent<LootItem>();
            
            // Add float animation
            loot.AddComponent<LootFloatAnimation>();
        }
        Debug.Log("[SceneAutoBuilder] Spawned 30 loot items with weapon art");
    }

    // ─── ESSENTIAL SYSTEMS ──────────────────────────────────────
    private void BuildEssentialSystems(Scene scene)
    {
        // Check if key systems exist, if not create them
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var evt = new GameObject("[AUTO] EventSystem");
            evt.AddComponent<UnityEngine.EventSystems.EventSystem>();
            evt.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    // ─── HELPERS ───────────────────────────────────────────────

    private GameObject CreateCanvas(Scene scene, string name, int sortOrder)
    {
        var obj = new GameObject($"[AUTO] {name}", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = obj.GetComponent<Canvas>();
        canvas.sortingOrder = sortOrder;
        var scaler = obj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        SceneManager.MoveGameObjectToScene(obj, scene);
        return obj;
    }

    private GameObject CreatePanel(Transform parent, string name, Color color, 
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivotMin, Vector2 pivotMax)
    {
        var obj = new GameObject($"[AUTO] {name}", typeof(RectTransform), typeof(CanvasRenderer));
        var image = obj.AddComponent<Image>();
        image.color = color;
        var rt = obj.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivotMin;
        return obj;
    }

    private GameObject CreateImage(Transform parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivotMin, Vector2 pivotMax)
    {
        var obj = new GameObject($"[AUTO] {name}", typeof(RectTransform), typeof(CanvasRenderer));
        var image = obj.AddComponent<Image>();
        image.color = color;
        var rt = obj.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivotMin;
        
        // AUTO-LOAD YOUR AI ART: Try to find a matching sprite in Resources
        TryLoadArtSprite(image, name);
        
        return obj;
    }
    
    /// <summary>
    /// Attempts to load your AI-generated art from Resources by matching the image name.
    /// Looks in: Resources/Art/, Resources/UI/Sprites/, Resources/UI/Icons/
    /// Falls back to the solid color if no art is found.
    /// </summary>
    private void TryLoadArtSprite(Image image, string assetName)
    {
        // Map common UI names to your actual AI art filenames
        string artPath = assetName switch
        {
            "Logo" or "SplashBG" => "UI/Sprites/arena_fall_logo",
            "SplashBG" => "UI/Sprites/splash_screen",
            "MinimapBorder" => "UI/Sprites/modern_br_hud",
            "BGEffects" or "Background" => "Art/Environment/skybox_concept",
            "Crosshair" => "UI/Sprites/crosshairs",
            _ => null
        };
        
        if (artPath == null) return;
        
        var tex = Resources.Load<Texture2D>(artPath);
        if (tex != null)
        {
            image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            Debug.Log($"[SceneAutoBuilder] Loaded art: {artPath}");
        }
    }

    private GameObject CreateText(Transform parent, string name, string text, int fontSize, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivotMin, Vector2 pivotMax)
    {
        var obj = new GameObject($"[AUTO] {name}", typeof(RectTransform), typeof(CanvasRenderer));
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.font = Resources.Load<TMP_FontAsset>("LiberationSans SDF");
        var rt = obj.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivotMin;
        rt.sizeDelta = new Vector2(200, 30);
        return obj;
    }

    private enum ButtonStyle { Primary, Secondary, Tertiary, Accent, Danger }

    private void CreateMenuButton(Transform parent, string name, string label, int fontSize, 
        ButtonStyle style, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivotMin, Vector2 pivotMax,
        Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        var buttonObj = new GameObject($"[AUTO] {name}", typeof(RectTransform), typeof(CanvasRenderer));
        var image = buttonObj.AddComponent<Image>();
        var button = buttonObj.AddComponent<Button>();
        var rt = buttonObj.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivotMin;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        // Style
        switch (style)
        {
            case ButtonStyle.Primary:
                image.color = new Color(1, 0.42f, 0.21f);
                break;
            case ButtonStyle.Secondary:
                image.color = new Color(0, 0.2f, 0.3f);
                break;
            case ButtonStyle.Tertiary:
                image.color = new Color(0.08f, 0.12f, 0.2f);
                break;
            case ButtonStyle.Accent:
                image.color = new Color(0, 0.5f, 0.7f);
                break;
            case ButtonStyle.Danger:
                image.color = new Color(0.5f, 0.1f, 0.1f);
                break;
        }

        // Text
        var textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer));
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (style == ButtonStyle.Primary || style == ButtonStyle.Accent)
            tmp.fontStyle = FontStyles.Bold;
        var trt = textObj.GetComponent<RectTransform>();
        trt.SetParent(buttonObj.transform, false);
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        trt.anchoredPosition = Vector2.zero;

        // Click handler
        if (onClick != null)
        {
            button.onClick.AddListener(onClick);
        }

        // Hover effects
        var colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(image.color.r * 1.2f, image.color.g * 1.2f, image.color.b * 1.2f, 1);
        colors.pressedColor = new Color(image.color.r * 0.7f, image.color.g * 0.7f, image.color.b * 0.7f, 1);
        colors.selectedColor = image.color;
        colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        colors.fadeDuration = 0.1f;
        button.colors = colors;
    }

    private void CreateSettingsSlider(Transform parent, string name, string label, float defaultValue, int index, Vector2 position)
    {
        var panel = CreatePanel(parent, name + "Panel", new Color(0, 0, 0, 0),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        var prt = panel.GetComponent<RectTransform>();
        prt.sizeDelta = new Vector2(500, 40);
        prt.anchoredPosition = position;

        CreateText(panel.transform, "Label", label, 16, Color.white,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));

        var sliderObj = new GameObject("Slider", typeof(RectTransform), typeof(CanvasRenderer));
        var slider = sliderObj.AddComponent<Slider>();
        var srt = sliderObj.GetComponent<RectTransform>();
        srt.SetParent(panel.transform, false);
        srt.anchorMin = new Vector2(1, 0.5f);
        srt.anchorMax = new Vector2(1, 0.5f);
        srt.pivot = new Vector2(1, 0.5f);
        srt.anchoredPosition = new Vector2(-10, 0);
        srt.sizeDelta = new Vector2(250, 20);

        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = defaultValue;
        slider.direction = Slider.Direction.LeftToRight;

        // Background
        var bgImage = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer));
        var bgImgComp = bgImage.AddComponent<Image>();
        bgImgComp.color = new Color(0.15f, 0.15f, 0.15f, 0.5f);
        var brt = bgImage.GetComponent<RectTransform>();
        brt.SetParent(sliderObj.transform, false);
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.sizeDelta = new Vector2(0, 6);
        slider.targetGraphic = bgImgComp;

        // Fill
        var fillImage = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer));
        var fillImgComp = fillImage.AddComponent<Image>();
        fillImgComp.color = new Color(0, 0.83f, 1, 1);
        var frt = fillImage.GetComponent<RectTransform>();
        frt.SetParent(sliderObj.transform, false);
        frt.anchorMin = new Vector2(0, 0);
        frt.anchorMax = new Vector2(0, 1);
        frt.sizeDelta = new Vector2(0, 6);
        slider.fillRect = frt;

        // Handle
        var handleImage = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer));
        var handleImgComp = handleImage.AddComponent<Image>();
        handleImgComp.color = new Color(1, 0.42f, 0.21f, 1);
        var hrt = handleImage.GetComponent<RectTransform>();
        hrt.SetParent(sliderObj.transform, false);
        hrt.sizeDelta = new Vector2(16, 16);
        slider.handleRect = hrt;
    }

    private Color RandomColor()
    {
        Color[] colors = {
            new Color(1, 0.42f, 0.21f), // Orange
            new Color(0, 0.83f, 1),     // Cyan
            new Color(0.27f, 0.85f, 0.27f), // Green
            new Color(0.8f, 0.3f, 0.8f), // Purple
            new Color(1, 0.7f, 0)       // Gold
        };
        return colors[Random.Range(0, colors.Length)];
    }
}

/// <summary>
/// Auto-loader to transition scenes after a delay
/// </summary>
public class AutoSceneLoader : MonoBehaviour
{
    public string sceneName = "MainMenu";
    public float delay = 2f;
    private float _timer;

    private void Start()
    {
        _timer = delay;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}

/// <summary>
/// Simple rotation animation for target dummies
/// </summary>
public class RotateAnimation : MonoBehaviour
{
    private void Update()
    {
        transform.Rotate(Vector3.up, 30 * Time.deltaTime);
    }
}

/// <summary>
/// Floating animation for loot items so they're visible
/// </summary>
public class LootFloatAnimation : MonoBehaviour
{
    private Vector3 _startPos;
    private float _offset;

    private void Start()
    {
        _startPos = transform.position;
        _offset = Random.Range(0, 6.28f);
    }

    private void Update()
    {
        float y = Mathf.Sin(Time.time * 1.5f + _offset) * 0.3f;
        transform.position = new Vector3(_startPos.x, _startPos.y + y, _startPos.z);
        transform.Rotate(Vector3.up, 45 * Time.deltaTime);
    }
}

/// <summary>
/// Keeps a quad facing the camera — your AI art always visible!
/// </summary>
public class BillboardToCamera : MonoBehaviour
{
    private void LateUpdate()
    {
        if (Camera.main != null)
            transform.LookAt(Camera.main.transform);
    }
}
