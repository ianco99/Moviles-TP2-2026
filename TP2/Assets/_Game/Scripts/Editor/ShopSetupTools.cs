using System.Collections.Generic;
using System.IO;
using _Game.Scripts.Shop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GunJump.EditorTools
{
    public static class ShopSetupTools
    {
        private const string SHOP_DATA_FOLDER = "Assets/_Game/Data/Shop";
        private const string CATALOG_PATH = SHOP_DATA_FOLDER + "/ShopCatalog.asset";
        private const string DEFAULT_GUN_PATH = SHOP_DATA_FOLDER + "/Guns/Guns_Pistol.asset";
        private const string DEFAULT_SKIN_PATH = SHOP_DATA_FOLDER + "/Skins/Skins_Common.asset";
        private const string SHOT_VFX_ITEMS_FOLDER_NAME = "Particles";
        private const string SHOT_VFX_ITEMS_FOLDER = SHOP_DATA_FOLDER + "/" + SHOT_VFX_ITEMS_FOLDER_NAME;
        private const string SHOT_VFX_ICONS_FOLDER = "Assets/_Game/Art/Textures/Icons/ShotVFX";
        private const string SHOT_VFX_ITEM_PREFIX = "Particles_";
        private const string SHOT_VFX_PREFAB_PREFIX = "HCFX_";
        private const string STUB_ITEM_PATH = SHOT_VFX_ITEMS_FOLDER + "/Particles_Explosion.asset";
        private const int DEFAULT_SHOT_VFX_PRICE = 100;
        private const string STAGE_NAME = "GunPreviewStage";
        private static readonly Vector3 STAGE_POSITION = new Vector3(0f, -1000f, 0f);

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
    }
}
