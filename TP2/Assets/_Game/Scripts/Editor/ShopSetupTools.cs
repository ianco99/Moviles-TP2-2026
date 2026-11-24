using System.Collections.Generic;
using System.IO;
using _Game.Scripts.Shop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace GunJump.EditorTools
{
    public static class ShopSetupTools
    {
        private const string SHOP_DATA_FOLDER = "Assets/_Game/Data/Shop";
        private const string CATALOG_PATH = SHOP_DATA_FOLDER + "/ShopCatalog.asset";
        private const string DEFAULT_GUN_PATH = SHOP_DATA_FOLDER + "/Guns/Guns_Pistol.asset";
        private const string DEFAULT_SKIN_PATH = SHOP_DATA_FOLDER + "/Skins/Skins_Common.asset";
        private const string DEFAULT_BULLET_PATH = SHOP_DATA_FOLDER + "/Bullets/Bullets_Common.asset";
        private const string SHOT_VFX_ITEMS_FOLDER_NAME = "Particles";
        private const string SHOT_VFX_ITEMS_FOLDER = SHOP_DATA_FOLDER + "/" + SHOT_VFX_ITEMS_FOLDER_NAME;
        private const string SHOT_VFX_ICONS_FOLDER = "Assets/_Game/Art/Textures/Icons/ShotVFX";
        private const string SHOT_VFX_ITEM_PREFIX = "Particles_";
        private const string SHOT_VFX_PREFAB_PREFIX = "HCFX_";
        private const string STUB_ITEM_PATH = SHOT_VFX_ITEMS_FOLDER + "/Particles_Explosion.asset";
        private const int DEFAULT_SHOT_VFX_PRICE = 100;
        private const string BULLET_ITEMS_FOLDER = SHOP_DATA_FOLDER + "/Bullets";
        private const string BULLET_MATERIALS_FOLDER = "Assets/_Game/Art/Materials/Bullets";
        private const string BULLET_ICONS_FOLDER = "Assets/_Game/Art/Textures/Icons/Bullets";
        private const string SKIN_ICONS_FOLDER = "Assets/_Game/Art/Textures/Icons/Skins";
        private const int DEFAULT_BULLET_PRICE = 100;
        private const string LIT_SHADER_NAME = "Universal Render Pipeline/Lit";
        private const string BASE_COLOR_PROPERTY = "_BaseColor";
        private const string METALLIC_PROPERTY = "_Metallic";
        private const string SMOOTHNESS_PROPERTY = "_Smoothness";
        private const string EMISSION_COLOR_PROPERTY = "_EmissionColor";
        private const string EMISSION_KEYWORD = "_EMISSION";
        private const string STAGE_NAME = "GunPreviewStage";
        private static readonly Vector3 STAGE_POSITION = new Vector3(0f, -1000f, 0f);
        private static readonly Vector3 ALONG_FLIGHT = new Vector3(0f, 0f, 90f);

        private static readonly BulletPart[] CLASSIC_SHAPE =
        {
            new BulletPart(PrimitiveType.Cylinder, new Vector3(-0.06f, 0f, 0f), ALONG_FLIGHT, new Vector3(0.1f, 0.12f, 0.1f)),
            new BulletPart(PrimitiveType.Sphere, new Vector3(0.06f, 0f, 0f), ALONG_FLIGHT, new Vector3(0.1f, 0.24f, 0.1f))
        };

        private static readonly BulletPart[] SLUG_SHAPE =
        {
            new BulletPart(PrimitiveType.Cylinder, Vector3.zero, ALONG_FLIGHT, new Vector3(0.13f, 0.13f, 0.13f))
        };

        private static readonly BulletPart[] ORB_SHAPE =
        {
            new BulletPart(PrimitiveType.Sphere, Vector3.zero, ALONG_FLIGHT, new Vector3(0.16f, 0.2f, 0.16f))
        };

        private static readonly BulletPart[] BOLT_SHAPE =
        {
            new BulletPart(PrimitiveType.Capsule, Vector3.zero, ALONG_FLIGHT, new Vector3(0.05f, 0.25f, 0.05f))
        };

        private static readonly BulletPart[] SHARD_SHAPE =
        {
            new BulletPart(PrimitiveType.Cube, Vector3.zero, new Vector3(45f, 0f, 0f), new Vector3(0.34f, 0.09f, 0.09f))
        };

        private static readonly BulletStyle[] BULLET_STYLES =
        {
            new BulletStyle("Bullets_Common", CLASSIC_SHAPE, new Color(0.83f, 0.65f, 0.27f), 1f, 0.7f, Color.black, 0),
            new BulletStyle("Bullets_Copper", SLUG_SHAPE, new Color(0.78f, 0.4f, 0.23f), 1f, 0.6f, Color.black, DEFAULT_BULLET_PRICE),
            new BulletStyle("Bullets_Chrome", CLASSIC_SHAPE, new Color(0.9f, 0.92f, 0.95f), 1f, 0.92f, Color.black, DEFAULT_BULLET_PRICE),
            new BulletStyle("Bullets_Gold", CLASSIC_SHAPE, new Color(1f, 0.8f, 0.2f), 1f, 0.8f, new Color(1f, 0.7f, 0f) * 0.8f, DEFAULT_BULLET_PRICE),
            new BulletStyle("Bullets_Stealth", CLASSIC_SHAPE, new Color(0.11f, 0.11f, 0.14f), 0.2f, 0.25f, Color.black, DEFAULT_BULLET_PRICE),
            new BulletStyle("Bullets_Candy", ORB_SHAPE, new Color(1f, 0.44f, 0.71f), 0f, 0.95f, Color.black, DEFAULT_BULLET_PRICE),
            new BulletStyle("Bullets_Plasma", ORB_SHAPE, new Color(0.25f, 0.91f, 1f), 0f, 0.8f, new Color(0.25f, 0.91f, 1f) * 3f, DEFAULT_BULLET_PRICE),
            new BulletStyle("Bullets_Toxic", ORB_SHAPE, new Color(0.49f, 1f, 0.23f), 0f, 0.6f, new Color(0.49f, 1f, 0.23f) * 2.5f, DEFAULT_BULLET_PRICE),
            new BulletStyle("Bullets_Laser", BOLT_SHAPE, new Color(1f, 0.17f, 0.24f), 0f, 0.5f, new Color(1f, 0.17f, 0.24f) * 4f, DEFAULT_BULLET_PRICE),
            new BulletStyle("Bullets_Void", CLASSIC_SHAPE, new Color(0.15f, 0.06f, 0.23f), 0.3f, 0.7f, new Color(0.61f, 0.23f, 1f) * 2f, DEFAULT_BULLET_PRICE),
            new BulletStyle("Bullets_Ice", SHARD_SHAPE, new Color(0.75f, 0.94f, 1f), 0f, 0.95f, new Color(0.5f, 0.85f, 1f) * 0.6f, DEFAULT_BULLET_PRICE)
        };

        [MenuItem("GunJump/Shop/Create Shop Catalog")]
        private static void CreateCatalog()
        {
            if (AssetDatabase.LoadAssetAtPath<ShopCatalog>(CATALOG_PATH) != null &&
                !EditorUtility.DisplayDialog("GunJump", $"Ya existe {CATALOG_PATH}. ¿Reemplazarlo?", "Reemplazar", "Cancelar"))
            {
                return;
            }

            List<string> paths = CollectItemPaths();
            Dictionary<ShopCategory, List<ShopItem>> byCategory = GroupByCategory(paths);

            ShopCatalog catalog = ScriptableObject.CreateInstance<ShopCatalog>();
            SerializedObject serialized = new SerializedObject(catalog);
            Fill(serialized.FindProperty("guns"), byCategory, ShopCategory.Guns);
            Fill(serialized.FindProperty("gunSkins"), byCategory, ShopCategory.GunSkins);
            Fill(serialized.FindProperty("bullets"), byCategory, ShopCategory.Bullets);
            Fill(serialized.FindProperty("shotVfx"), byCategory, ShopCategory.ShotVFX);
            serialized.FindProperty("defaultGun").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ShopItem>(DEFAULT_GUN_PATH);
            serialized.FindProperty("defaultSkin").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ShopItem>(DEFAULT_SKIN_PATH);
            serialized.FindProperty("defaultBullet").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ShopItem>(DEFAULT_BULLET_PATH);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            ShopCatalog existing = AssetDatabase.LoadAssetAtPath<ShopCatalog>(CATALOG_PATH);

            if (existing != null)
            {
                EditorUtility.CopySerialized(catalog, existing);
                Object.DestroyImmediate(catalog);
                EditorUtility.SetDirty(existing);
                catalog = existing;
            }
            else
            {
                AssetDatabase.CreateAsset(catalog, CATALOG_PATH);
            }

            AssetDatabase.SaveAssets();
            Selection.activeObject = catalog;
            EditorGUIUtility.PingObject(catalog);
            Debug.Log($"ShopCatalog creado en {CATALOG_PATH} con {paths.Count} items.", catalog);
        }

        private static List<string> CollectItemPaths()
        {
            List<string> paths = new List<string>();

            foreach (string guid in AssetDatabase.FindAssets("t:ShopItem", new string[] { SHOP_DATA_FOLDER }))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            paths.Sort(System.StringComparer.Ordinal);
            return paths;
        }

        private static Dictionary<ShopCategory, List<ShopItem>> GroupByCategory(List<string> paths)
        {
            Dictionary<ShopCategory, List<ShopItem>> byCategory = new Dictionary<ShopCategory, List<ShopItem>>();

            foreach (string path in paths)
            {
                ShopItem item = AssetDatabase.LoadAssetAtPath<ShopItem>(path);

                if (!byCategory.TryGetValue(item.Category, out List<ShopItem> list))
                {
                    list = new List<ShopItem>();
                    byCategory.Add(item.Category, list);
                }

                list.Add(item);
            }

            return byCategory;
        }

        private static void Fill(SerializedProperty list, Dictionary<ShopCategory, List<ShopItem>> byCategory, ShopCategory category)
        {
            byCategory.TryGetValue(category, out List<ShopItem> items);
            int count = items?.Count ?? 0;
            list.arraySize = count;

            for (int i = 0; i < count; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }
        }

        [MenuItem("GunJump/Shop/Create Shot VFX Items")]
        private static void CreateShotVfxItems()
        {
            ShopCatalog catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(CATALOG_PATH);

            if (catalog == null)
            {
                EditorUtility.DisplayDialog("GunJump", $"No existe {CATALOG_PATH}. Corré antes GunJump > Shop > Create Shop Catalog.", "Entendido");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<ShopItem>(STUB_ITEM_PATH) != null)
            {
                AssetDatabase.DeleteAsset(STUB_ITEM_PATH);
                Debug.Log($"Borrado {STUB_ITEM_PATH}: era un item de prueba sin efecto asignado.");
            }

            if (!AssetDatabase.IsValidFolder(SHOT_VFX_ITEMS_FOLDER))
            {
                AssetDatabase.CreateFolder(SHOP_DATA_FOLDER, SHOT_VFX_ITEMS_FOLDER_NAME);
            }

            List<string> prefabPaths = new List<string>();

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new string[] { GunBundleBuilder.SHOT_VFX_FOLDER }))
            {
                prefabPaths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            prefabPaths.Sort(System.StringComparer.Ordinal);

            int createdCount = 0;
            int skippedCount = 0;
            List<string> prefabsWithoutIcon = new List<string>();

            foreach (string prefabPath in prefabPaths)
            {
                string prefabName = Path.GetFileNameWithoutExtension(prefabPath);
                string effectName = prefabName.StartsWith(SHOT_VFX_PREFAB_PREFIX, System.StringComparison.Ordinal)
                    ? prefabName.Substring(SHOT_VFX_PREFAB_PREFIX.Length)
                    : prefabName;
                string itemPath = $"{SHOT_VFX_ITEMS_FOLDER}/{SHOT_VFX_ITEM_PREFIX}{effectName}.asset";

                if (AssetDatabase.LoadAssetAtPath<ShopItem>(itemPath) != null)
                {
                    skippedCount++;
                    continue;
                }

                Sprite icon = ImportAsSprite($"{SHOT_VFX_ICONS_FOLDER}/{prefabName}.png");

                if (icon == null)
                {
                    prefabsWithoutIcon.Add(prefabName);
                }

                ShopItem item = ScriptableObject.CreateInstance<ShopItem>();
                SerializedObject serializedItem = new SerializedObject(item);
                serializedItem.FindProperty("sprite").objectReferenceValue = icon;
                serializedItem.FindProperty("category").enumValueIndex = (int)ShopCategory.ShotVFX;
                serializedItem.FindProperty("price").intValue = DEFAULT_SHOT_VFX_PRICE;
                serializedItem.FindProperty("assetName").stringValue = prefabName;
                serializedItem.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(item, itemPath);
                createdCount++;
            }

            AssetDatabase.SaveAssets();

            SerializedObject serializedCatalog = new SerializedObject(catalog);
            Fill(serializedCatalog.FindProperty("shotVfx"), GroupByCategory(CollectItemPaths()), ShopCategory.ShotVFX);
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            if (prefabsWithoutIcon.Count > 0)
            {
                Debug.LogWarning($"Estos efectos no tienen ícono en {SHOT_VFX_ICONS_FOLDER}: {string.Join(", ", prefabsWithoutIcon)}.");
            }

            Selection.activeObject = catalog;
            EditorGUIUtility.PingObject(catalog);
            Debug.Log($"Shot VFX: {createdCount} items creados y {skippedCount} que ya existían en {SHOT_VFX_ITEMS_FOLDER}. " +
                      $"ShopCatalog tiene {catalog.GetItems(ShopCategory.ShotVFX).Count} efectos.", catalog);
        }

        private static Sprite ImportAsSprite(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            AssetDatabase.ImportAsset(path);

            if (AssetImporter.GetAtPath(path) is TextureImporter importer && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        [MenuItem("GunJump/Shop/Create Bullet Items")]
        private static void CreateBulletItems()
        {
            ShopCatalog catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(CATALOG_PATH);

            if (catalog == null)
            {
                EditorUtility.DisplayDialog("GunJump", $"No existe {CATALOG_PATH}. Corré antes GunJump > Shop > Create Shop Catalog.", "Entendido");
                return;
            }

            Shader litShader = Shader.Find(LIT_SHADER_NAME);

            if (litShader == null)
            {
                EditorUtility.DisplayDialog("GunJump", $"No se encontró el shader '{LIT_SHADER_NAME}'.", "Entendido");
                return;
            }

            EnsureFolder(BULLET_MATERIALS_FOLDER);
            EnsureFolder(GunBundleBuilder.BULLETS_FOLDER);
            EnsureFolder(BULLET_ITEMS_FOLDER);

            int createdCount = 0;
            int repairedCount = 0;

            foreach (BulletStyle style in BULLET_STYLES)
            {
                Material material = EnsureBulletMaterial(style, litShader);
                EnsureBulletPrefab(style, material);

                string itemPath = $"{BULLET_ITEMS_FOLDER}/{style.name}.asset";
                ShopItem item = AssetDatabase.LoadAssetAtPath<ShopItem>(itemPath);

                if (item == null)
                {
                    item = ScriptableObject.CreateInstance<ShopItem>();
                    WriteBulletItem(item, style);
                    AssetDatabase.CreateAsset(item, itemPath);
                    createdCount++;
                }
                else if (item.Category != ShopCategory.Bullets || string.IsNullOrEmpty(item.AssetName))
                {
                    WriteBulletItem(item, style);
                    EditorUtility.SetDirty(item);
                    repairedCount++;
                }
            }

            AssetDatabase.SaveAssets();

            SerializedObject serializedCatalog = new SerializedObject(catalog);
            Fill(serializedCatalog.FindProperty("bullets"), GroupByCategory(CollectItemPaths()), ShopCategory.Bullets);
            serializedCatalog.FindProperty("defaultBullet").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ShopItem>(DEFAULT_BULLET_PATH);
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);

            int iconCount = AssignIcons(catalog, ShopCategory.Bullets, BULLET_ICONS_FOLDER);
            AssetDatabase.SaveAssets();

            Selection.activeObject = catalog;
            EditorGUIUtility.PingObject(catalog);
            Debug.Log($"Balas: {createdCount} items creados y {repairedCount} corregidos en {BULLET_ITEMS_FOLDER}, {iconCount} íconos asignados. " +
                      $"ShopCatalog tiene {catalog.GetItems(ShopCategory.Bullets).Count} balas.", catalog);
        }

        [MenuItem("GunJump/Shop/Apply Skin Icons")]
        private static void ApplySkinIcons()
        {
            ShopCatalog catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(CATALOG_PATH);

            if (catalog == null)
            {
                EditorUtility.DisplayDialog("GunJump", $"No existe {CATALOG_PATH}. Corré antes GunJump > Shop > Create Shop Catalog.", "Entendido");
                return;
            }

            int iconCount = AssignIcons(catalog, ShopCategory.GunSkins, SKIN_ICONS_FOLDER);
            AssetDatabase.SaveAssets();
            Debug.Log($"Skins: {iconCount} íconos asignados desde {SKIN_ICONS_FOLDER}.", catalog);
        }

        private static Material EnsureBulletMaterial(BulletStyle style, Shader litShader)
        {
            string path = $"{BULLET_MATERIALS_FOLDER}/{style.name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material != null)
            {
                return material;
            }

            material = new Material(litShader);
            material.SetColor(BASE_COLOR_PROPERTY, style.baseColor);
            material.SetFloat(METALLIC_PROPERTY, style.metallic);
            material.SetFloat(SMOOTHNESS_PROPERTY, style.smoothness);

            if (style.emission.maxColorComponent > 0f)
            {
                material.EnableKeyword(EMISSION_KEYWORD);
                material.SetColor(EMISSION_COLOR_PROPERTY, style.emission);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureBulletPrefab(BulletStyle style, Material material)
        {
            string path = $"{GunBundleBuilder.BULLETS_FOLDER}/{style.name}.prefab";

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                return;
            }

            GameObject root = new GameObject(style.name);

            foreach (BulletPart part in style.parts)
            {
                GameObject partObject = GameObject.CreatePrimitive(part.primitive);
                Object.DestroyImmediate(partObject.GetComponent<Collider>());
                partObject.transform.SetParent(root.transform, false);
                partObject.transform.localPosition = part.localPosition;
                partObject.transform.localEulerAngles = part.localEulerAngles;
                partObject.transform.localScale = part.localScale;

                MeshRenderer partRenderer = partObject.GetComponent<MeshRenderer>();
                partRenderer.sharedMaterial = material;
                partRenderer.shadowCastingMode = ShadowCastingMode.Off;
                partRenderer.receiveShadows = false;
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void WriteBulletItem(ShopItem item, BulletStyle style)
        {
            SerializedObject serializedItem = new SerializedObject(item);
            serializedItem.FindProperty("category").enumValueIndex = (int)ShopCategory.Bullets;
            serializedItem.FindProperty("price").intValue = style.price;
            serializedItem.FindProperty("assetName").stringValue = style.name;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
        }

        private static int AssignIcons(ShopCatalog catalog, ShopCategory category, string iconsFolder)
        {
            int assignedCount = 0;
            List<string> itemsWithoutIcon = new List<string>();

            foreach (ShopItem item in catalog.GetItems(category))
            {
                if (item == null)
                {
                    continue;
                }

                Sprite icon = ImportAsSprite($"{iconsFolder}/{item.name}.png");

                if (icon == null)
                {
                    itemsWithoutIcon.Add(item.name);
                    continue;
                }

                SerializedObject serializedItem = new SerializedObject(item);
                serializedItem.FindProperty("sprite").objectReferenceValue = icon;
                serializedItem.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(item);
                assignedCount++;
            }

            if (itemsWithoutIcon.Count > 0)
            {
                Debug.LogWarning($"Estos items de {category} no tienen ícono en {iconsFolder}: {string.Join(", ", itemsWithoutIcon)}.");
            }

            return assignedCount;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            AssetDatabase.CreateFolder(Path.GetDirectoryName(folder).Replace('\\', '/'), Path.GetFileName(folder));
        }

        [MenuItem("GunJump/Shop/Create Gun Preview Stage")]
        private static void CreatePreviewStage()
        {
            GameObject existing = GameObject.Find(STAGE_NAME);

            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("GunJump", $"Ya hay un {STAGE_NAME} en la escena. ¿Reemplazarlo?", "Reemplazar", "Cancelar"))
                {
                    return;
                }

                Undo.DestroyObjectImmediate(existing);
            }

            GameObject stage = new GameObject(STAGE_NAME);
            Undo.RegisterCreatedObjectUndo(stage, "Create Gun Preview Stage");
            stage.transform.position = STAGE_POSITION;

            Transform spinner = new GameObject("Spinner").transform;
            spinner.SetParent(stage.transform, false);

            GameObject cameraObject = new GameObject("PreviewCamera");
            cameraObject.transform.SetParent(stage.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0.25f, -2.2f);
            cameraObject.transform.localRotation = Quaternion.Euler(6f, 0f, 0f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.fieldOfView = 30f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10f;
            camera.enabled = false;

            if (!HasDirectionalLight())
            {
                GameObject lightObject = new GameObject("PreviewLight");
                lightObject.transform.SetParent(stage.transform, false);
                lightObject.transform.localRotation = Quaternion.Euler(40f, -30f, 0f);
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
            }

            GunPreviewController controller = stage.AddComponent<GunPreviewController>();
            SerializedObject serialized = new SerializedObject(controller);
            serialized.FindProperty("previewCamera").objectReferenceValue = camera;
            serialized.FindProperty("spinner").objectReferenceValue = spinner;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(stage.scene);
            Selection.activeGameObject = stage;
            Debug.Log($"{STAGE_NAME} creado. Falta asignar el RawImage de la tienda en 'Target' y guardar la escena.", stage);
        }

        private static bool HasDirectionalLight()
        {
            foreach (Light light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (light.type == LightType.Directional)
                {
                    return true;
                }
            }

            return false;
        }

        private struct BulletPart
        {
            public PrimitiveType primitive;
            public Vector3 localPosition;
            public Vector3 localEulerAngles;
            public Vector3 localScale;

            public BulletPart(PrimitiveType primitive, Vector3 localPosition, Vector3 localEulerAngles, Vector3 localScale)
            {
                this.primitive = primitive;
                this.localPosition = localPosition;
                this.localEulerAngles = localEulerAngles;
                this.localScale = localScale;
            }
        }

        private struct BulletStyle
        {
            public string name;
            public BulletPart[] parts;
            public Color baseColor;
            public float metallic;
            public float smoothness;
            public Color emission;
            public int price;

            public BulletStyle(string name, BulletPart[] parts, Color baseColor, float metallic, float smoothness, Color emission, int price)
            {
                this.name = name;
                this.parts = parts;
                this.baseColor = baseColor;
                this.metallic = metallic;
                this.smoothness = smoothness;
                this.emission = emission;
                this.price = price;
            }
        }
    }
}
