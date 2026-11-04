using System.Collections.Generic;
using System.IO;
using _Game.Scripts.Shop;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace GunJump.EditorTools
{
    // Arma el AssetBundle "guns" con todos los modelos de Art/Models.
    // - Para Play mode: GunJump > Asset Bundles > Build Gun Bundle (Editor), o solo, al dar Play si falta.
    // - Para el build: se arma solo antes de cada build del jugador y se copia a StreamingAssets,
    //   así que nunca hay que commitearlo ni se puede subir uno viejo.
    public sealed class GunBundleBuilder : BuildPlayerProcessor
    {
        private const string ModelsFolder = "Assets/_Game/Art/Models";

        public override int callbackOrder => 0;

        public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            BuildTarget target = buildPlayerContext.BuildPlayerOptions.target;
            string output = Build(target, target.ToString());
            buildPlayerContext.AddAdditionalPathToStreamingAssets(output, GunModelBundle.StreamingFolder);
        }

        [MenuItem("GunJump/Asset Bundles/Build Gun Bundle (Editor)")]
        private static void BuildForEditorMenu()
        {
            try
            {
                string output = BuildForEditor();
                EditorUtility.RevealInFinder(Path.Combine(output, GunModelBundle.BundleName));
            }
            catch (BuildFailedException e)
            {
                EditorUtility.DisplayDialog("GunJump", e.Message, "Entendido");
            }
        }

        // Siempre para Android, y solo si Android es la plataforma activa:
        // - armar un bundle compila los scripts del jugador para esa plataforma, y los que usan
        //   GooglePlayGames solo compilan para Android;
        // - armarlo para otra plataforma que la activa puede dejar el Editor cambiado a esa plataforma
        //   si el build falla (ya pasó: quedó en Windows).
        // Los shaders de un bundle de Android no andan en el Editor; GunModelBundle los reemplaza al cargar.
        public static string BuildForEditor()
        {
            BuildTarget active = EditorUserBuildSettings.activeBuildTarget;

            if (active != BuildTarget.Android)
            {
                throw new BuildFailedException(
                    $"La plataforma activa es {active}, no Android. Cambiá a Android en File > Build Profiles " +
                    "y volvé a armar el bundle de armas.");
            }

            return Build(BuildTarget.Android, GunModelBundle.EditorPlatformFolder);
        }

        // Devuelve la carpeta donde quedó el bundle.
        private static string Build(BuildTarget target, string platformFolder)
        {
            string output = Path.Combine(GunModelBundle.OutputRoot, platformFolder);
            Directory.CreateDirectory(output);

            AssetBundleBuild build = CreateBuild(out HashSet<string> modelNames);
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
                output,
                new[] { build },
                BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode,
                target);

            if (manifest == null)
            {
                throw new BuildFailedException($"No se pudo armar el AssetBundle '{GunModelBundle.BundleName}' para {target}.");
            }

            Debug.Log($"AssetBundle '{GunModelBundle.BundleName}' para {target}: {modelNames.Count} modelos en {Path.GetFullPath(output)}");
            ValidateCatalogs(modelNames);
            return output;
        }

        private static AssetBundleBuild CreateBuild(out HashSet<string> modelNames)
        {
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { ModelsFolder });
            var assetNames = new List<string>(guids.Length);
            var addressableNames = new List<string>(guids.Length);
            modelNames = new HashSet<string>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string modelName = Path.GetFileNameWithoutExtension(path);

                if (!modelNames.Add(modelName))
                {
                    throw new BuildFailedException($"Hay dos modelos llamados '{modelName}' en {ModelsFolder}; el bundle los pide por nombre.");
                }

                assetNames.Add(path);
                addressableNames.Add(modelName);
            }

            if (assetNames.Count == 0)
            {
                throw new BuildFailedException($"No hay modelos en {ModelsFolder} para el AssetBundle '{GunModelBundle.BundleName}'.");
            }

            return new AssetBundleBuild
            {
                assetBundleName = GunModelBundle.BundleName,
                assetNames = assetNames.ToArray(),
                addressableNames = addressableNames.ToArray()
            };
        }

        // Avisa de las armas del catálogo cuyo Model Name no está en el bundle (no frena el build).
        private static void ValidateCatalogs(HashSet<string> modelNames)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:ShopCatalog"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ShopCatalog catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(path);

                foreach (ShopItem gun in catalog.GetItems(ShopCategory.Guns))
                {
                    if (gun == null)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(gun.ModelName))
                    {
                        Debug.LogWarning($"{gun.name} no tiene Model Name: la tienda no va a poder mostrar su modelo.", gun);
                    }
                    else if (!modelNames.Contains(gun.ModelName))
                    {
                        Debug.LogWarning($"{gun.name} pide el modelo '{gun.ModelName}', que no está en {ModelsFolder}.", gun);
                    }
                }
            }
        }
    }

    // Si se da Play sin haber armado el bundle del Editor, lo arma antes de entrar.
    [InitializeOnLoad]
    internal static class GunBundlePlayModeCheck
    {
        static GunBundlePlayModeCheck()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode || File.Exists(GunModelBundle.BundlePath))
            {
                return;
            }

            Debug.Log("No existe el AssetBundle de armas para el Editor; armándolo antes de entrar a Play.");

            try
            {
                GunBundleBuilder.BuildForEditor();
            }
            catch (BuildFailedException e)
            {
                Debug.LogError($"{e.Message} La tienda no va a mostrar el arma en esta sesión de Play. " +
                               "(Si el motivo no está en este mensaje, está más arriba en la consola.)");
            }
        }
    }
}
