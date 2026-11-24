using System;
using System.Collections.Generic;
using System.IO;
using GunJump.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GunJump.EditorTools
{
    public static class LevelArtTools
    {
        private const string ROOT = "Assets/_Game";
        private const string MODELS_FOLDER = ROOT + "/Art/Models/Level";
        private const string LEVEL_MATERIALS_FOLDER = ROOT + "/Art/Materials/Level";
        private const string COLORMAP_PATH = ROOT + "/Art/Textures/Level/colormap.png";
        private const string COLORMAP_SOURCE = "kenney_platformer-kit/Models/FBX format/Textures/colormap.png";
        private const string LEVEL_KIT_MATERIAL_PATH = LEVEL_MATERIALS_FOLDER + "/LevelKit.mat";
        private const string COIN_GLOW_MATERIAL_PATH = LEVEL_MATERIALS_FOLDER + "/CoinGlow.mat";

        private const string PLATFORM_PREFAB_PATH = ROOT + "/Prefabs/Level/Platform.prefab";
        private const string MOVING_PLATFORM_PREFAB_PATH = ROOT + "/Prefabs/Level/MovingPlatform.prefab";
        private const string SPIKES_PREFAB_PATH = ROOT + "/Prefabs/Level/Spikes.prefab";
        private const string SAW_PREFAB_PATH = ROOT + "/Prefabs/Level/Saw.prefab";
        private const string COIN_PREFAB_PATH = ROOT + "/Prefabs/Level/Coin.prefab";

        private const string BLOCK_SNOW_LOW_LARGE_PATH = MODELS_FOLDER + "/block-snow-low-large.fbx";
        private const string BLOCK_SNOW_LARGE_PATH = MODELS_FOLDER + "/block-snow-large.fbx";
        private const string BLOCK_SNOW_LARGE_TALL_PATH = MODELS_FOLDER + "/block-snow-large-tall.fbx";
        private const string BLOCK_GRASS_LARGE_PATH = MODELS_FOLDER + "/block-grass-large.fbx";
        private const string TRAP_SPIKES_PATH = MODELS_FOLDER + "/trap-spikes.fbx";
        private const string TRAP_SPIKES_LARGE_PATH = MODELS_FOLDER + "/trap-spikes-large.fbx";
        private const string COIN_GOLD_PATH = MODELS_FOLDER + "/coin-gold.fbx";

        private const string LIT_SHADER_NAME = "Universal Render Pipeline/Lit";
        private const string BASE_MAP_PROPERTY = "_BaseMap";
        private const string BASE_COLOR_PROPERTY = "_BaseColor";
        private const string SMOOTHNESS_PROPERTY = "_Smoothness";
        private const string EMISSION_MAP_PROPERTY = "_EmissionMap";
        private const string EMISSION_COLOR_PROPERTY = "_EmissionColor";
        private const string EMISSION_KEYWORD = "_EMISSION";

        private const string GEOMETRY_GROUP_NAME = "Geometry";
        private const string TILES_NAME = "Tiles";
        private const string TILE_PREFIX = "Tile_";
        private const string RING_NAME = "Ring";
        private const string PLATE_PREFIX = "Plate_";
        private const string VISUAL_NAME = "Visual";
        private const string BLADE_NAME = "Blade";
        private const string HUB_NAME = "Disk";
        private const string SPIKE_PREFIX = "Spike_";
        private const string TEETH_PREFIX = "Teeth_";

        private const int SAW_PLATE_COUNT = 8;
        private const float SAW_PLATE_SCALE = 1f;
        private const float SPIKES_FOOTPRINT = 1f;
        private const float COIN_MODEL_SCALE = 1.6f;
        private const float COIN_GOLD_HEIGHT = 0.4f;
        private const float CYLINDER_HEIGHT = 2f;

        private static readonly Vector3 TRAP_SPIKES_SIZE = new Vector3(0.64f, 0.255f, 0.64f);
        private static readonly Vector3 TRAP_SPIKES_LARGE_SIZE = new Vector3(0.765f, 0.255f, 0.765f);
        private static readonly Color COIN_GLOW_COLOR = new Color(1.5f, 1.2f, 0.6f);

        private static readonly TileModel[] SNOW_TILES =
        {
            new TileModel(BLOCK_SNOW_LOW_LARGE_PATH, new Vector3(2f, 0.5f, 2f), new Vector3(2f, 0.5f, 2f)),
            new TileModel(BLOCK_SNOW_LARGE_PATH, new Vector3(2f, 1f, 2f), new Vector3(2f, 1f, 2f)),
            new TileModel(BLOCK_SNOW_LARGE_TALL_PATH, new Vector3(2f, 2f, 2f), new Vector3(2f, 2f, 2f))
        };

        private static readonly TileModel GRASS_TILE = new TileModel(BLOCK_GRASS_LARGE_PATH, new Vector3(2f, 1f, 2f), new Vector3(2f, 1f, 2f));
        private static readonly TileModel SPIKES_TILE = new TileModel(TRAP_SPIKES_LARGE_PATH, TRAP_SPIKES_LARGE_SIZE, Vector3.one * SPIKES_FOOTPRINT);

        [MenuItem("GunJump/Art/Apply Level Models")]
        private static void ApplyLevelModels()
        {
            Texture2D colormap = AssetDatabase.LoadAssetAtPath<Texture2D>(COLORMAP_PATH);
            if (colormap == null)
            {
                EditorUtility.DisplayDialog(
                    "GunJump",
                    $"Falta {COLORMAP_PATH}. Copiá ahí el de {COLORMAP_SOURCE}; el de Blasters es otra paleta y la nieve sale de otro color.",
                    "Entendido");
                return;
            }

            Shader litShader = Shader.Find(LIT_SHADER_NAME);
            if (litShader == null)
            {
                EditorUtility.DisplayDialog("GunJump", $"No se encontró el shader '{LIT_SHADER_NAME}'.", "Entendido");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string previousScenePath = SceneManager.GetActiveScene().path;

            GameSceneBuilder.EnsureFolder(LEVEL_MATERIALS_FOLDER);
            Material kit = EnsureMaterial(LEVEL_KIT_MATERIAL_PATH, colormap, litShader, Color.black);
            Material glow = EnsureMaterial(COIN_GLOW_MATERIAL_PATH, colormap, litShader, COIN_GLOW_COLOR);
            AssetDatabase.SaveAssets();

            List<string> dressedPrefabs = new List<string>();
            List<string> skipped = new List<string>();

            EditPrefab(PLATFORM_PREFAB_PATH, platform => DressPlatform(platform, kit), dressedPrefabs, skipped);
            EditPrefab(MOVING_PLATFORM_PREFAB_PATH, movingPlatform => DressMovingPlatform(movingPlatform, kit), dressedPrefabs, skipped);
            EditPrefab(SPIKES_PREFAB_PATH, spikes => DressSpikes(spikes, kit), dressedPrefabs, skipped);
            EditPrefab(SAW_PREFAB_PATH, saw => DressSaw(saw, kit), dressedPrefabs, skipped);
            EditPrefab(COIN_PREFAB_PATH, coin => DressCoin(coin, glow), dressedPrefabs, skipped);

            List<string> dressedScenes = DressLevelScenes(kit, skipped);

            AssetDatabase.SaveAssets();

            if (!string.IsNullOrEmpty(previousScenePath) && File.Exists(previousScenePath))
                EditorSceneManager.OpenScene(previousScenePath);

            Debug.Log($"GunJump: modelos del kit aplicados. Prefabs: {NamesOrNone(dressedPrefabs)}. " +
                      $"Escenas: {NamesOrNone(dressedScenes)}. Sin tocar: {NamesOrNone(skipped)}.");
        }

        private static void EditPrefab(string path, Action<GameObject> dress, List<string> dressed, List<string> skipped)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                skipped.Add(Path.GetFileName(path) + " (no existe)");
                return;
            }

            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                dress(contents);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
                dressed.Add(Path.GetFileNameWithoutExtension(path));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void DressPlatform(GameObject platform, Material kit)
        {
            BoxCollider box = GetRequired<BoxCollider>(platform, PLATFORM_PREFAB_PATH);
            HidePrimitive(platform);
            FillWithSnow(platform.transform, box, kit);
        }

        private static void DressMovingPlatform(GameObject movingPlatform, Material kit)
        {
            BoxCollider box = GetRequired<BoxCollider>(movingPlatform, MOVING_PLATFORM_PREFAB_PATH);
            HidePrimitive(movingPlatform);
            FillBox(movingPlatform.transform, box, GRASS_TILE, kit, false);
        }

        private static void DressSpikes(GameObject spikes, Material kit)
        {
            BoxCollider box = GetRequired<BoxCollider>(spikes, SPIKES_PREFAB_PATH);
            HideChildPrimitives(spikes.transform, SPIKE_PREFIX);
            FillBox(spikes.transform, box, SPIKES_TILE, kit, true);
        }

        private static void DressSaw(GameObject saw, Material kit)
        {
            SphereCollider hitArea = GetRequired<SphereCollider>(saw, SAW_PREFAB_PATH);
            Transform blade = saw.transform.Find(BLADE_NAME);
            if (blade == null)
                throw new InvalidOperationException($"{SAW_PREFAB_PATH} no tiene el hijo '{BLADE_NAME}' que gira: no se sabe dónde poner la rueda.");

            HideChildPrimitives(blade, TEETH_PREFIX);
            DestroyChild(blade, RING_NAME);

            Transform ring = CreateChild(RING_NAME, blade);
            float plateHeight = TRAP_SPIKES_SIZE.y * SAW_PLATE_SCALE;
            float plateDepth = TRAP_SPIKES_SIZE.z * SAW_PLATE_SCALE;
            float innerRadius = hitArea.radius - plateHeight;
            GameObject plateModel = LoadModel(TRAP_SPIKES_PATH);

            for (int i = 0; i < SAW_PLATE_COUNT; i++)
            {
                Quaternion outward = Quaternion.Euler(0f, 0f, i * 360f / SAW_PLATE_COUNT);
                Vector3 position = outward * (Vector3.up * innerRadius);
                PlaceModel(plateModel, ring, PLATE_PREFIX + (i + 1), position, outward, Vector3.one * SAW_PLATE_SCALE, kit);
            }

            Transform hub = blade.Find(HUB_NAME);
            if (hub != null)
                hub.localScale = new Vector3(innerRadius * 2f, plateDepth / CYLINDER_HEIGHT, innerRadius * 2f);
        }

        private static void DressCoin(GameObject coin, Material glow)
        {
            HidePrimitive(coin);
            DestroyChild(coin.transform, VISUAL_NAME);

            Transform visual = CreateChild(VISUAL_NAME, coin.transform);
            visual.localRotation = Quaternion.Inverse(coin.transform.localRotation);
            visual.localScale = Inverse(coin.transform.localScale);

            GameObject coinModel = LoadModel(COIN_GOLD_PATH);
            Vector3 centered = Vector3.down * (COIN_GOLD_HEIGHT * COIN_MODEL_SCALE * 0.5f);
            GameObject model = PlaceModel(coinModel, visual, coinModel.name, centered, Quaternion.identity, Vector3.one * COIN_MODEL_SCALE, glow);

            CoinBob bob = coin.GetComponent<CoinBob>();
            if (bob == null)
                bob = coin.AddComponent<CoinBob>();

            GameSceneBuilder.SetReference(bob, "visual", model.transform);
        }

        private static List<string> DressLevelScenes(Material kit, List<string> skipped)
        {
            List<string> dressed = new List<string>();

            foreach (string path in GameSceneBuilder.LEVEL_SCENE_PATHS)
            {
                string levelName = Path.GetFileNameWithoutExtension(path);
                if (!File.Exists(path))
                {
                    skipped.Add(levelName + " (no existe)");
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                GameObject geometry = GameSceneBuilder.FindRoot(scene, GEOMETRY_GROUP_NAME);
                if (geometry == null)
                {
                    skipped.Add(levelName + $" (sin '{GEOMETRY_GROUP_NAME}')");
                    continue;
                }

                int blockCount = 0;
                foreach (Transform block in geometry.transform)
                {
                    if (PrefabUtility.IsPartOfPrefabInstance(block))
                        continue;

                    BoxCollider box = block.GetComponent<BoxCollider>();
                    if (box == null || block.GetComponent<MeshRenderer>() == null)
                        continue;

                    HidePrimitive(block.gameObject);
                    FillWithSnow(block, box, kit);
                    blockCount++;
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                dressed.Add($"{levelName} ({blockCount} bloques)");
            }

            return dressed;
        }

        private static void FillWithSnow(Transform owner, BoxCollider box, Material kit)
        {
            float height = box.size.y * owner.localScale.y;
            FillBox(owner, box, SnowTileFor(height), kit, true);
        }

        private static TileModel SnowTileFor(float height)
        {
            TileModel closest = SNOW_TILES[0];
            for (int i = 1; i < SNOW_TILES.Length; i++)
            {
                if (Mathf.Abs(SNOW_TILES[i].footprint.y - height) <= Mathf.Abs(closest.footprint.y - height))
                    closest = SNOW_TILES[i];
            }

            return closest;
        }

        private static void FillBox(Transform owner, BoxCollider box, TileModel tile, Material material, bool isStatic)
        {
            DestroyChild(owner, TILES_NAME);

            Vector3 size = Vector3.Scale(box.size, owner.localScale);
            Transform tiles = CreateChild(TILES_NAME, owner);
            tiles.localPosition = box.center;
            tiles.localScale = Inverse(owner.localScale);

            int countX = CountAlong(size.x, tile.footprint.x);
            int countY = CountAlong(size.y, tile.footprint.y);
            int countZ = CountAlong(size.z, tile.footprint.z);
            Vector3 step = new Vector3(size.x / countX, size.y / countY, size.z / countZ);
            Vector3 tileScale = new Vector3(step.x / tile.modelSize.x, step.y / tile.modelSize.y, step.z / tile.modelSize.z);
            GameObject model = LoadModel(tile.modelPath);
            int tileNumber = 1;

            for (int x = 0; x < countX; x++)
            {
                for (int y = 0; y < countY; y++)
                {
                    for (int z = 0; z < countZ; z++)
                    {
                        Vector3 position = new Vector3((x + 0.5f) * step.x, y * step.y, (z + 0.5f) * step.z) - size * 0.5f;
                        GameObject instance = PlaceModel(model, tiles, TILE_PREFIX + tileNumber, position, Quaternion.identity, tileScale, material);
                        tileNumber++;

                        if (isStatic)
                            MarkBatchingStatic(instance);
                    }
                }
            }
        }

        private static int CountAlong(float length, float footprint)
        {
            return Mathf.Max(1, Mathf.RoundToInt(length / footprint));
        }

        private static GameObject PlaceModel(GameObject model, Transform parent, string name, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Material material)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model, parent);
            instance.name = name;
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = localScale;

            foreach (MeshRenderer part in instance.GetComponentsInChildren<MeshRenderer>(true))
            {
                part.sharedMaterial = material;
            }

            return instance;
        }

        private static void MarkBatchingStatic(GameObject root)
        {
            foreach (Transform part in root.GetComponentsInChildren<Transform>(true))
            {
                GameObjectUtility.SetStaticEditorFlags(part.gameObject, StaticEditorFlags.BatchingStatic);
            }
        }

        private static void HidePrimitive(GameObject owner)
        {
            MeshRenderer primitive = owner.GetComponent<MeshRenderer>();
            if (primitive != null)
                primitive.enabled = false;
        }

        private static void HideChildPrimitives(Transform parent, string namePrefix)
        {
            foreach (Transform child in parent)
            {
                if (child.name.StartsWith(namePrefix, StringComparison.Ordinal))
                    HidePrimitive(child.gameObject);
            }
        }

        private static Transform CreateChild(string name, Transform parent)
        {
            Transform child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }

        private static void DestroyChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null)
                Object.DestroyImmediate(child.gameObject);
        }

        private static Vector3 Inverse(Vector3 scale)
        {
            return new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
        }

        private static GameObject LoadModel(string path)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null)
                throw new InvalidOperationException($"No se pudo cargar el modelo {path}.");

            return model;
        }

        private static ComponentType GetRequired<ComponentType>(GameObject owner, string assetPath) where ComponentType : Component
        {
            ComponentType component = owner.GetComponent<ComponentType>();
            if (component == null)
                throw new InvalidOperationException($"{assetPath} no tiene {typeof(ComponentType).Name} en '{owner.name}'.");

            return component;
        }

        private static Material EnsureMaterial(string path, Texture2D colormap, Shader litShader, Color emission)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            material = new Material(litShader);
            material.SetTexture(BASE_MAP_PROPERTY, colormap);
            material.SetColor(BASE_COLOR_PROPERTY, Color.white);
            material.SetFloat(SMOOTHNESS_PROPERTY, 0f);

            if (emission.maxColorComponent > 0f)
            {
                material.EnableKeyword(EMISSION_KEYWORD);
                material.SetTexture(EMISSION_MAP_PROPERTY, colormap);
                material.SetColor(EMISSION_COLOR_PROPERTY, emission);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static string NamesOrNone(List<string> names)
        {
            return names.Count > 0 ? string.Join(", ", names) : "ninguno";
        }

        private struct TileModel
        {
            public string modelPath;
            public Vector3 modelSize;
            public Vector3 footprint;

            public TileModel(string modelPath, Vector3 modelSize, Vector3 footprint)
            {
                this.modelPath = modelPath;
                this.modelSize = modelSize;
                this.footprint = footprint;
            }
        }
    }
}
