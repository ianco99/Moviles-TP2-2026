using System;
using System.Collections.Generic;
using System.IO;
using GunJump.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GunJump.EditorTools
{
    // Arma la escena del prototipo de la semana 1 con primitivas, para no cablearla a mano.
    // Los assets de datos que ya existen se respetan: se puede volver a correr sin perder el tuning.
    public static class PrototypeSceneBuilder
    {
        const string Root = "Assets/_Game";
        const string ScenePath = Root + "/Scenes/Prototype.unity";
        const string BulletPrefabPath = Root + "/Prefabs/Bullets/Bullet.prefab";
        const float Depth = 4f;

        [MenuItem("GunJump/Build Prototype Scene")]
        public static void Build()
        {
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog(
                    "GunJump",
                    "La escena Prototype ya existe. ¿Reemplazarla?",
                    "Reemplazar",
                    "Cancelar"))
                return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var gunTuning = LoadOrCreate<GunTuning>(Root + "/Data/Tuning/GunTuning.asset");
            var cameraTuning = LoadOrCreate<CameraTuning>(Root + "/Data/Tuning/CameraTuning.asset");
            var bulletConfig = LoadOrCreate<BulletConfig>(Root + "/Data/Weapons/PistolBullet.asset");
            var singleShot = LoadOrCreate<SingleShot>(Root + "/Data/Weapons/SingleShot.asset");
            var enemyConfig = LoadOrCreate<EnemyConfig>(Root + "/Data/Enemies/StaticEnemy.asset");

            Material groundMaterial = LoadOrCreateMaterial("Proto_Ground", new Color(0.55f, 0.6f, 0.65f));
            Material gunMaterial = LoadOrCreateMaterial("Proto_Gun", new Color(0.15f, 0.15f, 0.18f));
            Material enemyMaterial = LoadOrCreateMaterial("Proto_Enemy", new Color(0.85f, 0.25f, 0.2f));
            Material bulletMaterial = LoadOrCreateMaterial("Proto_Bullet", new Color(1f, 0.85f, 0.2f));

            CreateBulletPrefab(bulletConfig, bulletMaterial);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // El prefab se carga recién acá: la referencia que devuelve SaveAsPrefabAsset
            // no sobrevive al cambio de escena y quedaba sin asignar en el spawner.
            var bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BulletPrefabPath).GetComponent<Bullet>();

            CreateLight();
            CreateBlock("Ground_A", new Vector3(5f, -1f, 0f), new Vector3(20f, 2f, Depth), groundMaterial);
            CreateBlock("Ground_B", new Vector3(31f, -1f, 0f), new Vector3(24f, 2f, Depth), groundMaterial);
            CreateBlock("Step", new Vector3(38f, 0.75f, 0f), new Vector3(6f, 1.5f, Depth), groundMaterial);

            CreateEnemy("Enemy_1", new Vector3(10f, 1f, 0f), enemyConfig, enemyMaterial);
            CreateEnemy("Enemy_2", new Vector3(26f, 1f, 0f), enemyConfig, enemyMaterial);
            CreateEnemy("Enemy_3", new Vector3(38f, 2.5f, 0f), enemyConfig, enemyMaterial);

            var spawner = new GameObject("BulletSpawner").AddComponent<BulletSpawner>();
            SetReference(spawner, "prefab", bulletPrefab);

            Gun gun = CreateGun(gunTuning, singleShot, spawner, gunMaterial);
            CreateCamera(cameraTuning, gun.transform);
            CreateSystems(gun);

            EnsureFolder(Root + "/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuild();
            AssetDatabase.SaveAssets();

            Debug.Log("GunJump: escena Prototype creada en " + ScenePath);
        }

        static void CreateLight()
        {
            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        static void CreateBlock(string name, Vector3 center, Vector3 size, Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.position = center;
            block.transform.localScale = size;
            block.GetComponent<MeshRenderer>().sharedMaterial = material;
            GameObjectUtility.SetStaticEditorFlags(block, StaticEditorFlags.BatchingStatic);
        }

        static void CreateEnemy(string name, Vector3 position, EnemyConfig config, Material material)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = name;
            enemy.transform.position = position;
            enemy.GetComponent<MeshRenderer>().sharedMaterial = material;

            var body = enemy.AddComponent<Rigidbody>();
            body.isKinematic = true;

            var staticEnemy = enemy.AddComponent<StaticEnemy>();
            SetReference(staticEnemy, "config", config);
            SetReference(staticEnemy, "body", body);
        }

        static Gun CreateGun(GunTuning tuning, FireStrategy strategy, BulletSpawner spawner, Material material)
        {
            var root = new GameObject("Gun");
            root.transform.position = new Vector3(0f, 3f, 0f);

            var body = root.AddComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 0.35f, 0.25f);

            CreateGunPart("Barrel", root.transform, Vector3.zero, new Vector3(1f, 0.35f, 0.25f), material);
            CreateGunPart("Grip", root.transform, new Vector3(-0.3f, -0.35f, 0f), new Vector3(0.25f, 0.45f, 0.2f), material);

            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(root.transform, false);
            muzzle.transform.localPosition = new Vector3(0.6f, 0f, 0f);

            var gun = root.AddComponent<Gun>();
            SetReference(gun, "tuning", tuning);
            SetReference(gun, "muzzle", muzzle.transform);
            SetReference(gun, "fireStrategy", strategy);
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

        static void CreateCamera(CameraTuning tuning, Transform target)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(target.position.x, target.position.y, -tuning.Distance);

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.62f, 0.8f, 0.92f);
            cameraObject.AddComponent<AudioListener>();

            var follow = cameraObject.AddComponent<FollowCamera>();
            SetReference(follow, "tuning", tuning);
            SetReference(follow, "target", target);
        }

        static void CreateSystems(Gun gun)
        {
            var systems = new GameObject("Systems");
            var input = systems.AddComponent<InputReader>();

            var installer = systems.AddComponent<GameplayInstaller>();
            SetReference(installer, "gun", gun);
            SetReference(installer, "input", input);

            var loop = systems.AddComponent<PrototypeLoop>();
            SetReference(loop, "gun", gun.transform);
        }

        static void CreateBulletPrefab(BulletConfig config, Material material)
        {
            GameObject bulletObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bulletObject.name = "Bullet";
            Object.DestroyImmediate(bulletObject.GetComponent<Collider>());
            bulletObject.transform.localScale = new Vector3(0.35f, 0.1f, 0.1f);
            bulletObject.GetComponent<MeshRenderer>().sharedMaterial = material;

            var bullet = bulletObject.AddComponent<Bullet>();
            SetReference(bullet, "config", config);

            EnsureFolder(Path.GetDirectoryName(BulletPrefabPath).Replace('\\', '/'));
            PrefabUtility.SaveAsPrefabAsset(bulletObject, BulletPrefabPath);
            Object.DestroyImmediate(bulletObject);
        }

        static void AddSceneToBuild()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(scene => scene.path == ScenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static Material LoadOrCreateMaterial(string name, Color color)
        {
            string path = Root + "/Art/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
            material = pipeline != null && pipeline.defaultMaterial != null
                ? new Material(pipeline.defaultMaterial)
                : new Material(Shader.Find("Standard"));
            material.color = color;

            EnsureFolder(Root + "/Art/Materials");
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        static void SetReference(Object target, string field, Object value)
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
    }
}
