using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GunJump.Core
{
    // Envuelve uno de los AssetBundles del contenido de la tienda (modelos de armas, skins, balas, efectos):
    // lo abre cuando se le pide, cachea los assets que se sacan de él y se descarga entero con Unload.
    // No se crea a mano: se pide a ContentLibrary, que tiene uno solo por bundle.
    public sealed class ContentBundle
    {
        // Carpeta, relativa al proyecto, donde GunBundleBuilder deja los bundles (una subcarpeta por plataforma).
        public const string OutputRoot = "AssetBundles";
        public const string EditorPlatformFolder = "Editor";

        // Subcarpeta de OutputRoot donde GunBundleBuilder arma los bundles del build del jugador (solo Android).
        public const string PlayerPlatformFolder = "Android";

        // Carpeta dentro de StreamingAssets donde el build copia los bundles.
        public const string StreamingFolder = "AssetBundles";

        readonly Dictionary<string, Object> assets = new Dictionary<string, Object>();
        // Todas las dependencias, directas e indirectas (ver ContentLibrary).
        readonly ContentBundle[] dependencies;
        AssetBundle bundle;
        AssetBundleCreateRequest pendingLoad;

        public ContentBundle(string bundleName, ContentBundle[] dependencies)
        {
            Name = bundleName;
            this.dependencies = dependencies;
        }

        public string Name { get; }
        public bool IsLoaded => bundle != null;
        public string BundlePath => PathOf(Name);

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

        // BuildAssetBundles deja el AssetBundleManifest en un bundle que se llama como la carpeta donde armó.
        public static string ManifestBundleName
        {
            get
            {
#if UNITY_EDITOR
                return EditorPlatformFolder;
#else
                return PlayerPlatformFolder;
#endif
            }
        }

        public static string PathOf(string bundleName)
        {
            return Path.Combine(Folder, bundleName);
        }

        // Carga primero los bundles de los que depende: si no, lo que este bundle use de ellos
        // (por ejemplo, el material de un modelo) sale vacío y no se arregla al cargarlos después.
        public IEnumerator LoadBundle()
        {
            for (int i = 0; i < dependencies.Length; i++)
                yield return dependencies[i].LoadOwnFile();

            yield return LoadOwnFile();
        }

        IEnumerator LoadOwnFile()
        {
            if (bundle != null)
                yield break;

            // Si ya hay una carga en curso, esperamos esa en vez de abrir el archivo dos veces.
            if (pendingLoad == null)
                pendingLoad = AssetBundle.LoadFromFileAsync(BundlePath);

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
                Debug.LogError($"No se pudo cargar el AssetBundle '{Name}' en '{BundlePath}'. " +
                               "En el Editor, corré GunJump > Asset Bundles > Build Gun Bundles (Editor).");
            }
        }

        // Devuelve el asset sin esperar nada, si ya se cargó antes y el bundle no se descargó desde entonces.
        public bool TryGetLoaded<T>(string assetName, out T asset) where T : Object
        {
            asset = assets.TryGetValue(assetName, out Object cached) ? cached as T : null;
            return asset != null;
        }

        // Llama a onLoaded con el asset, o con null si el bundle no está cargado o no tiene ese asset.
        public IEnumerator LoadAsset<T>(string assetName, Action<T> onLoaded) where T : Object
        {
            if (assets.TryGetValue(assetName, out Object cached))
            {
                onLoaded(cached as T);
                yield break;
            }

            if (bundle == null)
            {
                onLoaded(null);
                yield break;
            }

            AssetBundle source = bundle;
            AssetBundleRequest request = source.LoadAssetAsync<T>(assetName);
            yield return request;

            if (bundle != source)
            {
                // Se descargó mientras cargaba.
                onLoaded(null);
                yield break;
            }

            T asset = request.asset as T;

            if (asset == null)
            {
                Debug.LogError($"El AssetBundle '{Name}' no tiene un {typeof(T).Name} llamado '{assetName}'. Revisá Asset Name en el ShopItem.");
            }
            else
            {
#if UNITY_EDITOR
                UseEditorShaders(asset);
#endif
                assets[assetName] = asset;
            }

            onLoaded(asset);
        }

#if UNITY_EDITOR
        // Los bundles del Editor se arman para Android y sus shaders no compilan para la API gráfica del
        // Editor (se verían rosas). Se cambian por los mismos shaders del Editor, buscados por nombre.
        // Solo toca los materiales que salieron del bundle, no los del proyecto.
        static void UseEditorShaders(Object asset)
        {
            if (asset is Material material)
            {
                UseEditorShader(material);
                return;
            }

            if (!(asset is GameObject model))
                return;

            // Incluye ParticleSystemRenderer, para los efectos de disparo.
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                foreach (Material shared in renderer.sharedMaterials)
                    UseEditorShader(shared);
            }
        }

        static void UseEditorShader(Material material)
        {
            if (material == null || material.shader == null)
                return;

            Shader editorShader = Shader.Find(material.shader.name);
            if (editorShader != null)
                material.shader = editorShader;
        }
#endif

        // Descarga el bundle y todo lo que se sacó de él. Destruí antes las instancias que estén en escena.
        public void Unload()
        {
            assets.Clear();

            if (pendingLoad != null)
            {
                // Leer assetBundle termina la carga en el momento; si no, el bundle quedaría cargado para siempre.
                AssetBundle loading = pendingLoad.assetBundle;
                pendingLoad = null;

                if (loading != null)
                    loading.Unload(true);
            }

            if (bundle != null)
            {
                bundle.Unload(true);
                bundle = null;
            }
        }
    }
}
