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
    // Arma con primitivas el prefab LevelCore y las escenas Boot, Loading y Level1, para no cablearlas a mano.
    // Los assets de datos que ya existen se respetan. El prefab y las escenas sí se reemplazan, previa confirmación:
    // una vez que se editen a mano, conviene no volver a correr estos menús sobre ellos.
    public static class GameSceneBuilder
    {
        public const string ScenesFolder = Root + "/Scenes";
        public const string BootScenePath = ScenesFolder + "/Boot.unity";
        public const string LoadingScenePath = ScenesFolder + "/Loading.unity";

        const string Root = "Assets/_Game";
        const string Level1ScenePath = ScenesFolder + "/Level1.unity";
        const string PrototypeScenePath = ScenesFolder + "/Prototype.unity";
        const string LevelCorePrefabPath = Root + "/Prefabs/Level/LevelCore.prefab";
        const string BulletPrefabPath = Root + "/Prefabs/Bullets/Bullet.prefab";

        const string GunTuningPath = Root + "/Data/Tuning/GunTuning.asset";
        const string CameraTuningPath = Root + "/Data/Tuning/CameraTuning.asset";
        const string LevelRulesPath = Root + "/Data/Tuning/LevelRules.asset";
        const string EconomyConfigPath = Root + "/Data/Tuning/EconomyConfig.asset";
        const string LevelCatalogPath = Root + "/Data/Levels/LevelCatalog.asset";
        const string BulletConfigPath = Root + "/Data/Weapons/PistolBullet.asset";
        const string SingleShotPath = Root + "/Data/Weapons/SingleShot.asset";
        const string EnemyConfigPath = Root + "/Data/Enemies/StaticEnemy.asset";

        const string GroundMaterial = "Proto_Ground";
        const string GunMaterial = "Proto_Gun";
        const string EnemyMaterial = "Proto_Enemy";
        const string BulletMaterial = "Proto_Bullet";
        const string CoinMaterial = "Proto_Coin";
        const string FinishMaterial = "Proto_Finish";
        const string TargetMaterialA = "Proto_TargetA";
        const string TargetMaterialB = "Proto_TargetB";

        const float Depth = 8f;
        const int MultiplierSteps = 5;

        static readonly Color HudTextColor = new Color(0.1f, 0.1f, 0.15f);
        static readonly Color SkyColor = new Color(0.62f, 0.8f, 0.92f);
        static readonly Color LoadingColor = new Color(0.1f, 0.12f, 0.18f);

        [MenuItem("GunJump/Build/All (first time)")]
        public static void BuildAll()
        {
            if (!CanBuild(LevelCorePrefabPath, BootScenePath, LoadingScenePath, Level1ScenePath))
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
            if (!CanBuild(LevelCorePrefabPath))
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
            if (!CanBuild(BootScenePath, LoadingScenePath))
                return;

            string previous = SceneManager.GetActiveScene().path;
            EnsureData();
            BuildBootScene();
            BuildLoadingScene();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            Reopen(previous);
        }

        [MenuItem("GunJump/Build/Level 1 Scene")]
        public static void BuildLevel1Only()
        {
            if (!CanBuild(Level1ScenePath))
                return;

            if (!File.Exists(LevelCorePrefabPath))
            {
                EditorUtility.DisplayDialog("GunJump", "Primero hay que crear el prefab LevelCore (GunJump > Build > LevelCore Prefab).", "Entendido");
                return;
            }

            EnsureData();
            BuildLevel1Scene();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
        }

        // ---------------------------------------------------------------- Condiciones previas

        static bool CanBuild(params string[] outputs)
        {
            if (!EnsureTextMeshPro())
                return false;

            var existing = new List<string>();
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

        // ---------------------------------------------------------------- Datos

        static void EnsureData()
        {
            LoadOrCreate<GunTuning>(GunTuningPath);
            LoadOrCreate<CameraTuning>(CameraTuningPath);
            LoadOrCreate<LevelRules>(LevelRulesPath);
            LoadOrCreate<EconomyConfig>(EconomyConfigPath);
            LoadOrCreate<SingleShot>(SingleShotPath);
            LoadOrCreate<EnemyConfig>(EnemyConfigPath);
            var bulletConfig = LoadOrCreate<BulletConfig>(BulletConfigPath);

            var catalog = LoadOrCreate<LevelCatalog>(LevelCatalogPath);
            if (catalog.Count == 0)
            {
                var serialized = new SerializedObject(catalog);
                SerializedProperty names = serialized.FindProperty("sceneNames");
                names.arraySize = 1;
                names.GetArrayElementAtIndex(0).stringValue = Path.GetFileNameWithoutExtension(Level1ScenePath);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            EnsureMaterial(GroundMaterial, new Color(0.55f, 0.6f, 0.65f));
            EnsureMaterial(GunMaterial, new Color(0.15f, 0.15f, 0.18f));
            EnsureMaterial(EnemyMaterial, new Color(0.85f, 0.25f, 0.2f));
            EnsureMaterial(BulletMaterial, new Color(1f, 0.85f, 0.2f));
            EnsureMaterial(CoinMaterial, new Color(1f, 0.78f, 0.1f));
            EnsureMaterial(FinishMaterial, new Color(0.25f, 0.75f, 0.35f));
            EnsureMaterial(TargetMaterialA, new Color(0.95f, 0.95f, 0.95f));
            EnsureMaterial(TargetMaterialB, new Color(0.9f, 0.35f, 0.3f));

            if (!File.Exists(BulletPrefabPath))
                CreateBulletPrefab(bulletConfig);

            AssetDatabase.SaveAssets();
        }

        static void CreateBulletPrefab(BulletConfig config)
        {
            GameObject bulletObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bulletObject.name = "Bullet";
            Object.DestroyImmediate(bulletObject.GetComponent<Collider>());
            bulletObject.transform.localScale = new Vector3(0.35f, 0.1f, 0.1f);
            bulletObject.GetComponent<MeshRenderer>().sharedMaterial = LoadMaterial(BulletMaterial);

            var bullet = bulletObject.AddComponent<Bullet>();
            SetReference(bullet, "config", config);

            EnsureFolder(FolderOf(BulletPrefabPath));
            PrefabUtility.SaveAsPrefabAsset(bulletObject, BulletPrefabPath);
            Object.DestroyImmediate(bulletObject);
        }

        // ---------------------------------------------------------------- Prefab LevelCore

        static void BuildLevelCorePrefab()
        {
            // Se arma en una escena vacía que no se guarda.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("LevelCore");
            var settings = root.AddComponent<LevelSettings>();
            var core = root.AddComponent<LevelCore>();

            var spawner = new GameObject("BulletSpawner").AddComponent<BulletSpawner>();
            spawner.transform.SetParent(root.transform, false);
            SetReference(spawner, "prefab", Load<GameObject>(BulletPrefabPath).GetComponent<Bullet>());

            Gun gun = CreateGun(root.transform, spawner);
            CreateGameplayCamera(root.transform, gun.transform);

            var input = new GameObject("Input").AddComponent<InputReader>();
            input.transform.SetParent(root.transform, false);

            BuildLevelUi(root.transform, out HudView hud, out PauseView pauseView, out ResultView resultView);

            SetReference(core, "settings", settings);
            SetReference(core, "rules", Load<LevelRules>(LevelRulesPath));
            SetReference(core, "gun", gun);
            SetReference(core, "input", input);
            SetReference(core, "hud", hud);
            SetReference(core, "pauseView", pauseView);
            SetReference(core, "resultView", resultView);

            EnsureFolder(FolderOf(LevelCorePrefabPath));
            PrefabUtility.SaveAsPrefabAsset(root, LevelCorePrefabPath);
            Object.DestroyImmediate(root);
        }

        static Gun CreateGun(Transform parent, BulletSpawner spawner)
        {
            var gunObject = new GameObject("Gun");
            gunObject.transform.SetParent(parent, false);

            var body = gunObject.AddComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var collider = gunObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 0.35f, 0.25f);

            Material material = LoadMaterial(GunMaterial);
            CreateGunPart("Barrel", gunObject.transform, Vector3.zero, new Vector3(1f, 0.35f, 0.25f), material);
            CreateGunPart("Grip", gunObject.transform, new Vector3(-0.3f, -0.35f, 0f), new Vector3(0.25f, 0.45f, 0.2f), material);

            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(gunObject.transform, false);
            muzzle.transform.localPosition = new Vector3(0.6f, 0f, 0f);

            var gun = gunObject.AddComponent<Gun>();
            SetReference(gun, "tuning", Load<GunTuning>(GunTuningPath));
            SetReference(gun, "muzzle", muzzle.transform);
            SetReference(gun, "fireStrategy", Load<SingleShot>(SingleShotPath));
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
            // Sin AudioListener: el único del juego vive en la escena Boot.
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(parent, false);

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = SkyColor;

            var follow = cameraObject.AddComponent<FollowCamera>();
            SetReference(follow, "tuning", Load<CameraTuning>(CameraTuningPath));
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
            var labelSize = new Vector2(700f, 70f);

            TMP_Text ammo = UiFactory.CreateLabel("Ammo", safeArea, "Balas 0", 48f, TextAlignmentOptions.TopLeft, HudTextColor);
            UiFactory.Place(ammo.rectTransform, UiFactory.TopLeft, new Vector2(40f, -40f), labelSize);

            TMP_Text coins = UiFactory.CreateLabel("Coins", safeArea, "Monedas 0", 48f, TextAlignmentOptions.TopLeft, HudTextColor);
            UiFactory.Place(coins.rectTransform, UiFactory.TopLeft, new Vector2(40f, -110f), labelSize);

            TMP_Text enemies = UiFactory.CreateLabel("Enemies", safeArea, "Enemigos 0", 48f, TextAlignmentOptions.TopLeft, HudTextColor);
            UiFactory.Place(enemies.rectTransform, UiFactory.TopLeft, new Vector2(40f, -180f), labelSize);

            CommandButton pauseButton = UiFactory.CreateButton("PauseButton", safeArea, "II", 56f);
            UiFactory.Place((RectTransform)pauseButton.transform, UiFactory.TopRight, new Vector2(-40f, -40f), new Vector2(130f, 130f));

            SetReference(hud, "ammoLabel", ammo);
            SetReference(hud, "coinsLabel", coins);
            SetReference(hud, "enemiesLabel", enemies);
            SetReference(hud, "pauseButton", pauseButton);
        }

        static void BuildPausePanel(Transform canvas, PauseView pauseView)
        {
            GameObject panel = UiFactory.CreateOverlay("PausePanel", canvas);
            var buttonSize = new Vector2(600f, 150f);

            TMP_Text title = UiFactory.CreateLabel("Title", panel.transform, "Pausa", 96f, TextAlignmentOptions.Center, Color.white);
            UiFactory.Place(title.rectTransform, UiFactory.Center, new Vector2(0f, 260f), new Vector2(900f, 160f));

            CommandButton resume = UiFactory.CreateButton("ResumeButton", panel.transform, "Seguir", 52f);
            UiFactory.Place((RectTransform)resume.transform, UiFactory.Center, new Vector2(0f, 40f), buttonSize);

            CommandButton retry = UiFactory.CreateButton("RetryButton", panel.transform, "Reintentar", 52f);
            UiFactory.Place((RectTransform)retry.transform, UiFactory.Center, new Vector2(0f, -150f), buttonSize);

            SetReference(pauseView, "panel", panel);
            SetReference(pauseView, "resumeButton", resume);
            SetReference(pauseView, "retryButton", retry);
            panel.SetActive(false);
        }

        static void BuildResultPanel(Transform canvas, ResultView resultView)
        {
            GameObject panel = UiFactory.CreateOverlay("ResultPanel", canvas);
            var buttonSize = new Vector2(600f, 150f);

            TMP_Text title = UiFactory.CreateLabel("Title", panel.transform, "¡Nivel completado!", 80f, TextAlignmentOptions.Center, Color.white);
            UiFactory.Place(title.rectTransform, UiFactory.Center, new Vector2(0f, 460f), new Vector2(1000f, 160f));

            RectTransform starsRow = UiFactory.CreateRect("Stars", panel.transform);
            UiFactory.Place(starsRow, UiFactory.Center, new Vector2(0f, 270f), new Vector2(500f, 140f));
            var stars = new Object[3];
            for (int i = 0; i < stars.Length; i++)
            {
                Image star = UiFactory.CreateImage("Star" + (i + 1), starsRow, Color.white, false);
                UiFactory.Place(star.rectTransform, UiFactory.Center, new Vector2((i - 1) * 170f, 0f), new Vector2(100f, 100f));
                star.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                stars[i] = star;
            }

            TMP_Text coins = UiFactory.CreateLabel("Coins", panel.transform, "+0 monedas", 64f, TextAlignmentOptions.Center, Color.white);
            UiFactory.Place(coins.rectTransform, UiFactory.Center, new Vector2(0f, 80f), new Vector2(900f, 100f));

            TMP_Text multiplier = UiFactory.CreateLabel("Multiplier", panel.transform, "Multiplicador x1", 44f, TextAlignmentOptions.Center, Color.white);
            UiFactory.Place(multiplier.rectTransform, UiFactory.Center, new Vector2(0f, -10f), new Vector2(900f, 80f));

            CommandButton next = UiFactory.CreateButton("NextButton", panel.transform, "Siguiente nivel", 52f);
            UiFactory.Place((RectTransform)next.transform, UiFactory.Center, new Vector2(0f, -220f), buttonSize);

            CommandButton retry = UiFactory.CreateButton("RetryButton", panel.transform, "Reintentar", 52f);
            UiFactory.Place((RectTransform)retry.transform, UiFactory.Center, new Vector2(0f, -410f), buttonSize);

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

        // ---------------------------------------------------------------- Escenas Boot y Loading

        static void BuildBootScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootstrap = new GameObject("Bootstrap").AddComponent<GameBootstrap>();
            SetReference(bootstrap, "economyConfig", Load<EconomyConfig>(EconomyConfigPath));
            SetReference(bootstrap, "levels", Load<LevelCatalog>(LevelCatalogPath));

            UiFactory.CreateEventSystem();

            new GameObject("Audio", typeof(AudioListener));

            SaveScene(scene, BootScenePath);
        }

        static void BuildLoadingScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Mientras se carga no hay cámara de juego: esta limpia la pantalla.
            var camera = new GameObject("Loading Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = LoadingColor;
            camera.cullingMask = 0;

            GameObject canvas = UiFactory.CreateCanvas("UI", null);
            var view = canvas.AddComponent<LoadingView>();

            Image background = UiFactory.CreateImage("Background", canvas.transform, LoadingColor, true);
            UiFactory.Stretch(background.rectTransform);

            TMP_Text label = UiFactory.CreateLabel("Label", canvas.transform, "Cargando", 64f, TextAlignmentOptions.Center, Color.white);
            UiFactory.Place(label.rectTransform, UiFactory.Center, new Vector2(0f, 60f), new Vector2(900f, 120f));

            Image bar = UiFactory.CreateImage("Bar", canvas.transform, new Color(1f, 1f, 1f, 0.2f), false);
            UiFactory.Place(bar.rectTransform, UiFactory.Center, new Vector2(0f, -60f), new Vector2(700f, 24f));

            Image fill = UiFactory.CreateImage("Fill", bar.transform, Color.white, false);
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = new Vector2(0f, 1f);
            fill.rectTransform.offsetMin = Vector2.zero;
            fill.rectTransform.offsetMax = Vector2.zero;
            SetReference(view, "fill", fill.rectTransform);

            SaveScene(scene, LoadingScenePath);
        }

        // ---------------------------------------------------------------- Escena Level1

        static void BuildLevel1Scene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // La posición del prefab es el punto de partida del arma.
            var core = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(LevelCorePrefabPath), scene);
            core.transform.position = new Vector3(0f, 3f, 0f);

            Transform geometry = new GameObject("Geometry").transform;
            Material ground = LoadMaterial(GroundMaterial);
            CreateBlock("Ground_A", geometry, new Vector3(5f, -1f, 0f), new Vector3(22f, 2f, Depth), ground);
            CreateBlock("Ground_B", geometry, new Vector3(30f, -1f, 0f), new Vector3(22f, 2f, Depth), ground);
            CreateBlock("Step", geometry, new Vector3(33f, 0.75f, 0f), new Vector3(6f, 1.5f, Depth), ground);
            CreateBlock("Ground_C", geometry, new Vector3(54.75f, -1f, 0f), new Vector3(20.5f, 2f, Depth), ground);

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

            SaveScene(scene, Level1ScenePath);
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
            enemy.GetComponent<MeshRenderer>().sharedMaterial = LoadMaterial(EnemyMaterial);

            var body = enemy.AddComponent<Rigidbody>();
            body.isKinematic = true;

            var staticEnemy = enemy.AddComponent<StaticEnemy>();
            SetReference(staticEnemy, "config", Load<EnemyConfig>(EnemyConfigPath));
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
            coin.GetComponent<MeshRenderer>().sharedMaterial = LoadMaterial(CoinMaterial);

            // El trigger es más grande que la moneda para que juntarla no exija precisión.
            var trigger = coin.GetComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(3f, 3f, Depth / 0.15f);
            coin.AddComponent<CoinPickup>();
        }

        static void CreateFinish(Vector3 groundPosition)
        {
            var finish = new GameObject("Finish");
            finish.transform.position = groundPosition;

            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = "Pad";
            Object.DestroyImmediate(pad.GetComponent<Collider>());
            pad.transform.SetParent(finish.transform, false);
            pad.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            pad.transform.localScale = new Vector3(5f, 0.06f, Depth);
            pad.GetComponent<MeshRenderer>().sharedMaterial = LoadMaterial(FinishMaterial);

            var zone = new GameObject("Zone");
            zone.transform.SetParent(finish.transform, false);
            zone.transform.localPosition = new Vector3(0f, 3f, 0f);
            var trigger = zone.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(5f, 6f, Depth);
            zone.AddComponent<FinishZone>();
        }

        // Una pared de blancos apilados: cuanto más alto, mayor el multiplicador y más difícil de acertar.
        static void CreateMultiplierWall(Vector3 groundPosition)
        {
            const float blockHeight = 1.5f;
            const float spacing = 1.55f;
            const float thickness = 1f;

            var wall = new GameObject("MultiplierWall");
            wall.transform.position = groundPosition;

            for (int i = 0; i < MultiplierSteps; i++)
            {
                int value = i + 1;
                var target = new GameObject("Target_x" + value);
                target.transform.SetParent(wall.transform, false);
                target.transform.localPosition = new Vector3(0f, blockHeight * 0.5f + i * spacing, 0f);

                var collider = target.AddComponent<BoxCollider>();
                collider.size = new Vector3(thickness, blockHeight, Depth);

                var visual = new GameObject("Visual");
                visual.transform.SetParent(target.transform, false);

                GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.name = "Block";
                Object.DestroyImmediate(block.GetComponent<Collider>());
                block.transform.SetParent(visual.transform, false);
                block.transform.localScale = new Vector3(thickness, blockHeight, Depth);
                block.GetComponent<MeshRenderer>().sharedMaterial = LoadMaterial(i % 2 == 0 ? TargetMaterialA : TargetMaterialB);

                // El número va en la cara que mira hacia el jugador que se acerca.
                var label = new GameObject("Label").AddComponent<TextMeshPro>();
                label.transform.SetParent(visual.transform, false);
                label.transform.localPosition = new Vector3(-(thickness * 0.5f + 0.02f), 0f, 0f);
                label.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                label.rectTransform.sizeDelta = new Vector2(Depth, blockHeight);
                label.text = "x" + value;
                label.fontSize = 10f;
                label.alignment = TextAlignmentOptions.Center;
                label.color = HudTextColor;

                var multiplierTarget = target.AddComponent<MultiplierTarget>();
                SetReference(multiplierTarget, "visual", visual);
                var serialized = new SerializedObject(multiplierTarget);
                serialized.FindProperty("multiplier").intValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // ---------------------------------------------------------------- Utilidades

        static void SaveScene(Scene scene, string path)
        {
            EnsureFolder(FolderOf(path));
            EditorSceneManager.SaveScene(scene, path);
        }

        // Boot, Loading y Level1 quedan primeras y en ese orden. La escena del prototipo sale de la lista:
        // sigue sirviendo para probar en el Editor, pero no va en el build.
        static void UpdateBuildSettings()
        {
            string[] ordered = { BootScenePath, LoadingScenePath, Level1ScenePath };
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string path in ordered)
            {
                if (File.Exists(path))
                    scenes.Add(new EditorBuildSettingsScene(path, true));
            }

            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                if (existing.path == PrototypeScenePath || Array.IndexOf(ordered, existing.path) >= 0)
                    continue;

                scenes.Add(existing);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static T Load<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
                throw new InvalidOperationException("No se pudo cargar " + path);

            return asset;
        }

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            EnsureFolder(FolderOf(path));
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static string MaterialPath(string name)
        {
            return Root + "/Art/Materials/" + name + ".mat";
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

            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
                throw new InvalidOperationException($"{target.GetType().Name} no tiene el campo serializado '{field}'.");

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetReferences(Object target, string field, Object[] values)
        {
            var serialized = new SerializedObject(target);
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
