using System;
using System.Collections.Generic;
using System.IO;
using GunJump.Core;
using GunJump.Gameplay;
using GunJump.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GunJump.EditorTools
{
    public static class GameSceneBuilder
    {
        public const string SCENES_FOLDER = ROOT + "/Scenes";
        public const string BOOT_SCENE_PATH = SCENES_FOLDER + "/Boot.unity";
        public const string LOADING_SCENE_PATH = SCENES_FOLDER + "/Loading.unity";

        const string ROOT = "Assets/_Game";
        const string LEVEL1_SCENE_PATH = SCENES_FOLDER + "/Level1.unity";
        const string PROTOTYPE_SCENE_PATH = SCENES_FOLDER + "/Prototype.unity";
        const string LEVEL_CORE_PREFAB_PATH = ROOT + "/Prefabs/Level/LevelCore.prefab";
        const string BULLET_PREFAB_PATH = ROOT + "/Prefabs/Bullets/Bullet.prefab";

        const string GUN_TUNING_PATH = ROOT + "/Data/Tuning/GunTuning.asset";
        const string CAMERA_TUNING_PATH = ROOT + "/Data/Tuning/CameraTuning.asset";
        const string LEVEL_RULES_PATH = ROOT + "/Data/Tuning/LevelRules.asset";
        const string ECONOMY_CONFIG_PATH = ROOT + "/Data/Tuning/EconomyConfig.asset";
        const string LEVEL_CATALOG_PATH = ROOT + "/Data/Levels/LevelCatalog.asset";
        const string BULLET_CONFIG_PATH = ROOT + "/Data/Weapons/PistolBullet.asset";
        const string SINGLE_SHOT_PATH = ROOT + "/Data/Weapons/SingleShot.asset";
        const string ENEMY_CONFIG_PATH = ROOT + "/Data/Enemies/StaticEnemy.asset";

        const string GROUND_MATERIAL = "Proto_Ground";
        const string GUN_MATERIAL = "Proto_Gun";
        const string ENEMY_MATERIAL = "Proto_Enemy";
        const string BULLET_MATERIAL = "Proto_Bullet";
        const string COIN_MATERIAL = "Proto_Coin";
        const string FINISH_MATERIAL = "Proto_Finish";
        const string TARGET_MATERIAL_A = "Proto_TargetA";
        const string TARGET_MATERIAL_B = "Proto_TargetB";

        const float DEPTH = 8f;
        const int MULTIPLIER_STEPS = 5;

        static readonly Color HUD_TEXT_COLOR = new Color(0.1f, 0.1f, 0.15f);
        static readonly Color SKY_COLOR = new Color(0.62f, 0.8f, 0.92f);
        static readonly Color LOADING_COLOR = new Color(0.1f, 0.12f, 0.18f);

        const int LOADING_SORTING_ORDER = 100;

        [MenuItem("GunJump/Build/All (first time)")]
        public static void BuildAll()
        {
            if (!CanBuild(LEVEL_CORE_PREFAB_PATH, BOOT_SCENE_PATH, LOADING_SCENE_PATH, LEVEL1_SCENE_PATH))
                return;

            EnsureData();
            BuildLevelCorePrefab();
            BuildBootScene();
            BuildLoadingScene();
            BuildLevel1Scene();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("GunJump: prefab LevelCore y escenas Boot, Loading y Level1 creados.");
        }

        [MenuItem("GunJump/Build/LevelCore Prefab")]
        public static void BuildLevelCoreOnly()
        {
            if (!CanBuild(LEVEL_CORE_PREFAB_PATH))
                return;

            string previous = SceneManager.GetActiveScene().path;
            EnsureData();
            BuildLevelCorePrefab();
            AssetDatabase.SaveAssets();
            Reopen(previous);
        }

        [MenuItem("GunJump/Build/Boot and Loading Scenes")]
        public static void BuildBootAndLoadingOnly()
        {
            if (!CanBuild(BOOT_SCENE_PATH, LOADING_SCENE_PATH))
                return;

            string previous = SceneManager.GetActiveScene().path;
            EnsureData();
            BuildBootScene();
            BuildLoadingScene();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            Reopen(previous);
        }

        [MenuItem("GunJump/Build/Loading Scene")]
        public static void BuildLoadingOnly()
        {
            if (!CanBuild(LOADING_SCENE_PATH))
                return;

            string previous = SceneManager.GetActiveScene().path;
            BuildLoadingScene();
            AppendToBuildSettings(LOADING_SCENE_PATH);
            AssetDatabase.SaveAssets();
            Reopen(previous);
            Debug.Log($"GunJump: escena {LOADING_SCENE_PATH} creada y agregada al final de Build Settings.");
        }

        [MenuItem("GunJump/Build/Level 1 Scene")]
        public static void BuildLevel1Only()
        {
            if (!CanBuild(LEVEL1_SCENE_PATH))
                return;

            if (!File.Exists(LEVEL_CORE_PREFAB_PATH))
            {
                EditorUtility.DisplayDialog("GunJump", "Primero hay que crear el prefab LevelCore (GunJump > Build > LevelCore Prefab).", "Entendido");
                return;
            }

            EnsureData();
            BuildLevel1Scene();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
        }

        static bool CanBuild(params string[] outputs)
        {
            if (!EnsureTextMeshPro())
                return false;

            List<string> existing = new List<string>();
            foreach (string output in outputs)
            {
                if (File.Exists(output))
                    existing.Add(Path.GetFileName(output));
            }

            if (existing.Count > 0 && !EditorUtility.DisplayDialog(
                    "GunJump",
                    "Ya existen y se van a reemplazar: " + string.Join(", ", existing) + ". Se pierde lo que se les haya editado a mano.",
                    "Reemplazar",
                    "Cancelar"))
                return false;

            return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        static bool EnsureTextMeshPro()
        {
            if (AssetDatabase.FindAssets("t:TMP_Settings").Length > 0)
                return true;

            bool started = EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
            EditorUtility.DisplayDialog(
                "GunJump",
                started
                    ? "Faltaban los recursos esenciales de TextMesh Pro y se mandaron a importar. Cuando termine, volvé a correr este menú."
                    : "Faltan los recursos esenciales de TextMesh Pro. Importalos desde Window > TextMeshPro > Import TMP Essential Resources y volvé a correr este menú.",
                "Entendido");
            return false;
        }

        static void Reopen(string scenePath)
        {
            if (!string.IsNullOrEmpty(scenePath) && File.Exists(scenePath))
                EditorSceneManager.OpenScene(scenePath);
        }

        static void EnsureData()
        {
            LoadOrCreate<GunTuning>(GUN_TUNING_PATH);
            LoadOrCreate<CameraTuning>(CAMERA_TUNING_PATH);
            LoadOrCreate<LevelRules>(LEVEL_RULES_PATH);
            LoadOrCreate<EconomyConfig>(ECONOMY_CONFIG_PATH);
            LoadOrCreate<SingleShot>(SINGLE_SHOT_PATH);
            LoadOrCreate<EnemyConfig>(ENEMY_CONFIG_PATH);
            BulletConfig bulletConfig = LoadOrCreate<BulletConfig>(BULLET_CONFIG_PATH);

            LevelCatalog catalog = LoadOrCreate<LevelCatalog>(LEVEL_CATALOG_PATH);
            if (catalog.Count == 0)
            {
                SerializedObject serialized = new SerializedObject(catalog);
                SerializedProperty names = serialized.FindProperty("sceneNames");
                names.arraySize = 1;
                names.GetArrayElementAtIndex(0).stringValue = Path.GetFileNameWithoutExtension(LEVEL1_SCENE_PATH);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            EnsureMaterial(GROUND_MATERIAL, new Color(0.55f, 0.6f, 0.65f));
            EnsureMaterial(GUN_MATERIAL, new Color(0.15f, 0.15f, 0.18f));
            EnsureMaterial(ENEMY_MATERIAL, new Color(0.85f, 0.25f, 0.2f));
            EnsureMaterial(BULLET_MATERIAL, new Color(1f, 0.85f, 0.2f));
            EnsureMaterial(COIN_MATERIAL, new Color(1f, 0.78f, 0.1f));
            EnsureMaterial(FINISH_MATERIAL, new Color(0.25f, 0.75f, 0.35f));
            EnsureMaterial(TARGET_MATERIAL_A, new Color(0.95f, 0.95f, 0.95f));
            EnsureMaterial(TARGET_MATERIAL_B, new Color(0.9f, 0.35f, 0.3f));

            if (!File.Exists(BULLET_PREFAB_PATH))
                CreateBulletPrefab(bulletConfig);

            AssetDatabase.SaveAssets();
        }

        static void CreateBulletPrefab(BulletConfig config)
        {
            GameObject bulletObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bulletObject.name = "Bullet";
            Object.DestroyImmediate(bulletObject.GetComponent<Collider>());
            bulletObject.transform.localScale = new Vector3(0.35f, 0.1f, 0.1f);
            bulletObject.GetComponent<MeshRenderer>().sharedMaterial = LoadMaterial(BULLET_MATERIAL);

            Bullet bullet = bulletObject.AddComponent<Bullet>();
            SetReference(bullet, "config", config);

            EnsureFolder(FolderOf(BULLET_PREFAB_PATH));
            PrefabUtility.SaveAsPrefabAsset(bulletObject, BULLET_PREFAB_PATH);
            Object.DestroyImmediate(bulletObject);
        }

        static void BuildLevelCorePrefab()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject root = new GameObject("LevelCore");
            LevelSettings settings = root.AddComponent<LevelSettings>();
            LevelCore core = root.AddComponent<LevelCore>();

            BulletSpawner spawner = new GameObject("BulletSpawner").AddComponent<BulletSpawner>();
            spawner.transform.SetParent(root.transform, false);
            SetReference(spawner, "prefab", Load<GameObject>(BULLET_PREFAB_PATH).GetComponent<Bullet>());

            Gun gun = CreateGun(root.transform, spawner);
            CreateGameplayCamera(root.transform, gun.transform);

            InputReader input = new GameObject("Input").AddComponent<InputReader>();
            input.transform.SetParent(root.transform, false);

            BuildLevelUi(root.transform, out HudView hud, out PauseView pauseView, out ResultView resultView);

            SetReference(core, "settings", settings);
            SetReference(core, "rules", Load<LevelRules>(LEVEL_RULES_PATH));
            SetReference(core, "gun", gun);
            SetReference(core, "input", input);
            SetReference(core, "hud", hud);
            SetReference(core, "pauseView", pauseView);
            SetReference(core, "resultView", resultView);

            EnsureFolder(FolderOf(LEVEL_CORE_PREFAB_PATH));
            PrefabUtility.SaveAsPrefabAsset(root, LEVEL_CORE_PREFAB_PATH);
            Object.DestroyImmediate(root);
        }

        static Gun CreateGun(Transform parent, BulletSpawner spawner)
        {
            GameObject gunObject = new GameObject("Gun");
            gunObject.transform.SetParent(parent, false);

            Rigidbody body = gunObject.AddComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;

            BoxCollider collider = gunObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 0.35f, 0.25f);

            Material material = LoadMaterial(GUN_MATERIAL);
            CreateGunPart("Barrel", gunObject.transform, Vector3.zero, new Vector3(1f, 0.35f, 0.25f), material);
            CreateGunPart("Grip", gunObject.transform, new Vector3(-0.3f, -0.35f, 0f), new Vector3(0.25f, 0.45f, 0.2f), material);

            GameObject muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(gunObject.transform, false);
            muzzle.transform.localPosition = new Vector3(0.6f, 0f, 0f);

            Gun gun = gunObject.AddComponent<Gun>();
            SetReference(gun, "tuning", Load<GunTuning>(GUN_TUNING_PATH));
            SetReference(gun, "muzzle", muzzle.transform);
            SetReference(gun, "fireStrategy", Load<SingleShot>(SINGLE_SHOT_PATH));
            SetReference(gun, "bulletSpawner", spawner);
            return gun;
        }

        static void CreateGunPart(string name, Transform parent, Vector3 localPosition, Vector3 size, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = size;
            part.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        static void CreateGameplayCamera(Transform parent, Transform target)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(parent, false);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = SKY_COLOR;

            FollowCamera follow = cameraObject.AddComponent<FollowCamera>();
            SetReference(follow, "tuning", Load<CameraTuning>(CAMERA_TUNING_PATH));
            SetReference(follow, "target", target);
        }

        static void BuildLevelUi(Transform parent, out HudView hud, out PauseView pauseView, out ResultView resultView)
        {
            GameObject canvas = UiFactory.CreateCanvas("UI", parent);
            hud = canvas.AddComponent<HudView>();
            pauseView = canvas.AddComponent<PauseView>();
            resultView = canvas.AddComponent<ResultView>();

            BuildHud(canvas.transform, hud);
            BuildPausePanel(canvas.transform, pauseView);
            BuildResultPanel(canvas.transform, resultView);
        }

        static void BuildHud(Transform canvas, HudView hud)
        {
            RectTransform safeArea = UiFactory.CreateSafeArea(canvas);
            Vector2 labelSize = new Vector2(700f, 70f);

            TMP_Text ammo = UiFactory.CreateLabel("Ammo", safeArea, "Balas 0", 48f, TextAlignmentOptions.TopLeft, HUD_TEXT_COLOR);
            UiFactory.Place(ammo.rectTransform, UiFactory.TOP_LEFT, new Vector2(40f, -40f), labelSize);

            TMP_Text coins = UiFactory.CreateLabel("Coins", safeArea, "Monedas 0", 48f, TextAlignmentOptions.TopLeft, HUD_TEXT_COLOR);
            UiFactory.Place(coins.rectTransform, UiFactory.TOP_LEFT, new Vector2(40f, -110f), labelSize);

            TMP_Text enemies = UiFactory.CreateLabel("Enemies", safeArea, "Enemigos 0", 48f, TextAlignmentOptions.TopLeft, HUD_TEXT_COLOR);
            UiFactory.Place(enemies.rectTransform, UiFactory.TOP_LEFT, new Vector2(40f, -180f), labelSize);

            CommandButton pauseButton = UiFactory.CreateButton("PauseButton", safeArea, "II", 56f);
            UiFactory.Place((RectTransform)pauseButton.transform, UiFactory.TOP_RIGHT, new Vector2(-40f, -40f), new Vector2(130f, 130f));

            SetReference(hud, "ammoLabel", ammo);
            SetReference(hud, "coinsLabel", coins);
            SetReference(hud, "enemiesLabel", enemies);
            SetReference(hud, "pauseButton", pauseButton);
        }

        static void BuildPausePanel(Transform canvas, PauseView pauseView)
        {
            GameObject panel = UiFactory.CreateOverlay("PausePanel", canvas);
            Vector2 buttonSize = new Vector2(600f, 150f);

            TMP_Text title = UiFactory.CreateLabel("Title", panel.transform, "Pausa", 96f, TextAlignmentOptions.Center, Color.white);
            UiFactory.Place(title.rectTransform, UiFactory.CENTER, new Vector2(0f, 260f), new Vector2(900f, 160f));

            CommandButton resume = UiFactory.CreateButton("ResumeButton", panel.transform, "Seguir", 52f);
            UiFactory.Place((RectTransform)resume.transform, UiFactory.CENTER, new Vector2(0f, 40f), buttonSize);

            CommandButton retry = UiFactory.CreateButton("RetryButton", panel.transform, "Reintentar", 52f);
            UiFactory.Place((RectTransform)retry.transform, UiFactory.CENTER, new Vector2(0f, -150f), buttonSize);

            SetReference(pauseView, "panel", panel);
            SetReference(pauseView, "resumeButton", resume);
            SetReference(pauseView, "retryButton", retry);
            panel.SetActive(false);
        }

        static void BuildResultPanel(Transform canvas, ResultView resultView)
        {
            GameObject panel = UiFactory.CreateOverlay("ResultPanel", canvas);
            Vector2 buttonSize = new Vector2(600f, 150f);

            TMP_Text title = UiFactory.CreateLabel("Title", panel.transform, "¡Nivel completado!", 80f, TextAlignmentOptions.Center, Color.white);
            UiFactory.Place(title.rectTransform, UiFactory.CENTER, new Vector2(0f, 460f), new Vector2(1000f, 160f));

            RectTransform starsRow = UiFactory.CreateRect("Stars", panel.transform);
            UiFactory.Place(starsRow, UiFactory.CENTER, new Vector2(0f, 270f), new Vector2(500f, 140f));
            Object[] stars = new Object[3];
            for (int i = 0; i < stars.Length; i++)
            {
                Image star = UiFactory.CreateImage("Star" + (i + 1), starsRow, Color.white, false);
                UiFactory.Place(star.rectTransform, UiFactory.CENTER, new Vector2((i - 1) * 170f, 0f), new Vector2(100f, 100f));
                star.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                stars[i] = star;
            }

            TMP_Text coins = UiFactory.CreateLabel("Coins", panel.transform, "+0 monedas", 64f, TextAlignmentOptions.Center, Color.white);
            UiFactory.Place(coins.rectTransform, UiFactory.CENTER, new Vector2(0f, 80f), new Vector2(900f, 100f));

            TMP_Text multiplier = UiFactory.CreateLabel("Multiplier", panel.transform, "Multiplicador x1", 44f, TextAlignmentOptions.Center, Color.white);
            UiFactory.Place(multiplier.rectTransform, UiFactory.CENTER, new Vector2(0f, -10f), new Vector2(900f, 80f));

            CommandButton next = UiFactory.CreateButton("NextButton", panel.transform, "Siguiente nivel", 52f);
            UiFactory.Place((RectTransform)next.transform, UiFactory.CENTER, new Vector2(0f, -220f), buttonSize);

            CommandButton retry = UiFactory.CreateButton("RetryButton", panel.transform, "Reintentar", 52f);
            UiFactory.Place((RectTransform)retry.transform, UiFactory.CENTER, new Vector2(0f, -410f), buttonSize);

            SetReference(resultView, "panel", panel);
            SetReference(resultView, "titleLabel", title);
            SetReference(resultView, "coinsLabel", coins);
            SetReference(resultView, "multiplierLabel", multiplier);
            SetReference(resultView, "starsRow", starsRow.gameObject);
            SetReferences(resultView, "stars", stars);
            SetReference(resultView, "nextButton", next);
            SetReference(resultView, "retryButton", retry);
            panel.SetActive(false);
        }

        static void BuildBootScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameBootstrap bootstrap = new GameObject("Bootstrap").AddComponent<GameBootstrap>();
            SetReference(bootstrap, "economyConfig", Load<EconomyConfig>(ECONOMY_CONFIG_PATH));
            SetReference(bootstrap, "levels", Load<LevelCatalog>(LEVEL_CATALOG_PATH));

            UiFactory.CreateEventSystem();

            new GameObject("Audio", typeof(AudioListener));

            SaveScene(scene, BOOT_SCENE_PATH);
        }

        static void BuildLoadingScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera camera = new GameObject("Loading Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = LOADING_COLOR;
            camera.cullingMask = 0;

            GameObject canvas = UiFactory.CreateCanvas("UI", null);
            canvas.GetComponent<Canvas>().sortingOrder = LOADING_SORTING_ORDER;
            LoadingView view = canvas.AddComponent<LoadingView>();

            Image background = UiFactory.CreateImage("Background", canvas.transform, LOADING_COLOR, true);
            UiFactory.Stretch(background.rectTransform);

            TMP_Text label = UiFactory.CreateLabel("Label", canvas.transform, "Cargando", 64f, TextAlignmentOptions.Center, Color.white);
            UiFactory.Place(label.rectTransform, UiFactory.CENTER, new Vector2(0f, 60f), new Vector2(900f, 120f));

            Image bar = UiFactory.CreateImage("Bar", canvas.transform, new Color(1f, 1f, 1f, 0.2f), false);
            UiFactory.Place(bar.rectTransform, UiFactory.CENTER, new Vector2(0f, -60f), new Vector2(700f, 24f));

            Image fill = UiFactory.CreateImage("Fill", bar.transform, Color.white, false);
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = new Vector2(0f, 1f);
            fill.rectTransform.offsetMin = Vector2.zero;
            fill.rectTransform.offsetMax = Vector2.zero;
            SetReference(view, "fill", fill.rectTransform);

            SaveScene(scene, LOADING_SCENE_PATH);
        }

        static void BuildLevel1Scene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Light light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject core = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(LEVEL_CORE_PREFAB_PATH), scene);
            core.transform.position = new Vector3(0f, 3f, 0f);

            Transform geometry = new GameObject("Geometry").transform;
            Material ground = LoadMaterial(GROUND_MATERIAL);
            CreateBlock("Ground_A", geometry, new Vector3(5f, -1f, 0f), new Vector3(22f, 2f, DEPTH), ground);
            CreateBlock("Ground_B", geometry, new Vector3(30f, -1f, 0f), new Vector3(22f, 2f, DEPTH), ground);
            CreateBlock("Step", geometry, new Vector3(33f, 0.75f, 0f), new Vector3(6f, 1.5f, DEPTH), ground);
            CreateBlock("Ground_C", geometry, new Vector3(54.75f, -1f, 0f), new Vector3(20.5f, 2f, DEPTH), ground);

            Transform enemies = new GameObject("Enemies").transform;
            CreateEnemy("Enemy_1", enemies, new Vector3(9f, 1f, 0f));
            CreateEnemy("Enemy_2", enemies, new Vector3(24f, 1f, 0f));
            CreateEnemy("Enemy_3", enemies, new Vector3(33f, 2.5f, 0f));
            CreateEnemy("Enemy_4", enemies, new Vector3(50f, 1f, 0f));

            Transform coins = new GameObject("Coins").transform;
            float[] coinX = { 4f, 13f, 16.5f, 17.5f, 18.5f, 27f, 42f, 42.75f, 43.5f };
            float[] coinY = { 2f, 2.5f, 2.5f, 3f, 2.5f, 2.5f, 2.5f, 3f, 2.5f };
            for (int i = 0; i < coinX.Length; i++)
                CreateCoin("Coin_" + (i + 1), coins, new Vector3(coinX[i], coinY[i], 0f));

            CreateFinish(new Vector3(58.5f, 0f, 0f));
            CreateMultiplierWall(new Vector3(63.5f, 0f, 0f));

            SaveScene(scene, LEVEL1_SCENE_PATH);
        }

        static void CreateBlock(string name, Transform parent, Vector3 center, Vector3 size, Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.position = center;
            block.transform.localScale = size;
            block.GetComponent<MeshRenderer>().sharedMaterial = material;
            GameObjectUtility.SetStaticEditorFlags(block, StaticEditorFlags.BatchingStatic);
        }

        static void CreateEnemy(string name, Transform parent, Vector3 position)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = name;
            enemy.transform.SetParent(parent, false);
            enemy.transform.position = position;
            enemy.GetComponent<MeshRenderer>().sharedMaterial = LoadMaterial(ENEMY_MATERIAL);

            Rigidbody body = enemy.AddComponent<Rigidbody>();
            body.isKinematic = true;

            StaticEnemy staticEnemy = enemy.AddComponent<StaticEnemy>();
            SetReference(staticEnemy, "config", Load<EnemyConfig>(ENEMY_CONFIG_PATH));
            SetReference(staticEnemy, "body", body);
        }

        static void CreateCoin(string name, Transform parent, Vector3 position)
        {
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            coin.name = name;
            coin.transform.SetParent(parent, false);
            coin.transform.position = position;
            coin.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
            coin.transform.localScale = new Vector3(0.45f, 0.45f, 0.15f);
            coin.GetComponent<MeshRenderer>().sharedMaterial = LoadMaterial(COIN_MATERIAL);

            BoxCollider trigger = coin.GetComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(3f, 3f, DEPTH / 0.15f);
            coin.AddComponent<CoinPickup>();
        }

        static void CreateFinish(Vector3 groundPosition)
        {
            GameObject finish = new GameObject("Finish");
            finish.transform.position = groundPosition;

            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = "Pad";
            Object.DestroyImmediate(pad.GetComponent<Collider>());
            pad.transform.SetParent(finish.transform, false);
            pad.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            pad.transform.localScale = new Vector3(5f, 0.06f, DEPTH);
            pad.GetComponent<MeshRenderer>().sharedMaterial = LoadMaterial(FINISH_MATERIAL);

            GameObject zone = new GameObject("Zone");
            zone.transform.SetParent(finish.transform, false);
            zone.transform.localPosition = new Vector3(0f, 3f, 0f);
            BoxCollider trigger = zone.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(5f, 6f, DEPTH);
            zone.AddComponent<FinishZone>();
        }

        static void CreateMultiplierWall(Vector3 groundPosition)
        {
            const float BLOCK_HEIGHT = 1.5f;
            const float SPACING = 1.55f;
            const float THICKNESS = 1f;

            GameObject wall = new GameObject("MultiplierWall");
            wall.transform.position = groundPosition;

            for (int i = 0; i < MULTIPLIER_STEPS; i++)
            {
                int value = i + 1;
                GameObject target = new GameObject("Target_x" + value);
                target.transform.SetParent(wall.transform, false);
                target.transform.localPosition = new Vector3(0f, BLOCK_HEIGHT * 0.5f + i * SPACING, 0f);

                BoxCollider collider = target.AddComponent<BoxCollider>();
                collider.size = new Vector3(THICKNESS, BLOCK_HEIGHT, DEPTH);

                GameObject visual = new GameObject("Visual");
                visual.transform.SetParent(target.transform, false);

                GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.name = "Block";
                Object.DestroyImmediate(block.GetComponent<Collider>());
                block.transform.SetParent(visual.transform, false);
                block.transform.localScale = new Vector3(THICKNESS, BLOCK_HEIGHT, DEPTH);
                block.GetComponent<MeshRenderer>().sharedMaterial = LoadMaterial(i % 2 == 0 ? TARGET_MATERIAL_A : TARGET_MATERIAL_B);

                TextMeshPro label = new GameObject("Label").AddComponent<TextMeshPro>();
                label.transform.SetParent(visual.transform, false);
                label.transform.localPosition = new Vector3(-(THICKNESS * 0.5f + 0.02f), 0f, 0f);
                label.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                label.rectTransform.sizeDelta = new Vector2(DEPTH, BLOCK_HEIGHT);
                label.text = "x" + value;
                label.fontSize = 10f;
                label.alignment = TextAlignmentOptions.Center;
                label.color = HUD_TEXT_COLOR;

                MultiplierTarget multiplierTarget = target.AddComponent<MultiplierTarget>();
                SetReference(multiplierTarget, "visual", visual);
                SerializedObject serialized = new SerializedObject(multiplierTarget);
                serialized.FindProperty("multiplier").intValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void SaveScene(Scene scene, string path)
        {
            EnsureFolder(FolderOf(path));
            EditorSceneManager.SaveScene(scene, path);
        }

        static void UpdateBuildSettings()
        {
            string[] ordered = { BOOT_SCENE_PATH, LOADING_SCENE_PATH, LEVEL1_SCENE_PATH };
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            foreach (string path in ordered)
            {
                if (File.Exists(path))
                    scenes.Add(new EditorBuildSettingsScene(path, true));
            }

            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                if (existing.path == PROTOTYPE_SCENE_PATH || Array.IndexOf(ordered, existing.path) >= 0)
                    continue;

                scenes.Add(existing);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static void AppendToBuildSettings(string path)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int index = scenes.FindIndex(scene => scene.path == path);

            if (index >= 0)
                scenes[index] = new EditorBuildSettingsScene(path, true);
            else
                scenes.Add(new EditorBuildSettingsScene(path, true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static AssetType Load<AssetType>(string path) where AssetType : Object
        {
            AssetType asset = AssetDatabase.LoadAssetAtPath<AssetType>(path);
            if (asset == null)
                throw new InvalidOperationException("No se pudo cargar " + path);

            return asset;
        }

        static AssetType LoadOrCreate<AssetType>(string path) where AssetType : ScriptableObject
        {
            AssetType asset = AssetDatabase.LoadAssetAtPath<AssetType>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<AssetType>();
            EnsureFolder(FolderOf(path));
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static string MaterialPath(string name)
        {
            return ROOT + "/Art/Materials/" + name + ".mat";
        }

        static Material LoadMaterial(string name)
        {
            return Load<Material>(MaterialPath(name));
        }

        static void EnsureMaterial(string name, Color color)
        {
            string path = MaterialPath(name);
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
                return;

            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
            Material material = pipeline != null && pipeline.defaultMaterial != null
                ? new Material(pipeline.defaultMaterial)
                : new Material(Shader.Find("Standard"));
            material.color = color;

            EnsureFolder(FolderOf(path));
            AssetDatabase.CreateAsset(material, path);
        }

        static string FolderOf(string assetPath)
        {
            return Path.GetDirectoryName(assetPath).Replace('\\', '/');
        }

        static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            string parent = FolderOf(folder);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        internal static void SetReference(Object target, string field, Object value)
        {
            if (value == null)
                throw new InvalidOperationException($"No hay nada que asignar a '{field}' de {target.GetType().Name}.");

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
                throw new InvalidOperationException($"{target.GetType().Name} no tiene el campo serializado '{field}'.");

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetReferences(Object target, string field, Object[] values)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null || !property.isArray)
                throw new InvalidOperationException($"{target.GetType().Name} no tiene la lista serializada '{field}'.");

            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
