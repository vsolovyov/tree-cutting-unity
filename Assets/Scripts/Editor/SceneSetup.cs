using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.FeedbacksForThirdParty;
#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
using Lofelt.NiceVibrations;
#endif

public class SceneSetup : EditorWindow
{
    [MenuItem("Tools/Setup Tree Cutting Scene")]
    public static void SetupScene()
    {
        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Setup lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1f;

        // Create layers
        CreateLayerIfNeeded("Ground", 6);
        CreateLayerIfNeeded("Tree", 7);

        // Create ground plane
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10, 1, 10);
        ground.layer = 6; // Ground layer

        // Setup player
        var player = CreatePlayer();
        player.transform.position = new Vector3(0, 0.05f, -5);

        // Load tree prefabs
        var treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Fruit_Tree_01_green.prefab");
        var stumpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Fruit_Tree_01_stump.prefab");
        var cutTreePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Fruit_Tree_01_green_cut.prefab");
        var logsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Fruit_Tree_01_logs.prefab");

        if (treePrefab == null)
        {
            Debug.LogError("Could not find tree prefab at expected path!");
            return;
        }

        // Place some trees
        Vector3[] treePositions = new Vector3[]
        {
            new Vector3(0, 0, 5),
            new Vector3(5, 0, 8),
            new Vector3(-4, 0, 6),
            new Vector3(3, 0, 12),
            new Vector3(-6, 0, 10),
        };

        var treesParent = new GameObject("Trees");
        int treeCount = 0;
        foreach (var pos in treePositions)
        {
            var tree = (GameObject)PrefabUtility.InstantiatePrefab(treePrefab, treesParent.transform);
            if (tree == null)
            {
                Debug.LogError($"Failed to instantiate tree at {pos}");
                continue;
            }

            tree.transform.position = pos;
            tree.layer = 7; // Tree layer

            // Set all children to Tree layer too
            foreach (Transform child in tree.GetComponentsInChildren<Transform>())
                child.gameObject.layer = 7;

            // Add CuttableTree component
            var cuttable = tree.AddComponent<CuttableTree>();
            cuttable.stumpPrefab = stumpPrefab;
            cuttable.cutTreePrefab = cutTreePrefab;
            cuttable.logsPrefab = logsPrefab;
            cuttable.maxHealth = 20;
            cuttable.cutHeight = 1.2f;       // Where particles spawn
            // Fall timing synced to falling-tree.wav (~7.8s total)
            cuttable.leanDuration = 2.5f;    // Cracking/creaking phase
            cuttable.fallDuration = 3.0f;    // Actual fall
            cuttable.settleDuration = 1.0f;  // Roll/settle

            // Add TreeHighlight
            tree.AddComponent<TreeHighlight>();

            // Create MMFeedbacks with camera shake, audio, rumble, and particles
            cuttable.hitFeedback = CreateHitFeedback(tree, "HitFeedback",
                duration: 0.08f, amplitude: 0.05f, frequency: 0.05f,
                new[] { "Assets/Feel/NiceVibrations/HapticSamples/Impacts/Wood1.wav",
                        "Assets/Feel/NiceVibrations/HapticSamples/Impacts/Wood2.wav" },
                rumbleLow: 0.3f, rumbleHigh: 0.2f, rumbleDuration: 0.1f,
                addParticles: true, particleCount: 8);
            cuttable.perfectHitFeedback = CreateHitFeedback(tree, "PerfectHitFeedback",
                duration: 0.05f, amplitude: 0.15f, frequency: 0.02f,
                new[] { "Assets/Feel/NiceVibrations/HapticSamples/Impacts/Wood3.wav" },
                rumbleLow: 0.5f, rumbleHigh: 0.4f, rumbleDuration: 0.15f,
                addParticles: true, particleCount: 20);
            cuttable.fallFeedback = CreateHitFeedback(tree, "FallFeedback",
                duration: 6.5f, amplitude: 0.08f, frequency: 0.03f,
                new[] { "Assets/Audio/falling-tree.wav" },
                rumbleLow: 0.7f, rumbleHigh: 0.5f, rumbleDuration: 0.5f);

            treeCount++;
        }
        Debug.Log($"Created {treeCount} trees");

        // Create UI
        CreateUI(player);

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/TreeCuttingPrototype.unity");

        Debug.Log("Scene setup complete! Don't forget to:\n" +
                  "1. Wire up PlayerInput events in Inspector\n" +
                  "2. Set Tree layer mask on TreeCutter\n" +
                  "3. Set Ground layer mask on PlayerMovement");
    }

    static GameObject CreatePlayer()
    {
        // Main player object
        var player = new GameObject("Player");

        // Character Controller
        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.4f;
        cc.center = new Vector3(0, 0.9f, 0);

        // Ground check
        var groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform);
        groundCheck.transform.localPosition = new Vector3(0, 0.1f, 0);

        // Camera
        var cameraHolder = new GameObject("CameraHolder");
        cameraHolder.transform.SetParent(player.transform);
        cameraHolder.transform.localPosition = new Vector3(0, 1.6f, 0);

        // Remove default camera and use ours
        var mainCam = Camera.main;
        if (mainCam != null)
            DestroyImmediate(mainCam.gameObject);

        var cam = cameraHolder.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.nearClipPlane = 0.1f;
        cameraHolder.AddComponent<AudioListener>();

        // Add camera shaker for MMFeedbacks (requires MMWiggle)
        var wiggle = cameraHolder.AddComponent<MMWiggle>();
        wiggle.PositionActive = true;
        wiggle.PositionWiggleProperties = new WiggleProperties {
            WigglePermitted = false,
        };
        wiggle.RotationActive = true;
        wiggle.RotationWiggleProperties = new WiggleProperties {
            WigglePermitted = false,
        };
        wiggle.ScaleActive = true;
        wiggle.ScaleWiggleProperties = new WiggleProperties {
            WigglePermitted = false,
        };
        cameraHolder.AddComponent<MMCameraShaker>();

        // Player Movement
        var movement = player.AddComponent<PlayerMovement>();
        movement.groundCheck = groundCheck.transform;
        movement.groundMask = 1 << 6; // Ground layer

        // First Person Look
        var look = cameraHolder.AddComponent<FirstPersonLook>();
        look.playerBody = player.transform;
        look.mouseSensitivity = 0.15f;

        // Cutting Minigame
        var minigame = player.AddComponent<CuttingMinigame>();

        // Tree Cutter
        var cutter = player.AddComponent<TreeCutter>();
        cutter.minigame = minigame;
        cutter.playerMovement = movement;
        cutter.firstPersonLook = look;
        cutter.detectionRadius = 4f;
        cutter.treeLayer = 1 << 7; // Tree layer

        // Player Input
        var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
            "Assets/Input/GameInputActions.inputactions");

        var playerInput = player.AddComponent<PlayerInput>();
        playerInput.actions = inputAsset;
        playerInput.defaultActionMap = "Gameplay";
        playerInput.notificationBehavior = PlayerNotifications.BroadcastMessages;

        return player;
    }

    static void CreateUI(GameObject player)
    {
        // Canvas
        var canvasGO = new GameObject("UI Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Minigame Panel - vertical bar on right side
        var panel = CreateUIPanel(canvasGO.transform, "MinigamePanel", new Vector2(80, 400));
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0.5f);
        panelRect.anchorMax = new Vector2(1, 0.5f);
        panelRect.pivot = new Vector2(1, 0.5f);
        panelRect.anchoredPosition = new Vector2(-40, 0);

        var panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f);

        // Timing Bar Background - vertical
        var barBG = CreateUIPanel(panel.transform, "TimingBar", new Vector2(40, 360));
        var barBGImage = barBG.AddComponent<Image>();
        barBGImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Bottom Green Zone
        var bottomGreenZone = CreateUIPanel(barBG.transform, "BottomGreenZone", new Vector2(40, 60));
        var bottomGreenImage = bottomGreenZone.AddComponent<Image>();
        bottomGreenImage.color = new Color(0.2f, 0.8f, 0.2f, 0.6f);

        // Bottom Perfect Zone
        var bottomPerfectZone = CreateUIPanel(barBG.transform, "BottomPerfectZone", new Vector2(40, 30));
        var bottomPerfectImage = bottomPerfectZone.AddComponent<Image>();
        bottomPerfectImage.color = new Color(1f, 0.8f, 0f, 0.8f);

        // Top Green Zone
        var topGreenZone = CreateUIPanel(barBG.transform, "TopGreenZone", new Vector2(40, 60));
        var topGreenImage = topGreenZone.AddComponent<Image>();
        topGreenImage.color = new Color(0.2f, 0.8f, 0.2f, 0.6f);

        // Top Perfect Zone
        var topPerfectZone = CreateUIPanel(barBG.transform, "TopPerfectZone", new Vector2(40, 30));
        var topPerfectImage = topPerfectZone.AddComponent<Image>();
        topPerfectImage.color = new Color(1f, 0.8f, 0f, 0.8f);

        // Indicator - horizontal line
        var indicator = CreateUIPanel(barBG.transform, "Indicator", new Vector2(50, 6));
        var indicatorImage = indicator.AddComponent<Image>();
        indicatorImage.color = Color.white;

        // Combo Text - left of bar
        var comboText = CreateTextUI(panel.transform, "ComboText", "x1", 24);
        comboText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-60, 0);
        comboText.gameObject.SetActive(false);

        // Result Text - center screen
        var resultText = CreateTextUI(canvasGO.transform, "ResultText", "PERFECT!", 32);
        resultText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100);
        resultText.gameObject.SetActive(false);

        // Interact Prompt
        var prompt = CreateTextUI(canvasGO.transform, "InteractPrompt", "Press E to cut", 20);
        prompt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -300);
        prompt.gameObject.SetActive(false);

        // Setup MinigameUI component
        var minigameUI = canvasGO.AddComponent<CuttingMinigameUI>();
        minigameUI.minigame = player.GetComponent<CuttingMinigame>();
        minigameUI.minigamePanel = panel;
        minigameUI.timingBar = barBG.GetComponent<RectTransform>();
        minigameUI.bottomGreenZone = bottomGreenZone.GetComponent<RectTransform>();
        minigameUI.bottomPerfectZone = bottomPerfectZone.GetComponent<RectTransform>();
        minigameUI.topGreenZone = topGreenZone.GetComponent<RectTransform>();
        minigameUI.topPerfectZone = topPerfectZone.GetComponent<RectTransform>();
        minigameUI.indicator = indicator.GetComponent<RectTransform>();
        minigameUI.comboText = comboText;
        minigameUI.resultText = resultText;
        minigameUI.interactPrompt = prompt.gameObject;

        // Wire up TreeCutter
        var cutter = player.GetComponent<TreeCutter>();
        cutter.minigameUI = minigameUI;

        // Create miss feedback on player (Body1 = duller thud for miss)
        cutter.missFeedback = CreateHitFeedback(player, "MissFeedback",
            duration: 0.1f, amplitude: 0.08f, frequency: 0.1f,
            new[] { "Assets/Feel/NiceVibrations/HapticSamples/Impacts/Body1.wav" },
            rumbleLow: 0.2f, rumbleHigh: 0.1f, rumbleDuration: 0.08f);

        panel.SetActive(false);
    }

    static GameObject CreateUIPanel(Transform parent, string name, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        return go;
    }

    static TextMeshProUGUI CreateTextUI(Transform parent, string name, string text, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 50);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return tmp;
    }

    static void CreateLayerIfNeeded(string layerName, int layerIndex)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        if (layers.GetArrayElementAtIndex(layerIndex).stringValue != layerName)
        {
            layers.GetArrayElementAtIndex(layerIndex).stringValue = layerName;
            tagManager.ApplyModifiedProperties();
        }
    }

    static MMF_Player CreateHitFeedback(GameObject parent, string name,
        float duration, float amplitude, float frequency,
        string[] audioPaths, float rumbleLow = 0f, float rumbleHigh = 0f, float rumbleDuration = 0.1f,
        bool addParticles = false, int particleCount = 10)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        var player = go.AddComponent<MMF_Player>();

        // Camera shake
        var shake = new MMF_CameraShake();
        shake.Label = "Camera Shake";
        shake.CameraShakeProperties.Duration = duration;
        shake.CameraShakeProperties.Amplitude = amplitude;
        shake.CameraShakeProperties.Frequency = frequency;
        player.AddFeedback(shake);

        // Sound (supports random selection from multiple clips)
        var sound = new MMF_Sound();
        sound.Label = "Sound";
        sound.PlayMethod = MMF_Sound.PlayMethods.OnDemand;
        sound.MinVolume = 0.8f;
        sound.MaxVolume = 1.0f;
        sound.MinPitch = 0.9f;
        sound.MaxPitch = 1.1f;

        if (audioPaths.Length == 1)
        {
            sound.Sfx = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPaths[0]);
        }
        else
        {
            // Multiple clips - use RandomSfx array
            sound.RandomSfx = new AudioClip[audioPaths.Length];
            for (int i = 0; i < audioPaths.Length; i++)
            {
                sound.RandomSfx[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPaths[i]);
            }
        }
        player.AddFeedback(sound);

#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
        // Haptic feedback via NiceVibrations
        if (rumbleLow > 0 || rumbleHigh > 0)
        {
            var haptic = new MMF_NVPreset();
            haptic.Label = "Haptic";
            // Map intensity to preset: low = Light, medium = Medium, high = Heavy
            float maxIntensity = Mathf.Max(rumbleLow, rumbleHigh);
            if (maxIntensity > 0.6f)
                haptic.Preset = HapticPatterns.PresetType.HeavyImpact;
            else if (maxIntensity > 0.3f)
                haptic.Preset = HapticPatterns.PresetType.MediumImpact;
            else
                haptic.Preset = HapticPatterns.PresetType.LightImpact;

            // Initialize HapticSettings (required for CanPlay() check)
            haptic.HapticSettings = new MMFeedbackNVSettings();
            haptic.HapticSettings.OnlyPlayIfHapticsSupported = false; // Allow desktop/gamepad
            player.AddFeedback(haptic);
        }
#endif

        // Wood chip particles
        if (addParticles)
        {
            var particleSystem = CreateWoodChipParticles(go, particleCount);

            var particles = new MMF_Particles();
            particles.Label = "Wood Chips";
            particles.Mode = MMF_Particles.Modes.Emit;
            particles.EmitCount = particleCount;
            particles.BoundParticleSystem = particleSystem;
            particles.MoveToPosition = true;
            player.AddFeedback(particles);
        }

        return player;
    }

    static ParticleSystem CreateWoodChipParticles(GameObject parent, int burstCount)
    {
        var psGO = new GameObject("WoodChips");
        psGO.transform.SetParent(parent.transform, false);

        var ps = psGO.AddComponent<ParticleSystem>();

        // Main module
        var main = ps.main;
        main.duration = 1f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor = new Color(0.6f, 0.4f, 0.2f); // Brown wood color
        main.gravityModifier = 1.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 50;
        main.playOnAwake = false;

        // Emission - burst only
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;

        // Shape - hemisphere burst spraying outward
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.15f;
        shape.rotation = new Vector3(-90f, 0f, 0f); // Point outward horizontally

        // Size over lifetime - shrink
        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0, 1, 1, 0));

        // Color over lifetime - fade out
        var colorOverLife = ps.colorOverLifetime;
        colorOverLife.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLife.color = gradient;

        // Renderer
        var renderer = psGO.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        // Use default particle material
        var mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.color = new Color(0.6f, 0.4f, 0.2f);
        renderer.sharedMaterial = mat;

        // Stop to wait for emit calls
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        return ps;
    }
}
