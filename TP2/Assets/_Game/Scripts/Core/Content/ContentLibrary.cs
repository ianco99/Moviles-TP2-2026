using System;
using System.Collections.Generic;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using UnityEngine;

namespace GunJump.Core
{
    public sealed class ContentLibrary : IService
    {
        public const string PREFABS_BUNDLE = "prefabs";

        const string MANIFEST_ASSET_NAME = "AssetBundleManifest";

        readonly Dictionary<string, ContentBundle> bundles = new Dictionary<string, ContentBundle>();
        readonly EventBus.EventCallback<LevelLoadStartedEvent> loadStartedCallback;
        Dictionary<string, string[]> dependencies;

        public bool IsPersistance => true;

        public ContentLibrary(EventBus eventBus)
        {
            loadStartedCallback = OnLevelLoadStarted;
            eventBus.Subscribe(loadStartedCallback);
        }

        void OnLevelLoadStarted(in LevelLoadStartedEvent levelLoadStartedEvent)
        {
            foreach (ContentBundle bundle in bundles.Values)
            {
                bundle.UnloadKeepingLoadedAssets();
            }
        }

        public ContentBundle Get(string bundleName)
        {
            if (bundles.TryGetValue(bundleName, out ContentBundle bundle))
            {
                return bundle;
            }

            string[] dependencyNames = DependenciesOf(bundleName);
            ContentBundle[] dependencyBundles = new ContentBundle[dependencyNames.Length];
            bundle = new ContentBundle(bundleName, dependencyBundles);
            bundles.Add(bundleName, bundle);

            for (int i = 0; i < dependencyNames.Length; i++)
            {
                dependencyBundles[i] = Get(dependencyNames[i]);
            }

            return bundle;
        }

        string[] DependenciesOf(string bundleName)
        {
            dependencies ??= ReadManifest();
            return dependencies.TryGetValue(bundleName, out string[] names) ? names : Array.Empty<string>();
        }

        static Dictionary<string, string[]> ReadManifest()
        {
            Dictionary<string, string[]> result = new Dictionary<string, string[]>();
            string path = ContentBundle.PathOf(ContentBundle.ManifestBundleName);
            AssetBundle manifestBundle = AssetBundle.LoadFromFile(path);

            if (manifestBundle == null)
            {
                Debug.LogError($"No se pudo cargar el manifest de los AssetBundles en '{path}'. Sin él no se cargan las " +
                               "dependencias entre bundles y el arma puede verse rosa. En el Editor, corré " +
                               "GunJump > Asset Bundles > Build Gun Bundles (Editor).");
                return result;
            }

            AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>(MANIFEST_ASSET_NAME);

            if (manifest == null)
            {
                Debug.LogError($"El bundle '{path}' no tiene un AssetBundleManifest.");
            }
            else
            {
                foreach (string name in manifest.GetAllAssetBundles())
                {
                    result[name] = manifest.GetAllDependencies(name);
                }
            }

            manifestBundle.Unload(true);
            return result;
        }
    }
}
