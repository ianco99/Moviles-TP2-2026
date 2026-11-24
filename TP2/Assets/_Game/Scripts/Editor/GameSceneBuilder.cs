using System;
using System.Collections.Generic;
using System.IO;
using _Game.Scripts.Shop;
using GunJump.Core;
using GunJump.Gameplay;
using GunJump.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GunJump.EditorTools
{
    public static class GameSceneBuilder
    {
        public const string SCENES_FOLDER = ROOT + "/Scenes";
        public const string MAIN_MENU_SCENE_PATH = SCENES_FOLDER + "/MainMenu.unity";
        public const string LOADING_SCENE_PATH = SCENES_FOLDER + "/Loading.unity";

        const string ROOT = "Assets/_Game";
        const string LEVEL1_SCENE_PATH = SCENES_FOLDER + "/Level1.unity";
        const string LEVEL2_SCENE_PATH = SCENES_FOLDER + "/Level2.unity";
        const string LEVEL3_SCENE_PATH = SCENES_FOLDER + "/Level3.unity";
        const string LEVEL4_SCENE_PATH = SCENES_FOLDER + "/Level4.unity";
        const string LEVEL5_SCENE_PATH = SCENES_FOLDER + "/Level5.unity";
        const string PROTOTYPE_SCENE_PATH = SCENES_FOLDER + "/Prototype.unity";
        const string LEVEL_CORE_PREFAB_PATH = ROOT + "/Prefabs/Level/LevelCore.prefab";
        const string GUN_RIG_PREFAB_PATH = ROOT + "/Prefabs/Gun/GunRig.prefab";
        const string ENEMY_PREFAB_PATH = ROOT + "/Prefabs/Level/Enemy.prefab";
        const string COIN_PREFAB_PATH = ROOT + "/Prefabs/Level/Coin.prefab";
        const string FINISH_PREFAB_PATH = ROOT + "/Prefabs/Level/Finish.prefab";
        const string MULTIPLIER_WALL_PREFAB_PATH = ROOT + "/Prefabs/Level/MultiplierWall.prefab";
        const string PLATFORM_PREFAB_PATH = ROOT + "/Prefabs/Level/Platform.prefab";
        const string MOVING_PLATFORM_PREFAB_PATH = ROOT + "/Prefabs/Level/MovingPlatform.prefab";
        const string SAW_PREFAB_PATH = ROOT + "/Prefabs/Level/Saw.prefab";
        const string SPIKES_PREFAB_PATH = ROOT + "/Prefabs/Level/Spikes.prefab";
        const string TUTORIAL_ZONE_PREFAB_PATH = ROOT + "/Prefabs/Level/TutorialZone.prefab";
        const string BULLET_PREFAB_PATH = ROOT + "/Prefabs/Runtime/Bullet.prefab";

        const string GUN_TUNING_PATH = ROOT + "/Data/Tuning/GunTuning.asset";
        const string CAMERA_TUNING_PATH = ROOT + "/Data/Tuning/CameraTuning.asset";
        const string LEVEL_RULES_PATH = ROOT + "/Data/Tuning/LevelRules.asset";
        const string ECONOMY_CONFIG_PATH = ROOT + "/Data/Tuning/EconomyConfig.asset";
        const string LEVEL_LIST_PATH = ROOT + "/Data/Menu/LevelListSO.asset";
        const string SHOP_CATALOG_PATH = ROOT + "/Data/Shop/ShopCatalog.asset";
        const string BULLET_CONFIG_PATH = ROOT + "/Data/Weapons/PistolBullet.asset";
        const string SINGLE_SHOT_PATH = ROOT + "/Data/Weapons/SingleShot.asset";
        const string HEAVY_SHOT_PATH = ROOT + "/Data/Weapons/HeavyShot.asset";
        const string SPREAD_SHOT_PATH = ROOT + "/Data/Weapons/SpreadShot.asset";
        const string BURST_SHOT_PATH = ROOT + "/Data/Weapons/BurstShot.asset";
        const string WEAPON_CATALOG_PATH = ROOT + "/Data/Weapons/WeaponCatalog.asset";
        const string ENEMY_CONFIG_PATH = ROOT + "/Data/Enemies/StaticEnemy.asset";

        const string GROUND_MATERIAL = "Proto_Ground";
        const string GUN_MATERIAL = "Proto_Gun";
        const string ENEMY_MATERIAL = "Proto_Enemy";
        const string BULLET_MATERIAL = "Proto_Bullet";
        const string COIN_MATERIAL = "Proto_Coin";
        const string FINISH_MATERIAL = "Proto_Finish";
        const string TARGET_MATERIAL_A = "Proto_TargetA";
        const string TARGET_MATERIAL_B = "Proto_TargetB";
        const string HAZARD_MATERIAL = "Proto_Hazard";
        const string MOVING_MATERIAL = "Proto_Moving";

        const string PANEL_SPRITE_NAME = "UI/Skin/Background.psd";
        const string KNOB_SPRITE_NAME = "UI/Skin/Knob.psd";
        const string SAFE_AREA_NAME = "SafeArea";
        const string TUTORIAL_GROUP_NAME = "Tutorial";
        const string GEOMETRY_GROUP_NAME = "Geometry";
        const string PLATFORMS_GROUP_NAME = "Platforms";
        const string ENEMIES_GROUP_NAME = "Enemies";
        const string COINS_GROUP_NAME = "Coins";
        const string HAZARDS_GROUP_NAME = "Hazards";
        const string LEVEL1_START_HINT = "Tap the screen to shoot";
        const string DEFAULT_ZONE_HINT = "Hint text";

        const float DEPTH = 8f;
        const int MULTIPLIER_STEPS = 5;
        const float HEAVY_SHOT_RECOIL = 11f;
        const float LEVEL1_START_TIME_SCALE = 0.35f;
        const float SAW_RADIUS = 0.8f;
        const int SAW_TEETH = 4;
        const float SPIKES_WIDTH = 4f;
        const float SPIKE_HEIGHT = 0.6f;
        const int SPIKE_COUNT = 5;
        const float HINT_FONT_SIZE = 46f;
        const float HINT_PANEL_BOTTOM = 220f;

        static readonly string[] SPREAD_GUN_IDS = { "Guns_Shotgun" };
        static readonly string[] BURST_GUN_IDS = { "Guns_BurstPistol", "Guns_MachineGun" };
        static readonly string[] HEAVY_GUN_IDS = { "Guns_Sniper", "Guns_SingleCannon", "Guns_GrenadeLauncher" };
        internal static readonly string[] LEVEL_SCENE_PATHS = { LEVEL1_SCENE_PATH, LEVEL2_SCENE_PATH, LEVEL3_SCENE_PATH, LEVEL4_SCENE_PATH, LEVEL5_SCENE_PATH };
        static readonly string[] TEMPLATE_LEVEL_PATHS = { LEVEL2_SCENE_PATH, LEVEL3_SCENE_PATH, LEVEL4_SCENE_PATH, LEVEL5_SCENE_PATH };
        static readonly int[] TEMPLATE_AMMO = { 40, 30, 30, 25 };
        static readonly float[] TEMPLATE_TARGET_TIMES = { 40f, 45f, 45f, 50f };

        static readonly float[] TUTORIAL_ZONE_X = { 3f, 10f, 28f, 52f };
        static readonly string[] TUTORIAL_HINTS =
        {
            "Each shot pushes you away from the barrel",
            "Shoot enemies: they drop coins",
            "Aim down to go up",
            "Reach the goal and hit the multiplier!"
        };

        static readonly Vector3 PLATFORM_SIZE = new Vector3(4f, 1f, DEPTH);
        static readonly Vector3 MOVING_PLATFORM_SIZE = new Vector3(4f, 0.5f, DEPTH);
        static readonly Vector3 TUTORIAL_ZONE_SIZE = new Vector3(5f, 12f, DEPTH);
        static readonly Vector3 TUTORIAL_ZONE_CENTER = new Vector3(0f, 4f, 0f);
        static readonly Vector3 LEVEL_CORE_POSITION = new Vector3(0f, 3f, 0f);
        static readonly Vector3 FINISH_POSITION = new Vector3(58.5f, 0f, 0f);
        static readonly Vector3 MULTIPLIER_WALL_POSITION = new Vector3(63.5f, 0f, 0f);
        static readonly Vector3 TEMPLATE_GROUND_SIZE = new Vector3(16f, 2f, DEPTH);
        static readonly Vector3 TEMPLATE_START_GROUND_CENTER = new Vector3(2f, -1f, 0f);
        static readonly Vector3 TEMPLATE_FINISH_GROUND_CENTER = new Vector3(57f, -1f, 0f);
        static readonly Vector2 BOTTOM_CENTER = new Vector2(0.5f, 0f);
        static readonly Vector2 HINT_PANEL_SIZE = new Vector2(940f, 190f);
        static readonly Vector2 HINT_PADDING = new Vector2(30f, 20f);
        static readonly Vector2 HINT_RING_POSITION = new Vector2(0f, -150f);
        static readonly Vector2 HINT_RING_SIZE = new Vector2(180f, 180f);
        static readonly Color HINT_PANEL_COLOR = new Color(0f, 0f, 0f, 0.6f);
        static readonly Color HINT_RING_COLOR = new Color(1f, 1f, 1f, 0.75f);
        static readonly Vector2 BUTTON_SIZE = new Vector2(600f, 150f);
        static readonly Vector2 HALF_BUTTON_SIZE = new Vector2(290f, 150f);
        static readonly Color HUD_TEXT_COLOR = new Color(0.1f, 0.1f, 0.15f);
        static readonly Color SKY_COLOR = new Color(0.62f, 0.8f, 0.92f);
        static readonly Color LOADING_COLOR = new Color(0.1f, 0.12f, 0.18f);

        const int LOADING_SORTING_ORDER = 100;

        [MenuItem("GunJump/Build/All (first time)")]
        public static void BuildAll()
        {
            List<string> outputs = new List<string>(LEVEL_SCENE_PATHS) { GUN_RIG_PREFAB_PATH, LEVEL_CORE_PREFAB_PATH };
            if (!CanBuild(outputs.ToArray()))
                return;

            EnsureData();
            BuildGunRigPrefab();
            BuildLevelPrefabs();
            BuildLevelCorePrefab();
            BuildLevel1Scene();

            for (int i = 0; i < TEMPLATE_LEVEL_PATHS.Length; i++)
                BuildLevelTemplate(TEMPLATE_LEVEL_PATHS[i], TEMPLATE_AMMO[i], TEMPLATE_TARGET_TIMES[i]);

            RegisterLevels();
            AssetDatabase.SaveAssets();
            Reopen(LEVEL1_SCENE_PATH);
            Debug.Log("GunJump: GunRig, prefabs de nivel, LevelCore y Level1 a Level5 creados; los cinco quedaron en Build Settings y en LevelListSO.");
        }

        [MenuItem("GunJump/Build/Levels and Tutorial")]
        public static void BuildLevelsAndTutorial()
        {
            if (!File.Exists(LEVEL_CORE_PREFAB_PATH) || !File.Exists(LEVEL1_SCENE_PATH))
            {
                EditorUtility.DisplayDialog("GunJump", "Primero hay que crear LevelCore y Level1 (GunJump > Build > All (first time)).", "Entendido");
                return;
            }

            if (!CanBuild())
                return;

            EnsureData();
            BuildLevelPrefabs();
            bool addedHint = AddHintPanelToLevelCore();

            Scene level1 = EditorSceneManager.OpenScene(LEVEL1_SCENE_PATH, OpenSceneMode.Single);
            bool hasTutorial = FindRoot(level1, TUTORIAL_GROUP_NAME) != null;
            bool replaceTutorial = !hasTutorial || EditorUtility.DisplayDialog(
                "GunJump",
                "Level1 ya tiene tutorial. ¿Reemplazar sus zonas y el texto de inicio? Se pierde lo que se les haya editado a mano.",
                "Reemplazar",
                "Dejarlo");

            if (replaceTutorial)
            {
                AddTutorial(level1);
                EditorSceneManager.SaveScene(level1);
            }

            List<string> created = new List<string>();
            for (int i = 0; i < TEMPLATE_LEVEL_PATHS.Length; i++)
            {
                if (File.Exists(TEMPLATE_LEVEL_PATHS[i]))
                    continue;

                BuildLevelTemplate(TEMPLATE_LEVEL_PATHS[i], TEMPLATE_AMMO[i], TEMPLATE_TARGET_TIMES[i]);
                created.Add(Path.GetFileNameWithoutExtension(TEMPLATE_LEVEL_PATHS[i]));
            }

            RegisterLevels();
            AssetDatabase.SaveAssets();
            Reopen(LEVEL1_SCENE_PATH);

            string hintResult = addedHint ? "agregado" : "ya estaba";
            string tutorialResult = replaceTutorial ? "armado" : "sin cambios";
            string templatesResult = created.Count > 0 ? string.Join(", ", created) : "ninguna";
            Debug.Log($"GunJump: cartel de ayuda en LevelCore: {hintResult}. Tutorial de Level1: {tutorialResult}. " +
                      $"Plantillas nuevas: {templatesResult}. Level1 a Level5 quedaron en Build Settings y en LevelListSO.");
        }

        [MenuItem("GunJump/Build/Level Drafts")]
        public static void BuildLevelDrafts()
        {
            if (!CanBuild())
                return;

            EnsureData();
            BuildLevelPrefabs();

            List<string> drafted = new List<string>();
            List<string> skipped = new List<string>();

            for (int i = 0; i < TEMPLATE_LEVEL_PATHS.Length; i++)
            {
                string path = TEMPLATE_LEVEL_PATHS[i];
                string levelName = Path.GetFileNameWithoutExtension(path);

                if (!File.Exists(path))
                {
                    skipped.Add(levelName + " (no existe)");
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                LevelGroups groups = FindLevelGroups(scene);

                if (groups.enemies.childCount > 0 || groups.platforms.childCount > 0 || groups.hazards.childCount > 0)
                {
                    skipped.Add(levelName + " (ya tiene contenido)");
                    continue;
                }

                DraftLevel(i, groups);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                drafted.Add(levelName);
            }

            AssetDatabase.SaveAssets();
            Reopen(LEVEL1_SCENE_PATH);
            Debug.Log($"GunJump: borradores armados en {(drafted.Count > 0 ? string.Join(", ", drafted) : "ningún nivel")}. " +
                      $"Sin tocar: {(skipped.Count > 0 ? string.Join(", ", skipped) : "ninguno")}. Ajustalos jugándolos en el teléfono.");
        }

        static LevelGroups FindLevelGroups(Scene scene)
        {
            return new LevelGroups
            {
                geometry = FindOrCreateRoot(scene, GEOMETRY_GROUP_NAME),
                platforms = FindOrCreateRoot(scene, PLATFORMS_GROUP_NAME),
                enemies = FindOrCreateRoot(scene, ENEMIES_GROUP_NAME),
                coins = FindOrCreateRoot(scene, COINS_GROUP_NAME),
                hazards = FindOrCreateRoot(scene, HAZARDS_GROUP_NAME)
            };
        }

        static Transform FindOrCreateRoot(Scene scene, string name)
        {
            GameObject root = FindRoot(scene, name);
            if (root != null)
                return root.transform;

            root = new GameObject(name);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root.transform;
        }

        static void DraftLevel(int templateIndex, LevelGroups groups)
        {
            switch (templateIndex)
            {
                case 0:
                    DraftRooftops(groups);
                    break;
                case 1:
                    DraftWarehouse(groups);
                    break;
                case 2:
                    DraftFoundry(groups);
                    break;
                case 3:
                    DraftFinale(groups);
                    break;
            }
        }

        static void DraftRooftops(LevelGroups groups)
        {
            PlaceAll(PLATFORM_PREFAB_PATH, "Roof", groups.platforms, new[]
            {
                new Vector3(14f, 1f, 0f), new Vector3(20f, 2.5f, 0f), new Vector3(26f, 1.5f, 0f),
                new Vector3(32f, 3f, 0f), new Vector3(38f, 2f, 0f), new Vector3(44f, 1f, 0f)
            });
            PlaceAll(ENEMY_PREFAB_PATH, "Enemy", groups.enemies, new[]
            {
                new Vector3(6f, 1f, 0f), new Vector3(20f, 4f, 0f), new Vector3(32f, 4.5f, 0f),
                new Vector3(44f, 2.5f, 0f), new Vector3(51f, 1f, 0f), new Vector3(54f, 1f, 0f)
            });
            PlaceAll(COIN_PREFAB_PATH, "Coin", groups.coins, new[]
            {
                new Vector3(3f, 2f, 0f), new Vector3(17f, 3.5f, 0f), new Vector3(23f, 4.5f, 0f), new Vector3(29f, 4f, 0f),
                new Vector3(35f, 5f, 0f), new Vector3(41f, 4f, 0f), new Vector3(47f, 3f, 0f)
            });
        }

        static void DraftWarehouse(LevelGroups groups)
        {
            PlaceMovingPlatform(groups.platforms, "Lift_1", new Vector3(14f, 0f, 0f), new Vector3(0f, 3f, 0f));
            PlaceAll(PLATFORM_PREFAB_PATH, "Shelf", groups.platforms, new[]
            {
                new Vector3(20f, 3f, 0f), new Vector3(31f, 5.5f, 0f), new Vector3(37f, 2f, 0f)
            });
            PlaceMovingPlatform(groups.platforms, "Conveyor", new Vector3(25f, 3f, 0f), new Vector3(6f, 0f, 0f));
            PlaceMovingPlatform(groups.platforms, "Lift_2", new Vector3(43f, 0.5f, 0f), new Vector3(0f, 3f, 0f));
            PlaceAll(ENEMY_PREFAB_PATH, "Enemy", groups.enemies, new[]
            {
                new Vector3(4f, 1f, 0f), new Vector3(8f, 1f, 0f), new Vector3(20f, 4.5f, 0f), new Vector3(31f, 7f, 0f),
                new Vector3(37f, 3.5f, 0f), new Vector3(50f, 1f, 0f), new Vector3(53f, 1f, 0f)
            });
            PlaceAll(COIN_PREFAB_PATH, "Coin", groups.coins, new[]
            {
                new Vector3(14f, 4f, 0f), new Vector3(20f, 5.5f, 0f), new Vector3(26f, 5f, 0f), new Vector3(29f, 5f, 0f),
                new Vector3(32f, 4f, 0f), new Vector3(37f, 4.5f, 0f), new Vector3(43f, 4.5f, 0f), new Vector3(47f, 3f, 0f)
            });
        }

        static void DraftFoundry(LevelGroups groups)
        {
            Material ground = LoadMaterial(GROUND_MATERIAL);
            CreateBlock("Ground_Middle", groups.geometry, new Vector3(29.5f, -1f, 0f), new Vector3(39f, 2f, DEPTH), ground);
            CreateBlock("Ceiling", groups.geometry, new Vector3(27f, 6.5f, 0f), new Vector3(10f, 1f, DEPTH), ground);

            PlaceAll(SPIKES_PREFAB_PATH, "Spikes", groups.hazards, new[]
            {
                new Vector3(15f, 0f, 0f), new Vector3(36f, 0f, 0f), new Vector3(44f, 0f, 0f)
            });
            PlaceCeilingSpikes(groups.hazards, "CeilingSpikes_1", new Vector3(25f, 6f, 0f));
            PlaceCeilingSpikes(groups.hazards, "CeilingSpikes_2", new Vector3(29f, 6f, 0f));
            PlaceAll(SAW_PREFAB_PATH, "Saw", groups.hazards, new[]
            {
                new Vector3(19f, 2.5f, 0f), new Vector3(40f, 3f, 0f)
            });
            PlaceAll(ENEMY_PREFAB_PATH, "Enemy", groups.enemies, new[]
            {
                new Vector3(8f, 1f, 0f), new Vector3(12f, 1f, 0f), new Vector3(26f, 1f, 0f), new Vector3(31f, 1f, 0f),
                new Vector3(47f, 1f, 0f), new Vector3(51f, 1f, 0f), new Vector3(54f, 1f, 0f)
            });
            PlaceAll(COIN_PREFAB_PATH, "Coin", groups.coins, new[]
            {
                new Vector3(12f, 3f, 0f), new Vector3(19f, 5f, 0f), new Vector3(27f, 3f, 0f),
                new Vector3(33f, 3f, 0f), new Vector3(40f, 5.5f, 0f), new Vector3(47f, 3f, 0f)
            });
        }

        static void DraftFinale(LevelGroups groups)
        {
            Material ground = LoadMaterial(GROUND_MATERIAL);
            CreateBlock("Ground_Island", groups.geometry, new Vector3(45.5f, -1f, 0f), new Vector3(7f, 2f, DEPTH), ground);
            CreateBlock("Ceiling", groups.geometry, new Vector3(38f, 6.5f, 0f), new Vector3(8f, 1f, DEPTH), ground);

            PlaceAll(PLATFORM_PREFAB_PATH, "Platform", groups.platforms, new[]
            {
                new Vector3(14f, 1f, 0f), new Vector3(21f, 2.5f, 0f), new Vector3(35f, 2f, 0f)
            });
            PlaceMovingPlatform(groups.platforms, "Lift", new Vector3(28f, 1f, 0f), new Vector3(0f, 3f, 0f));
            PlaceAll(SAW_PREFAB_PATH, "Saw", groups.hazards, new[]
            {
                new Vector3(24.5f, 5f, 0f), new Vector3(40f, 2.5f, 0f)
            });
            PlaceAll(SPIKES_PREFAB_PATH, "Spikes", groups.hazards, new[]
            {
                new Vector3(46f, 0f, 0f)
            });
            PlaceCeilingSpikes(groups.hazards, "CeilingSpikes", new Vector3(38f, 6f, 0f));
            PlaceAll(ENEMY_PREFAB_PATH, "Enemy", groups.enemies, new[]
            {
                new Vector3(4f, 1f, 0f), new Vector3(8f, 1f, 0f), new Vector3(14f, 2.5f, 0f), new Vector3(21f, 4f, 0f),
                new Vector3(35f, 3.5f, 0f), new Vector3(43f, 1f, 0f), new Vector3(50f, 1f, 0f), new Vector3(52f, 1f, 0f),
                new Vector3(54f, 1f, 0f)
            });
            PlaceAll(COIN_PREFAB_PATH, "Coin", groups.coins, new[]
            {
                new Vector3(17f, 3.5f, 0f), new Vector3(24.5f, 3f, 0f), new Vector3(28f, 5f, 0f), new Vector3(31.5f, 4f, 0f),
                new Vector3(38f, 4f, 0f), new Vector3(42f, 3f, 0f), new Vector3(47f, 3f, 0f)
            });
        }

        static void PlaceAll(string prefabPath, string baseName, Transform parent, Vector3[] positions)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                PlacePrefab(prefabPath, baseName + "_" + (i + 1), parent, positions[i]);
            }
        }

        static void PlaceMovingPlatform(Transform parent, string name, Vector3 position, Vector3 travel)
        {
            GameObject platform = PlacePrefab(MOVING_PLATFORM_PREFAB_PATH, name, parent, position);
            SerializedObject serialized = new SerializedObject(platform.GetComponent<MovingPlatform>());
            serialized.FindProperty("travel").vector3Value = travel;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void PlaceCeilingSpikes(Transform parent, string name, Vector3 position)
        {
            GameObject spikes = PlacePrefab(SPIKES_PREFAB_PATH, name, parent, position);
            spikes.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }

        [MenuItem("GunJump/Build/Drop Prototype From Build")]
        public static void DropPrototypeFromBuild()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            bool wasInBuild = false;

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == PROTOTYPE_SCENE_PATH)
                {
                    wasInBuild = true;
                    continue;
                }

                scenes.Add(scene);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            RegisterLevels();
            AssetDatabase.SaveAssets();
            Debug.Log(wasInBuild
                ? "GunJump: Prototype salió de Build Settings y LevelListSO apunta a los nuevos índices de Level1 a Level5."
                : "GunJump: Prototype no estaba en Build Settings; LevelListSO quedó actualizado igual.");
        }

        [MenuItem("GunJump/Build/Gun Rig (from Prototype)")]
        public static void BuildGunRigOnly()
        {
            if (!CanBuild(GUN_RIG_PREFAB_PATH))
                return;

            EnsureData();
            BuildGunRigPrefab();
            AssetDatabase.SaveAssets();
            Debug.Log($"GunJump: {GUN_RIG_PREFAB_PATH} copiado del arma de Prototype.");
        }

        [MenuItem("GunJump/Build/Level Prefabs")]
        public static void BuildLevelPrefabsOnly()
        {
            if (!CanBuild())
                return;

            EnsureData();
            List<string> created = BuildLevelPrefabs();
            AssetDatabase.SaveAssets();
            Debug.Log(created.Count > 0
                ? "GunJump: prefabs creados en Prefabs/Level: " + string.Join(", ", created) + ". Los que ya existían no se tocaron."
                : "GunJump: ya existían todos los prefabs de nivel. Para rehacer uno, borralo y volvé a correr este menú.");
        }

        [MenuItem("GunJump/Build/LevelCore Prefab")]
        public static void BuildLevelCoreOnly()
        {
            if (!File.Exists(GUN_RIG_PREFAB_PATH))
            {
                EditorUtility.DisplayDialog("GunJump", "Primero hay que crear el GunRig (GunJump > Build > Gun Rig (from Prototype)).", "Entendido");
                return;
            }

            if (!CanBuild(LEVEL_CORE_PREFAB_PATH))
                return;

            string previous = SceneManager.GetActiveScene().path;
            EnsureData();
            BuildLevelCorePrefab();
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
            if (!File.Exists(LEVEL_CORE_PREFAB_PATH) || !File.Exists(ENEMY_PREFAB_PATH))
            {
                EditorUtility.DisplayDialog("GunJump", "Primero hay que crear el prefab LevelCore y los prefabs de nivel (GunJump > Build > All).", "Entendido");
                return;
            }

            if (!CanBuild(LEVEL1_SCENE_PATH))
                return;

            EnsureData();
            BuildLevelPrefabs();
            BuildLevel1Scene();
            RegisterLevels();
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
            LoadOrCreate<EnemyConfig>(ENEMY_CONFIG_PATH);
            BulletConfig bulletConfig = LoadOrCreate<BulletConfig>(BULLET_CONFIG_PATH);

            EnsureWeapons();

            EnsureMaterial(GROUND_MATERIAL, new Color(0.55f, 0.6f, 0.65f));
            EnsureMaterial(GUN_MATERIAL, new Color(0.15f, 0.15f, 0.18f));
            EnsureMaterial(ENEMY_MATERIAL, new Color(0.85f, 0.25f, 0.2f));
            EnsureMaterial(BULLET_MATERIAL, new Color(1f, 0.85f, 0.2f));
            EnsureMaterial(COIN_MATERIAL, new Color(1f, 0.78f, 0.1f));
            EnsureMaterial(FINISH_MATERIAL, new Color(0.25f, 0.75f, 0.35f));
            EnsureMaterial(TARGET_MATERIAL_A, new Color(0.95f, 0.95f, 0.95f));
            EnsureMaterial(TARGET_MATERIAL_B, new Color(0.9f, 0.35f, 0.3f));
            EnsureMaterial(HAZARD_MATERIAL, new Color(0.75f, 0.76f, 0.8f));
            EnsureMaterial(MOVING_MATERIAL, new Color(0.3f, 0.55f, 0.9f));

            if (!File.Exists(BULLET_PREFAB_PATH))
                CreateBulletPrefab(bulletConfig);

            AssetDatabase.SaveAssets();
        }

        static void EnsureWeapons()
        {
            SingleShot singleShot = LoadOrCreate<SingleShot>(SINGLE_SHOT_PATH);

            bool hasHeavyShot = File.Exists(HEAVY_SHOT_PATH);
            SingleShot heavyShot = LoadOrCreate<SingleShot>(HEAVY_SHOT_PATH);
            if (!hasHeavyShot)
                SetFloat(heavyShot, "recoil", HEAVY_SHOT_RECOIL);

            SpreadShot spreadShot = LoadOrCreate<SpreadShot>(SPREAD_SHOT_PATH);
            BurstShot burstShot = LoadOrCreate<BurstShot>(BURST_SHOT_PATH);

            if (File.Exists(WEAPON_CATALOG_PATH))
                return;

            WeaponCatalog weapons = LoadOrCreate<WeaponCatalog>(WEAPON_CATALOG_PATH);
            IReadOnlyList<ShopItem> guns = Load<ShopCatalog>(SHOP_CATALOG_PATH).GetItems(ShopCategory.Guns);

            SerializedObject serialized = new SerializedObject(weapons);
            SerializedProperty entries = serialized.FindProperty("entries");
            entries.arraySize = guns.Count;

            for (int i = 0; i < guns.Count; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("gun").objectReferenceValue = guns[i];
                entry.FindPropertyRelative("strategy").objectReferenceValue = DefaultStrategyFor(guns[i].Id, singleShot, heavyShot, spreadShot, burstShot);
            }

            serialized.FindProperty("fallbackStrategy").objectReferenceValue = singleShot;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static FireStrategy DefaultStrategyFor(string gunID, FireStrategy single, FireStrategy heavy, FireStrategy spread, FireStrategy burst)
        {
            if (Array.IndexOf(SPREAD_GUN_IDS, gunID) >= 0)
                return spread;

            if (Array.IndexOf(BURST_GUN_IDS, gunID) >= 0)
                return burst;

            if (Array.IndexOf(HEAVY_GUN_IDS, gunID) >= 0)
                return heavy;

            return single;
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

            SaveAsPrefab(bulletObject, BULLET_PREFAB_PATH);
        }

        static void BuildGunRigPrefab()
        {
            if (!File.Exists(PROTOTYPE_SCENE_PATH))
                throw new InvalidOperationException($"No existe {PROTOTYPE_SCENE_PATH}: el arma de los niveles se copia de ahí.");

            Scene prototype = SceneManager.GetSceneByPath(PROTOTYPE_SCENE_PATH);
            bool wasOpen = prototype.IsValid() && prototype.isLoaded;
            if (!wasOpen)
                prototype = EditorSceneManager.OpenScene(PROTOTYPE_SCENE_PATH, OpenSceneMode.Additive);

            Gun sourceGun = FindInScene<Gun>(prototype);
            BulletSpawner sourceSpawner = sourceGun != null ? GetReference<BulletSpawner>(sourceGun, "bulletSpawner") : null;

            if (sourceGun == null || sourceSpawner == null)
            {
                if (!wasOpen)
                    EditorSceneManager.CloseScene(prototype, true);

                throw new InvalidOperationException("Prototype no tiene un Gun con su BulletSpawner asignado: no hay arma que copiar.");
            }

            GameObject root = new GameObject("GunRig");

            BulletSpawner spawner = Object.Instantiate(sourceSpawner, root.transform);
            spawner.name = "BulletSpawner";
            spawner.transform.localPosition = Vector3.zero;

            Gun gun = Object.Instantiate(sourceGun, root.transform);
            gun.name = "Gun";
            gun.transform.localPosition = Vector3.zero;

            SetReference(gun, "bulletSpawner", spawner);
            SetReference(gun, "weapons", Load<WeaponCatalog>(WEAPON_CATALOG_PATH));

            if (!wasOpen)
                EditorSceneManager.CloseScene(prototype, true);

            SaveAsPrefab(root, GUN_RIG_PREFAB_PATH);
        }

        static List<string> BuildLevelPrefabs()
        {
            List<string> created = new List<string>();

            if (!File.Exists(ENEMY_PREFAB_PATH))
                created.Add(SaveAsPrefab(CreateEnemy("Enemy", null, Vector3.zero), ENEMY_PREFAB_PATH).name);

            if (!File.Exists(COIN_PREFAB_PATH))
                created.Add(SaveAsPrefab(CreateCoin("Coin", null, Vector3.zero), COIN_PREFAB_PATH).name);

            if (!File.Exists(FINISH_PREFAB_PATH))
                created.Add(SaveAsPrefab(CreateFinish(Vector3.zero), FINISH_PREFAB_PATH).name);

            if (!File.Exists(MULTIPLIER_WALL_PREFAB_PATH))
                created.Add(SaveAsPrefab(CreateMultiplierWall(Vector3.zero), MULTIPLIER_WALL_PREFAB_PATH).name);

            if (!File.Exists(PLATFORM_PREFAB_PATH))
                created.Add(SaveAsPrefab(CreateBlock("Platform", null, Vector3.zero, PLATFORM_SIZE, LoadMaterial(GROUND_MATERIAL)), PLATFORM_PREFAB_PATH).name);

            if (!File.Exists(MOVING_PLATFORM_PREFAB_PATH))
                created.Add(SaveAsPrefab(CreateMovingPlatform(), MOVING_PLATFORM_PREFAB_PATH).name);

            if (!File.Exists(SAW_PREFAB_PATH))
                created.Add(SaveAsPrefab(CreateSaw(), SAW_PREFAB_PATH).name);

            if (!File.Exists(SPIKES_PREFAB_PATH))
                created.Add(SaveAsPrefab(CreateSpikes(), SPIKES_PREFAB_PATH).name);

            if (!File.Exists(TUTORIAL_ZONE_PREFAB_PATH))
                created.Add(SaveAsPrefab(CreateTutorialZone(DEFAULT_ZONE_HINT), TUTORIAL_ZONE_PREFAB_PATH).name);

            return created;
        }

        static void BuildLevelCorePrefab()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject root = new GameObject("LevelCore");
            LevelSettings settings = root.AddComponent<LevelSettings>();
            LevelCore core = root.AddComponent<LevelCore>();

            GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(GUN_RIG_PREFAB_PATH), root.transform);
            Gun gun = rig.GetComponentInChildren<Gun>(true);
            CreateGameplayCamera(root.transform, gun.transform);

            EventSystem eventSystem = UiFactory.CreateEventSystem();
            eventSystem.transform.SetParent(root.transform, false);

            InputReader input = new GameObject("Input").AddComponent<InputReader>();
            input.transform.SetParent(root.transform, false);
            SetReference(input, "eventSystem", eventSystem);

            BuildLevelUi(root.transform, out HudView hud, out PauseView pauseView, out ResultView resultView);

            SetReference(core, "settings", settings);
            SetReference(core, "rules", Load<LevelRules>(LEVEL_RULES_PATH));
            SetReference(core, "economy", Load<EconomyConfig>(ECONOMY_CONFIG_PATH));
            SetReference(core, "levels", Load<LevelListSO>(LEVEL_LIST_PATH));
            SetReference(core, "gun", gun);
            SetReference(core, "input", input);
            SetReference(core, "hud", hud);
            SetReference(core, "pauseView", pauseView);
            SetReference(core, "resultView", resultView);

            GameUiSetupTools.ApplyMenuStyle(root);
            SaveAsPrefab(root, LEVEL_CORE_PREFAB_PATH);
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

            TMP_Text ammo = UiFactory.CreateLabel("Ammo", safeArea, "Ammo 0", 48f, TextAlignmentOptions.TopLeft, HUD_TEXT_COLOR);
            UiFactory.Place(ammo.rectTransform, UiFactory.TOP_LEFT, new Vector2(40f, -40f), labelSize);

            TMP_Text coins = UiFactory.CreateLabel("Coins", safeArea, "Coins 0", 48f, TextAlignmentOptions.TopLeft, HUD_TEXT_COLOR);
            UiFactory.Place(coins.rectTransform, UiFactory.TOP_LEFT, new Vector2(40f, -110f), labelSize);

            TMP_Text enemies = UiFactory.CreateLabel("Enemies", safeArea, "Enemies 0", 48f, TextAlignmentOptions.TopLeft, HUD_TEXT_COLOR);
            UiFactory.Place(enemies.rectTransform, UiFactory.TOP_LEFT, new Vector2(40f, -180f), labelSize);

            CommandButton pauseButton = UiFactory.CreateButton("PauseButton", safeArea, "II", 56f);
            UiFactory.Place((RectTransform)pauseButton.transform, UiFactory.TOP_RIGHT, new Vector2(-40f, -40f), new Vector2(130f, 130f));

            SetReference(hud, "ammoLabel", ammo);
            SetReference(hud, "coinsLabel", coins);
            SetReference(hud, "enemiesLabel", enemies);
            SetReference(hud, "pauseButton", pauseButton);

            BuildHintPanel(safeArea, hud);
        }

        static void BuildHintPanel(RectTransform safeArea, HudView hud)
        {
            Image panel = UiFactory.CreateImage("HintPanel", safeArea, HINT_PANEL_COLOR, false);
            panel.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(PANEL_SPRITE_NAME);
            panel.type = Image.Type.Sliced;
            UiFactory.Place(panel.rectTransform, BOTTOM_CENTER, new Vector2(0f, HINT_PANEL_BOTTOM), HINT_PANEL_SIZE);

            TMP_Text label = UiFactory.CreateLabel("Label", panel.transform, string.Empty, HINT_FONT_SIZE, TextAlignmentOptions.Center, Color.white);
            label.textWrappingMode = TextWrappingModes.Normal;
            UiFactory.Stretch(label.rectTransform);
            label.rectTransform.offsetMin = HINT_PADDING;
            label.rectTransform.offsetMax = -HINT_PADDING;

            Image ring = UiFactory.CreateImage("TapIndicator", safeArea, HINT_RING_COLOR, false);
            ring.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(KNOB_SPRITE_NAME);
            UiFactory.Place(ring.rectTransform, UiFactory.CENTER, HINT_RING_POSITION, HINT_RING_SIZE);
            ring.gameObject.AddComponent<TapPulse>();

            SetReference(hud, "hintPanel", panel.gameObject);
            SetReference(hud, "hintLabel", label);
            SetReference(hud, "tapIndicator", ring.gameObject);
            panel.gameObject.SetActive(false);
            ring.gameObject.SetActive(false);
        }

        static bool AddHintPanelToLevelCore()
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(LEVEL_CORE_PREFAB_PATH);
            HudView hud = contents.GetComponentInChildren<HudView>(true);
            Transform safeArea = hud != null ? hud.transform.Find(SAFE_AREA_NAME) : null;

            if (safeArea == null)
            {
                PrefabUtility.UnloadPrefabContents(contents);
                throw new InvalidOperationException($"{LEVEL_CORE_PREFAB_PATH} no tiene un HudView con su hijo {SAFE_AREA_NAME}: no se sabe dónde poner el cartel de ayuda.");
            }

            bool isMissing = GetReference<GameObject>(hud, "hintPanel") == null;
            if (isMissing)
            {
                BuildHintPanel((RectTransform)safeArea, hud);
                PrefabUtility.SaveAsPrefabAsset(contents, LEVEL_CORE_PREFAB_PATH);
            }

            PrefabUtility.UnloadPrefabContents(contents);
            return isMissing;
        }

        static void BuildPausePanel(Transform canvas, PauseView pauseView)
        {
            GameObject panel = UiFactory.CreateOverlay("PausePanel", canvas);

            TMP_Text title = UiFactory.CreateLabel("Title", panel.transform, "Paused", 96f, TextAlignmentOptions.Center, Color.white);
            UiFactory.Place(title.rectTransform, UiFactory.CENTER, new Vector2(0f, 260f), new Vector2(900f, 160f));

            CommandButton resume = UiFactory.CreateButton("ResumeButton", panel.transform, "Resume", 52f);
            UiFactory.Place((RectTransform)resume.transform, UiFactory.CENTER, new Vector2(0f, 40f), BUTTON_SIZE);

            CommandButton retry = UiFactory.CreateButton("RetryButton", panel.transform, "Retry", 52f);
            UiFactory.Place((RectTransform)retry.transform, UiFactory.CENTER, new Vector2(0f, -150f), BUTTON_SIZE);

            CommandButton menu = UiFactory.CreateButton("MenuButton", panel.transform, "Menu", 52f);
            UiFactory.Place((RectTransform)menu.transform, UiFactory.CENTER, new Vector2(0f, -340f), BUTTON_SIZE);

            SetReference(pauseView, "panel", panel);
            SetReference(pauseView, "resumeButton", resume);
            SetReference(pauseView, "retryButton", retry);
            SetReference(pauseView, "menuButton", menu);
            panel.SetActive(false);
        }

        static void BuildResultPanel(Transform canvas, ResultView resultView)
        {
            GameObject panel = UiFactory.CreateOverlay("ResultPanel", canvas);

            TMP_Text title = UiFactory.CreateLabel("Title", panel.transform, "Level complete!", 80f, TextAlignmentOptions.Center, Color.white);
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

            TMP_Text coins = UiFactory.CreateLabel("Coins", panel.transform, "+0 coins", 64f, TextAlignmentOptions.Center, Color.white);
            UiFactory.Place(coins.rectTransform, UiFactory.CENTER, new Vector2(0f, 80f), new Vector2(900f, 100f));

            TMP_Text multiplier = UiFactory.CreateLabel("Multiplier", panel.transform, "Multiplier x1", 44f, TextAlignmentOptions.Center, Color.white);
            UiFactory.Place(multiplier.rectTransform, UiFactory.CENTER, new Vector2(0f, -10f), new Vector2(900f, 80f));

            TMP_Text score = UiFactory.CreateLabel("Score", panel.transform, "Score 0", 44f, TextAlignmentOptions.Center, Color.white);
            UiFactory.Place(score.rectTransform, UiFactory.CENTER, new Vector2(0f, -90f), new Vector2(900f, 80f));

            CommandButton next = UiFactory.CreateButton("NextButton", panel.transform, "Next level", 52f);
            UiFactory.Place((RectTransform)next.transform, UiFactory.CENTER, new Vector2(0f, -240f), BUTTON_SIZE);

            CommandButton retry = UiFactory.CreateButton("RetryButton", panel.transform, "Retry", 52f);
            UiFactory.Place((RectTransform)retry.transform, UiFactory.CENTER, new Vector2(0f, -420f), BUTTON_SIZE);

            CommandButton menu = UiFactory.CreateButton("MenuButton", panel.transform, "Menu", 52f);
            UiFactory.Place((RectTransform)menu.transform, UiFactory.CENTER, new Vector2(-155f, -600f), HALF_BUTTON_SIZE);

            CommandButton shop = UiFactory.CreateButton("ShopButton", panel.transform, "Shop", 52f);
            UiFactory.Place((RectTransform)shop.transform, UiFactory.CENTER, new Vector2(155f, -600f), HALF_BUTTON_SIZE);

            SetReference(resultView, "panel", panel);
            SetReference(resultView, "titleLabel", title);
            SetReference(resultView, "coinsLabel", coins);
            SetReference(resultView, "multiplierLabel", multiplier);
            SetReference(resultView, "scoreLabel", score);
            SetReference(resultView, "starsRow", starsRow.gameObject);
            SetReferences(resultView, "stars", stars);
            SetReference(resultView, "nextButton", next);
            SetReference(resultView, "retryButton", retry);
            SetReference(resultView, "menuButton", menu);
            SetReference(resultView, "shopButton", shop);
            panel.SetActive(false);
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

            TMP_Text label = UiFactory.CreateLabel("Label", canvas.transform, "Loading", 64f, TextAlignmentOptions.Center, Color.white);
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
            core.transform.position = LEVEL_CORE_POSITION;

            Transform geometry = new GameObject("Geometry").transform;
            Material ground = LoadMaterial(GROUND_MATERIAL);
            CreateBlock("Ground_A", geometry, new Vector3(5f, -1f, 0f), new Vector3(22f, 2f, DEPTH), ground);
            CreateBlock("Ground_B", geometry, new Vector3(30f, -1f, 0f), new Vector3(22f, 2f, DEPTH), ground);
            CreateBlock("Step", geometry, new Vector3(33f, 0.75f, 0f), new Vector3(6f, 1.5f, DEPTH), ground);
            CreateBlock("Ground_C", geometry, new Vector3(54.75f, -1f, 0f), new Vector3(20.5f, 2f, DEPTH), ground);

            Transform enemies = new GameObject("Enemies").transform;
            PlacePrefab(ENEMY_PREFAB_PATH, "Enemy_1", enemies, new Vector3(9f, 1f, 0f));
            PlacePrefab(ENEMY_PREFAB_PATH, "Enemy_2", enemies, new Vector3(24f, 1f, 0f));
            PlacePrefab(ENEMY_PREFAB_PATH, "Enemy_3", enemies, new Vector3(33f, 2.5f, 0f));
            PlacePrefab(ENEMY_PREFAB_PATH, "Enemy_4", enemies, new Vector3(50f, 1f, 0f));

            Transform coins = new GameObject("Coins").transform;
            float[] coinX = { 4f, 13f, 16.5f, 17.5f, 18.5f, 27f, 42f, 42.75f, 43.5f };
            float[] coinY = { 2f, 2.5f, 2.5f, 3f, 2.5f, 2.5f, 2.5f, 3f, 2.5f };
            for (int i = 0; i < coinX.Length; i++)
                PlacePrefab(COIN_PREFAB_PATH, "Coin_" + (i + 1), coins, new Vector3(coinX[i], coinY[i], 0f));

            PlacePrefab(FINISH_PREFAB_PATH, "Finish", null, FINISH_POSITION);
            PlacePrefab(MULTIPLIER_WALL_PREFAB_PATH, "MultiplierWall", null, MULTIPLIER_WALL_POSITION);

            AddTutorial(scene);
            SaveScene(scene, LEVEL1_SCENE_PATH);
        }

        static void AddTutorial(Scene scene)
        {
            GameObject oldTutorial = FindRoot(scene, TUTORIAL_GROUP_NAME);
            if (oldTutorial != null)
                Object.DestroyImmediate(oldTutorial);

            LevelCore core = FindInScene<LevelCore>(scene);
            if (core == null)
                throw new InvalidOperationException($"{scene.name} no tiene LevelCore: no se puede armar el tutorial.");

            LevelSettings settings = core.GetComponent<LevelSettings>();
            SetFloat(settings, "startTimeScale", LEVEL1_START_TIME_SCALE);
            SetString(settings, "startHint", LEVEL1_START_HINT);

            GameObject tutorial = new GameObject(TUTORIAL_GROUP_NAME);
            SceneManager.MoveGameObjectToScene(tutorial, scene);

            for (int i = 0; i < TUTORIAL_ZONE_X.Length; i++)
            {
                GameObject zone = PlacePrefab(TUTORIAL_ZONE_PREFAB_PATH, "Hint_" + (i + 1), tutorial.transform, new Vector3(TUTORIAL_ZONE_X[i], 0f, 0f));
                SetString(zone.GetComponent<TutorialZone>(), "hint", TUTORIAL_HINTS[i]);
            }
        }

        static void BuildLevelTemplate(string path, int ammo, float targetTime)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Light light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject core = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(LEVEL_CORE_PREFAB_PATH), scene);
            core.transform.position = LEVEL_CORE_POSITION;

            LevelSettings settings = core.GetComponent<LevelSettings>();
            SetInt(settings, "ammo", ammo);
            SetFloat(settings, "targetTime", targetTime);

            Transform geometry = new GameObject("Geometry").transform;
            Material ground = LoadMaterial(GROUND_MATERIAL);
            CreateBlock("Ground_Start", geometry, TEMPLATE_START_GROUND_CENTER, TEMPLATE_GROUND_SIZE, ground);
            CreateBlock("Ground_Finish", geometry, TEMPLATE_FINISH_GROUND_CENTER, TEMPLATE_GROUND_SIZE, ground);

            new GameObject("Platforms");
            new GameObject("Enemies");
            new GameObject("Coins");
            new GameObject("Hazards");

            PlacePrefab(FINISH_PREFAB_PATH, "Finish", null, FINISH_POSITION);
            PlacePrefab(MULTIPLIER_WALL_PREFAB_PATH, "MultiplierWall", null, MULTIPLIER_WALL_POSITION);

            SaveScene(scene, path);
        }

        static void RegisterLevels()
        {
            List<string> levels = new List<string>();
            foreach (string path in LEVEL_SCENE_PATHS)
            {
                if (!File.Exists(path))
                    continue;

                AppendToBuildSettings(path);
                levels.Add(path);
            }

            PointLevelListAt(levels.ToArray());
        }

        static GameObject PlacePrefab(string prefabPath, string name, Transform parent, Vector3 position)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(prefabPath));
            instance.name = name;

            if (parent != null)
                instance.transform.SetParent(parent, false);

            instance.transform.position = position;
            return instance;
        }

        static GameObject CreateBlock(string name, Transform parent, Vector3 center, Vector3 size, Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.position = center;
            block.transform.localScale = size;
            block.GetComponent<MeshRenderer>().sharedMaterial = material;
            GameObjectUtility.SetStaticEditorFlags(block, StaticEditorFlags.BatchingStatic);
            return block;
        }

        static GameObject CreateEnemy(string name, Transform parent, Vector3 position)
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
            return enemy;
        }

        static GameObject CreateCoin(string name, Transform parent, Vector3 position)
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
            return coin;
        }

        static GameObject CreateFinish(Vector3 groundPosition)
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
            return finish;
        }

        static GameObject CreateMultiplierWall(Vector3 groundPosition)
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

            return wall;
        }

        static GameObject CreateMovingPlatform()
        {
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "MovingPlatform";
            platform.transform.localScale = MOVING_PLATFORM_SIZE;
            platform.GetComponent<MeshRenderer>().sharedMaterial = LoadMaterial(MOVING_MATERIAL);

            Rigidbody body = platform.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            platform.AddComponent<MovingPlatform>();
            return platform;
        }

        static GameObject CreateSaw()
        {
            GameObject saw = new GameObject("Saw");
            SphereCollider trigger = saw.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = SAW_RADIUS;
            saw.AddComponent<HazardZone>();

            GameObject blade = new GameObject("Blade");
            blade.transform.SetParent(saw.transform, false);

            GameObject disk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disk.name = "Disk";
            Object.DestroyImmediate(disk.GetComponent<Collider>());
            disk.transform.SetParent(blade.transform, false);
            disk.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            disk.transform.localScale = new Vector3(SAW_RADIUS * 2f, 0.05f, SAW_RADIUS * 2f);
            disk.GetComponent<MeshRenderer>().sharedMaterial = LoadMaterial(GUN_MATERIAL);

            Material teethMaterial = LoadMaterial(HAZARD_MATERIAL);
            for (int i = 0; i < SAW_TEETH; i++)
            {
                GameObject tooth = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tooth.name = "Teeth_" + (i + 1);
                Object.DestroyImmediate(tooth.GetComponent<Collider>());
                tooth.transform.SetParent(blade.transform, false);
                tooth.transform.localRotation = Quaternion.Euler(0f, 0f, i * 180f / SAW_TEETH);
                tooth.transform.localScale = new Vector3(SAW_RADIUS * 2.3f, 0.25f, 0.15f);
                tooth.GetComponent<MeshRenderer>().sharedMaterial = teethMaterial;
            }

            SawBlade spinner = saw.AddComponent<SawBlade>();
            SetReference(spinner, "blade", blade.transform);
            return saw;
        }

        static GameObject CreateSpikes()
        {
            GameObject spikes = new GameObject("Spikes");
            BoxCollider trigger = spikes.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(SPIKES_WIDTH, SPIKE_HEIGHT, DEPTH);
            trigger.center = new Vector3(0f, SPIKE_HEIGHT * 0.5f, 0f);
            spikes.AddComponent<HazardZone>();

            Material material = LoadMaterial(HAZARD_MATERIAL);
            float spacing = SPIKES_WIDTH / SPIKE_COUNT;
            float side = SPIKE_HEIGHT * Mathf.Sqrt(2f);

            for (int i = 0; i < SPIKE_COUNT; i++)
            {
                GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spike.name = "Spike_" + (i + 1);
                Object.DestroyImmediate(spike.GetComponent<Collider>());
                spike.transform.SetParent(spikes.transform, false);
                spike.transform.localPosition = new Vector3((i - (SPIKE_COUNT - 1) * 0.5f) * spacing, 0f, 0f);
                spike.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                spike.transform.localScale = new Vector3(side, side, DEPTH);
                spike.GetComponent<MeshRenderer>().sharedMaterial = material;
                GameObjectUtility.SetStaticEditorFlags(spike, StaticEditorFlags.BatchingStatic);
            }

            return spikes;
        }

        static GameObject CreateTutorialZone(string hint)
        {
            GameObject zone = new GameObject("TutorialZone");
            BoxCollider trigger = zone.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = TUTORIAL_ZONE_SIZE;
            trigger.center = TUTORIAL_ZONE_CENTER;

            TutorialZone tutorial = zone.AddComponent<TutorialZone>();
            SetString(tutorial, "hint", hint);
            return zone;
        }

        static void SaveScene(Scene scene, string path)
        {
            EnsureFolder(FolderOf(path));
            EditorSceneManager.SaveScene(scene, path);
        }

        static GameObject SaveAsPrefab(GameObject root, string path)
        {
            EnsureFolder(FolderOf(path));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
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

        static void PointLevelListAt(params string[] levelScenePaths)
        {
            List<int> buildIndices = new List<int>();
            foreach (string path in levelScenePaths)
            {
                int buildIndex = BuildIndexOf(path);
                if (buildIndex < 0)
                    throw new InvalidOperationException($"{path} no está habilitada en Build Settings: no se puede poner en LevelListSO.");

                buildIndices.Add(buildIndex);
            }

            LevelListSO levelList = Load<LevelListSO>(LEVEL_LIST_PATH);
            levelList.levelBuildIDs = buildIndices;
            EditorUtility.SetDirty(levelList);
        }

        static int BuildIndexOf(string path)
        {
            int buildIndex = 0;
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                    continue;

                if (scene.path == path)
                    return buildIndex;

                buildIndex++;
            }

            return -1;
        }

        internal static GameObject FindRoot(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == name)
                    return root;
            }

            return null;
        }

        static ComponentType FindInScene<ComponentType>(Scene scene) where ComponentType : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                ComponentType found = root.GetComponentInChildren<ComponentType>(true);
                if (found != null)
                    return found;
            }

            return null;
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

        internal static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            string parent = FolderOf(folder);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        static ComponentType GetReference<ComponentType>(Object target, string field) where ComponentType : Object
        {
            SerializedProperty property = new SerializedObject(target).FindProperty(field);
            return property != null ? property.objectReferenceValue as ComponentType : null;
        }

        static void SetFloat(Object target, string field, float value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
                throw new InvalidOperationException($"{target.GetType().Name} no tiene el campo serializado '{field}'.");

            property.floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetInt(Object target, string field, int value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
                throw new InvalidOperationException($"{target.GetType().Name} no tiene el campo serializado '{field}'.");

            property.intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetString(Object target, string field, string value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
                throw new InvalidOperationException($"{target.GetType().Name} no tiene el campo serializado '{field}'.");

            property.stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
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

        internal static void SetReferences(Object target, string field, Object[] values)
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

        struct LevelGroups
        {
            public Transform geometry;
            public Transform platforms;
            public Transform enemies;
            public Transform coins;
            public Transform hazards;
        }
    }
}
