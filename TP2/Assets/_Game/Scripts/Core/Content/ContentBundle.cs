using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GunJump.Core
{
    public sealed class ContentBundle
    {
        public const string OUTPUT_ROOT = "AssetBundles";
        public const string EDITOR_PLATFORM_FOLDER = "Editor";
        public const string PLAYER_PLATFORM_FOLDER = "Android";
        public const string STREAMING_FOLDER = "AssetBundles";

        readonly Dictionary<string, Object> assets = new Dictionary<string, Object>();
        readonly ContentBundle[] dependencies;
        AssetBundle bundle;
        AssetBundleCreateRequest pendingLoad;

        readonly string name;
        public string Name => name;

        public ContentBundle(string bundleName, ContentBundle[] dependencies)
        {
            name = bundleName;
            this.dependencies = dependencies;
        }

        public bool IsLoaded => bundle != null;
        public string BundlePath => PathOf(Name);

        public static string Folder
        {
            get
            {
#if UNITY_EDITOR
                return Path.Combine(Path.GetDirectoryName(Application.dataPath), OUTPUT_ROOT, EDITOR_PLATFORM_FOLDER);
#else
                return Path.Combine(Application.streamingAssetsPath, STREAMING_FOLDER);
#endif
            }
        }

        public static string ManifestBundleName
        {
            get
            {
#if UNITY_EDITOR
                return EDITOR_PLATFORM_FOLDER;
#else
                return PLAYER_PLATFORM_FOLDER;
#endif
            }
        }

        public static string PathOf(string bundleName)
        {
            return Path.Combine(Folder, bundleName);
        }

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

            if (pendingLoad == null)
                pendingLoad = AssetBundle.LoadFromFileAsync(BundlePath);

            AssetBundleCreateRequest request = pendingLoad;
            yield return request;

            if (pendingLoad != request)
            {
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

        public void UnloadKeepingLoadedAssets()
        {
            assets.Clear();

            if (bundle == null)
                return;

            bundle.Unload(false);
            bundle = null;
        }

        public bool TryGetLoaded<AssetType>(string assetName, out AssetType asset) where AssetType : Object
        {
            asset = assets.TryGetValue(assetName, out Object cached) ? cached as AssetType : null;
            return asset != null;
        }

        public IEnumerator LoadAsset<AssetType>(string assetName, Action<AssetType> onLoaded) where AssetType : Object
        {
            if (assets.TryGetValue(assetName, out Object cached))
            {
                onLoaded(cached as AssetType);
                yield break;
            }

            if (bundle == null)
            {
                onLoaded(null);
                yield break;
            }

            AssetBundle source = bundle;
            AssetBundleRequest request = source.LoadAssetAsync<AssetType>(assetName);
            yield return request;

            if (bundle != source)
            {
                onLoaded(null);
                yield break;
            }

            AssetType asset = request.asset as AssetType;

            if (asset == null)
            {
                Debug.LogError($"El AssetBundle '{Name}' no tiene un {typeof(AssetType).Name} llamado '{assetName}'. Revisá Asset Name en el ShopItem.");
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
        static void UseEditorShaders(Object asset)
        {
            if (asset is Material material)
            {
                UseEditorShader(material);
                return;
            }

            if (!(asset is GameObject))
                return;

            foreach (Material loaded in Resources.FindObjectsOfTypeAll<Material>())
                UseEditorShader(loaded);
        }

        static void UseEditorShader(Material material)
        {
            if (material == null || material.shader == null)
                return;

            Shader editorShader = Shader.Find(material.shader.name);
            if (editorShader != null && editorShader != material.shader)
                material.shader = editorShader;
        }
#endif

        public void Unload()
        {
            assets.Clear();

            if (pendingLoad != null)
            {
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
