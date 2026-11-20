using System.Collections.Generic;
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

            Dictionary<ShopCategory, List<ShopItem>> byCategory = new Dictionary<ShopCategory, List<ShopItem>>();
            List<string> paths = new List<string>();

            foreach (string guid in AssetDatabase.FindAssets("t:ShopItem", new[] { SHOP_DATA_FOLDER }))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            paths.Sort(System.StringComparer.Ordinal);

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
