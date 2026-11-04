using System.Collections.Generic;
using _Game.Scripts.Shop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GunJump.EditorTools
{
    public static class ShopSetupTools
    {
        private const string ShopDataFolder = "Assets/_Game/Data/Shop";
        private const string CatalogPath = ShopDataFolder + "/ShopCatalog.asset";
        private const string DefaultGunPath = ShopDataFolder + "/Guns/Guns_Pistol.asset";
        private const string DefaultSkinPath = ShopDataFolder + "/Skins/Skins_Common.asset";
        private const string StageName = "GunPreviewStage";
        private static readonly Vector3 StagePosition = new Vector3(0f, -1000f, 0f);

        // Junta todos los ShopItem de Data/Shop en un ShopCatalog, por categoría y en orden alfabético
        // (el mismo orden que tenían las listas en ShopMenuController).
        [MenuItem("GunJump/Shop/Create Shop Catalog")]
        private static void CreateCatalog()
        {
            if (AssetDatabase.LoadAssetAtPath<ShopCatalog>(CatalogPath) != null &&
                !EditorUtility.DisplayDialog("GunJump", $"Ya existe {CatalogPath}. ¿Reemplazarlo?", "Reemplazar", "Cancelar"))
            {
                return;
            }

            var byCategory = new Dictionary<ShopCategory, List<ShopItem>>();
            var paths = new List<string>();

            foreach (string guid in AssetDatabase.FindAssets("t:ShopItem", new[] { ShopDataFolder }))
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

            var catalog = ScriptableObject.CreateInstance<ShopCatalog>();
            var serialized = new SerializedObject(catalog);
            Fill(serialized.FindProperty("guns"), byCategory, ShopCategory.Guns);
            Fill(serialized.FindProperty("gunSkins"), byCategory, ShopCategory.GunSkins);
            Fill(serialized.FindProperty("bullets"), byCategory, ShopCategory.Bullets);
            Fill(serialized.FindProperty("shotVfx"), byCategory, ShopCategory.ShotVFX);
            serialized.FindProperty("defaultGun").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ShopItem>(DefaultGunPath);
            serialized.FindProperty("defaultSkin").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ShopItem>(DefaultSkinPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            // CreateAsset sobre un archivo existente le cambiaría el GUID y rompería las referencias: copiamos encima.
            ShopCatalog existing = AssetDatabase.LoadAssetAtPath<ShopCatalog>(CatalogPath);

            if (existing != null)
            {
                EditorUtility.CopySerialized(catalog, existing);
                Object.DestroyImmediate(catalog);
                EditorUtility.SetDirty(existing);
                catalog = existing;
            }
            else
            {
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            AssetDatabase.SaveAssets();
            Selection.activeObject = catalog;
            EditorGUIUtility.PingObject(catalog);
            Debug.Log($"ShopCatalog creado en {CatalogPath} con {paths.Count} items.", catalog);
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

        // Crea en la escena abierta el escenario de la vista previa: cámara, luz y pivote, lejos de todo,
        // con el GunPreviewController ya conectado. Falta asignarle el RawImage de la tienda.
        [MenuItem("GunJump/Shop/Create Gun Preview Stage")]
        private static void CreatePreviewStage()
        {
            GameObject existing = GameObject.Find(StageName);

            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("GunJump", $"Ya hay un {StageName} en la escena. ¿Reemplazarlo?", "Reemplazar", "Cancelar"))
                {
                    return;
                }

                Undo.DestroyObjectImmediate(existing);
            }

            var stage = new GameObject(StageName);
            Undo.RegisterCreatedObjectUndo(stage, "Create Gun Preview Stage");
            stage.transform.position = StagePosition;

            var spinner = new GameObject("Spinner").transform;
            spinner.SetParent(stage.transform, false);

            var cameraObject = new GameObject("PreviewCamera");
            cameraObject.transform.SetParent(stage.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0.25f, -2.2f);
            cameraObject.transform.localRotation = Quaternion.Euler(6f, 0f, 0f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.fieldOfView = 30f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10f;
            camera.enabled = false;

            // Una luz direccional ilumina toda la escena, así que solo la agregamos si no hay ninguna.
            if (!HasDirectionalLight())
            {
                var lightObject = new GameObject("PreviewLight");
                lightObject.transform.SetParent(stage.transform, false);
                lightObject.transform.localRotation = Quaternion.Euler(40f, -30f, 0f);
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
            }

            var controller = stage.AddComponent<GunPreviewController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("previewCamera").objectReferenceValue = camera;
            serialized.FindProperty("spinner").objectReferenceValue = spinner;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(stage.scene);
            Selection.activeGameObject = stage;
            Debug.Log($"{StageName} creado. Falta asignar el RawImage de la tienda en 'Target' y guardar la escena.", stage);
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
