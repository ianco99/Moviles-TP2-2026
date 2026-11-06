using System;
using System.Collections.Generic;
using ianco99.ToolBox.Services;
using UnityEngine;

namespace GunJump.Core
{
    // Un ContentBundle por nombre para toda la sesión: la tienda y el arma piden acá el mismo,
    // porque Unity no deja abrir dos veces el mismo AssetBundle. Lo que ya se cargó queda listo
    // para la próxima vez; por ejemplo, al reintentar un nivel el arma no vuelve a cargar nada.
    //
    // También le dice a cada bundle de qué otros depende, leyéndolo del manifest que arma GunBundleBuilder.
    // Ejemplo: los modelos de 'guns' usan Skins_Common, que está en 'skins'. Si 'skins' no se carga antes,
    // el arma sale sin material (rosa).
    public sealed class ContentLibrary : IService
    {
        // Nombre que BuildPipeline.BuildAssetBundles le da al AssetBundleManifest dentro de su bundle.
        const string ManifestAssetName = "AssetBundleManifest";

        readonly Dictionary<string, ContentBundle> bundles = new Dictionary<string, ContentBundle>();
        Dictionary<string, string[]> dependencies;

        public bool IsPersistance => true;

        public ContentBundle Get(string bundleName)
        {
            if (bundles.TryGetValue(bundleName, out ContentBundle bundle))
            {
                return bundle;
            }

            string[] dependencyNames = DependenciesOf(bundleName);
            var dependencyBundles = new ContentBundle[dependencyNames.Length];
            bundle = new ContentBundle(bundleName, dependencyBundles);
            bundles.Add(bundleName, bundle);

            // Se llenan después de registrarlo, por si dos bundles dependen uno del otro.
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

        // Lee una sola vez, al pedir el primer bundle, todas las dependencias (directas e indirectas) de cada bundle.
        static Dictionary<string, string[]> ReadManifest()
        {
            var result = new Dictionary<string, string[]>();
            string path = ContentBundle.PathOf(ContentBundle.ManifestBundleName);
            AssetBundle manifestBundle = AssetBundle.LoadFromFile(path);

            if (manifestBundle == null)
            {
                Debug.LogError($"No se pudo cargar el manifest de los AssetBundles en '{path}'. Sin él no se cargan las " +
                               "dependencias entre bundles y el arma puede verse rosa. En el Editor, corré " +
                               "GunJump > Asset Bundles > Build Gun Bundles (Editor).");
                return result;
            }

            AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>(ManifestAssetName);

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
