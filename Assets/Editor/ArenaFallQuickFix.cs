using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;

/// <summary>
/// Arena Fall - Quick Fix & Validation Tool
/// Run from Tools > Arena Fall > Quick Fix
/// Scans the entire project and fixes common issues automatically.
/// </summary>
public class ArenaFallQuickFix : EditorWindow
{
    private Vector2 _scrollPos;
    private string _log = "";
    private bool _hasErrors;

    [MenuItem("Tools/Arena Fall/Quick Fix & Validate")]
    public static void ShowWindow()
    {
        GetWindow<ArenaFallQuickFix>("Arena Fall - Quick Fix");
    }

    private void OnGUI()
    {
        GUILayout.Label("Arena Fall — Quick Fix & Validation", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("🔍 RUN FULL VALIDATION", GUILayout.Height(35)))
        {
            _log = "";
            RunValidation();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Fixes:", EditorStyles.boldLabel);

        if (GUILayout.Button("Fix Missing Script References"))
        {
            FixMissingScripts();
        }

        if (GUILayout.Button("Reimport All Scripts"))
        {
            AssetDatabase.ImportAsset("Assets/Scripts", ImportAssetOptions.ImportRecursive);
            _log += "✓ Reimported all scripts\n";
        }

        if (GUILayout.Button("Create Missing Scene Folders"))
        {
            CreateMissingScenes();
        }

        if (GUILayout.Button("Reset Player Settings"))
        {
            ResetPlayerSettings();
        }

        if (GUILayout.Button("Open Boot Scene & Play"))
        {
            EditorSceneManager.OpenScene("Assets/Scenes/01_Boot/Boot.unity");
            EditorApplication.isPlaying = true;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Validation Log:", EditorStyles.boldLabel);
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        GUILayout.TextArea(_log, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void RunValidation()
    {
        _hasErrors = false;
        _log = "=== Arena Fall Validation ===\n\n";

        // 1. Check scripts
        int scriptCount = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/Scripts" }).Length;
        _log += $"[Scripts] Found {scriptCount} C# scripts (expected 42)\n";
        if (scriptCount < 40) { _hasErrors = true; _log += "  ⚠ Some scripts may be missing\n"; }

        // 2. Check scenes
        int sceneCount = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" }).Length;
        _log += $"[Scenes] Found {sceneCount} scenes (expected 14)\n";
        if (sceneCount < 14) { _hasErrors = true; _log += "  ⚠ Some scenes missing\n"; }

        // 3. Check ScriptableObjects
        int weaponData = AssetDatabase.FindAssets("t:ArenaFall.Data.WeaponData").Length;
        int itemData = AssetDatabase.FindAssets("t:ArenaFall.Data.ItemData").Length;
        _log += $"[Data] {weaponData} WeaponData, {itemData} ItemData assets\n";

        // 4. Check Build Settings
        var scenes = EditorBuildSettings.scenes;
        _log += $"[Build] {scenes.Length} scenes in build settings\n";

        // 5. Check Tags & Layers
        var tagManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
        var serializedTag = new SerializedObject(tagManager);
        var tagsProp = serializedTag.FindProperty("tags");
        int tagCount = 0;
        for (int i = 0; i < tagsProp.arraySize; i++)
            if (!string.IsNullOrEmpty(tagsProp.GetArrayElementAtIndex(i).stringValue))
                tagCount++;
        _log += $"[Layers] {tagCount} custom tags defined\n";

        // 6. Check URP
        var urpAssets = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
        _log += $"[URP] {urpAssets.Length} URP assets found\n";

        // 7. Check Audio Mixer
        var mixers = AssetDatabase.FindAssets("t:AudioMixer");
        _log += $"[Audio] {mixers.Length} Audio Mixers found\n";

        // 8. Check Input Actions
        var inputAssets = AssetDatabase.FindAssets("t:InputActionAsset");
        _log += $"[Input] {inputAssets.Length} Input Action Assets found\n";

        // 9. Check packages
        _log += $"\n=== Package Check ===\n";
        CheckPackage("com.unity.render-pipelines.universal", "URP");
        CheckPackage("com.unity.inputsystem", "Input System");
        CheckPackage("com.unity.addressables", "Addressables");
        CheckPackage("com.unity.textmeshpro", "TextMeshPro");
        CheckPackage("com.unity.cinemachine", "Cinemachine");
        CheckPackage("com.unity.netcode.gameobjects", "Netcode");
        CheckPackage("com.unity.ai.navigation", "AI Navigation");

        _log += $"\n=== Result ===\n";
        _log += _hasErrors ? "⚠ Some issues detected. Use Quick Fix buttons above.\n" : "✅ All checks passed! Ready to build.\n";
        
        Repaint();
    }

    private void CheckPackage(string packageId, string displayName)
    {
        var package = UnityEditor.PackageManager.Requests.ListRequest(false);
        // Simple check: look for the DLL
        bool found = System.AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.GetName().Name.ToLower().Contains(packageId.Split('.').Last()));
        _log += found ? $"  ✓ {displayName} installed\n" : $"  ⚠ {displayName} NOT found. Install via Window > Package Manager\n";
    }

    private void FixMissingScripts()
    {
        int fixedCount = 0;
        var allPrefabs = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in allPrefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (obj == null) continue;

            var components = obj.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var comp in components)
            {
                if (comp == null)
                {
                    // Remove missing component
                    var serialized = new SerializedObject(obj);
                    var componentProp = serialized.FindProperty("m_Component");
                    // Mark for cleanup
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                    fixedCount++;
                }
            }
        }
        _log += $"✓ Fixed {fixedCount} missing script references\n";
        AssetDatabase.SaveAssets();
        Repaint();
    }

    private void CreateMissingScenes()
    {
        string[] sceneNames = { 
            "01_Boot/Boot", "02_Login/Login", "03_MainMenu/MainMenu", "04_Profile/Profile",
            "05_Lobby/Lobby", "06_Customization/Customization", "07_Loadout/Loadout",
            "08_TrainingGround/TrainingGround", "09_Matchmaking/Matchmaking", 
            "10_GameMap/GameMap", "11_ResultScreen/ResultScreen", "12_Replay/Replay",
            "13_Settings/Settings", "14_TestScene/TestScene" 
        };

        int created = 0;
        foreach (var scene in sceneNames)
        {
            string path = $"Assets/Scenes/{scene}.unity";
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var sceneInstance = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
                EditorSceneManager.SaveScene(sceneInstance, path);
                EditorSceneManager.CloseScene(sceneInstance, true);
                created++;
            }
        }
        _log += $"✓ Created {created} missing scene files\n";
        Repaint();
    }

    private void ResetPlayerSettings()
    {
        PlayerSettings.productName = "Arena Fall";
        PlayerSettings.companyName = "Arena Games";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, "com.arenagames.arenafall");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.arenagames.arenafall");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.arenagames.arenafall");
        
        _log += "✓ Player settings configured\n";
        Repaint();
    }
}
