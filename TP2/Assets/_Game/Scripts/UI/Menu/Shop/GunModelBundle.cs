using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace _Game.Scripts.Shop
{
    // Envuelve el AssetBundle con los modelos 3D de las armas: se carga al abrir la tienda,
    // cachea los modelos que se piden y se descarga entero al cerrarla.
    public sealed class GunModelBundle
    {
        public const string BundleName = "guns";

        // Carpeta, relativa al proyecto, donde GunBundleBuilder deja los bundles (una subcarpeta por plataforma).
        public const string OutputRoot = "AssetBundles";
        public const string EditorPlatformFolder = "Editor";

        // Carpeta dentro de StreamingAssets donde el build copia el bundle.
        public const string StreamingFolder = "AssetBundles";

        private readonly Dictionary<string, GameObject> models = new Dictionary<string, GameObject>();
        private AssetBundle bundle;
        private AssetBundleCreateRequest pendingLoad;

        public bool IsLoaded => bundle != null;

        public static string Folder
        {
            get
            {
#if UNITY_EDITOR
                return Path.Combine(Path.GetDirectoryName(Application.dataPath), OutputRoot, EditorPlatformFolder);
#else
                return Path.Combine(Application.streamingAssetsPath, StreamingFolder);
#endif
            }
        }

        public static string BundlePath => Path.Combine(Folder, BundleName);

        public IEnumerator LoadBundle()
        {
            if (bundle != null)
            {
                yield break;
            }

            // Si ya hay una carga en curso, esperamos esa en vez de abrir el archivo dos veces.
            if (pendingLoad == null)
            {
                pendingLoad = AssetBundle.LoadFromFileAsync(BundlePath);
            }

            AssetBundleCreateRequest request = pendingLoad;
            yield return request;

            if (pendingLoad != request)
            {
                // Se descargó mientras cargaba.
                yield break;
            }

            pendingLoad = null;
            bundle = request.assetBundle;

            if (bundle == null)
            {
                Debug.LogError($"No se pudo cargar el AssetBundle de armas en '{BundlePath}'. " +
                               "En el Editor, corré GunJump > Asset Bundles > Build Gun Bundle (Editor).");
            }
        }

        // Llama a onLoaded con el prefab, o con null si el bundle no está cargado o no tiene ese modelo.
        public IEnumerator LoadModel(string modelName, Action<GameObject> onLoaded)
        {
            if (models.TryGetValue(modelName, out GameObject cached))
            {
                onLoaded(cached);
                yield break;
            }

            if (bundle == null)
            {
                onLoaded(null);
                yield break;
            }

            AssetBundle source = bundle;
            AssetBundleRequest request = source.LoadAssetAsync<GameObject>(modelName);
            yield return request;

            if (bundle != source)
            {
                // Se descargó mientras cargaba.
                onLoaded(null);
                yield break;
            }

            GameObject model = request.asset as GameObject;

            if (model == null)
            {
                Debug.LogError($"El AssetBundle '{BundleName}' no tiene un modelo llamado '{modelName}'. Revisá Model Name en el ShopItem.");
            }
            else
            {
#if UNITY_EDITOR
                UseEditorShaders(model);
#endif
                models[modelName] = model;
            }

            onLoaded(model);
        }

#if UNITY_EDITOR
        // El bundle del Editor se arma para Android y sus shaders no compilan para la API gráfica del
        // Editor (se verían rosas). Se cambian por los mismos shaders del Editor, buscados por nombre.
        // Solo toca los materiales que salieron del bundle, no los del proyecto.
        private static void UseEditorShaders(GameObject model)
        {
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material == null || material.shader == null)
                    {
                        continue;
                    }

                    Shader editorShader = Shader.Find(material.shader.name);

                    if (editorShader != null)
                    {
                        material.shader = editorShader;
                    }
                }
            }
        }
#endif

        // Descarga el bundle y todo lo que se sacó de él. Destruí antes las instancias que estén en escena.
        public void Unload()
        {
            models.Clear();

            if (pendingLoad != null)
            {
                // Leer assetBundle termina la carga en el momento; si no, el bundle quedaría cargado para siempre.
                AssetBundle loading = pendingLoad.assetBundle;
                pendingLoad = null;

                if (loading != null)
                {
                    loading.Unload(true);
                }
            }

            if (bundle != null)
            {
                bundle.Unload(true);
                bundle = null;
            }
        }
    }
}
