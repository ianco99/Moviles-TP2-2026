using System.Collections.Generic;
using System.IO;
using _Game.Scripts.Shop;
using GunJump.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace GunJump.EditorTools
{
    [InitializeOnLoad]
    public sealed class GunBundleBuilder : BuildPlayerProcessor
    {
        internal const string SHOT_VFX_FOLDER = "Assets/_Game/Prefabs/ShotsVFX";
        internal const string BULLETS_FOLDER = "Assets/_Game/Prefabs/Bullets";

        private static string s_preparedOutput;

        static GunBundleBuilder()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(BuildBundlesThenPlayer);
        }

        internal static readonly BundleSource[] Sources =
        {
            new BundleSource(ShopCategory.Guns, "Assets/_Game/Art/Models", "t:Model", required: true),
            new BundleSource(ShopCategory.GunSkins, "Assets/_Game/Art/Materials/Skins", "t:Material", required: false),
            new BundleSource(ShopCategory.Bullets, BULLETS_FOLDER, "t:Prefab", required: false),
            new BundleSource(ShopCategory.ShotVFX, SHOT_VFX_FOLDER, "t:Prefab", required: false),
            new BundleSource(ContentLibrary.PREFABS_BUNDLE, "Assets/_Game/Prefabs/Runtime", "t:Prefab", required: true),
        };

        public override int callbackOrder => 0;

        private static void BuildBundlesThenPlayer(BuildPlayerOptions options)
        {
            try
            {
                s_preparedOutput = Build(options.target, ContentBundle.PLAYER_PLATFORM_FOLDER);
            }
            catch (BuildFailedException e)
            {
                throw new BuildPlayerWindow.BuildMethodException(e.Message);
            }

            try
            {
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
            }
            finally
            {
                s_preparedOutput = null;
            }
        }

        public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            if (s_preparedOutput == null)
            {
                throw new BuildFailedException(
                    "Los AssetBundles del arma no se armaron para este build. Armá el jugador con el botón Build " +
                    "de File > Build Profiles (no se pueden armar adentro del build: hacen caer al Editor).");
            }

            buildPlayerContext.AddAdditionalPathToStreamingAssets(s_preparedOutput, ContentBundle.STREAMING_FOLDER);
        }

        [MenuItem("GunJump/Asset Bundles/Build Gun Bundles (Editor)")]
        private static void BuildForEditorMenu()
        {
            try
            {
                string output = BuildForEditor();
                EditorUtility.RevealInFinder(Path.Combine(output, ShopBundles.GUNS));
            }
            catch (BuildFailedException e)
            {
                EditorUtility.DisplayDialog("GunJump", e.Message, "Entendido");
            }
        }

        public static string BuildForEditor()
        {
            BuildTarget active = EditorUserBuildSettings.activeBuildTarget;

            if (active != BuildTarget.Android)
            {
                throw new BuildFailedException(
                    $"La plataforma activa es {active}, no Android. Cambiá a Android en File > Build Profiles " +
                    "y volvé a armar los bundles del arma.");
            }

            return Build(BuildTarget.Android, ContentBundle.EDITOR_PLATFORM_FOLDER);
        }

        private static string Build(BuildTarget target, string platformFolder)
        {
            string output = Path.Combine(ContentBundle.OUTPUT_ROOT, platformFolder);
            Directory.CreateDirectory(output);

            List<AssetBundleBuild> builds = new List<AssetBundleBuild>(Sources.Length);
            Dictionary<string, HashSet<string>> contents = new Dictionary<string, HashSet<string>>();

            foreach (BundleSource source in Sources)
            {
                HashSet<string> assetNames = source.FindAssetNames(out AssetBundleBuild build);
                contents.Add(source.BundleName, assetNames);

                if (assetNames.Count > 0)
                {
                    builds.Add(build);
                }
                else if (source.Required)
                {
                    throw new BuildFailedException($"No hay assets en {source.Folder} para el AssetBundle '{source.BundleName}'.");
                }
                else
                {
                    Debug.Log($"AssetBundle '{source.BundleName}': no hay contenido en {source.Folder}; " +
                              "los items de la tienda que lo usan toman lo que el arma trae por defecto.");
                }
            }

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
                output,
                builds.ToArray(),
                BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode,
                target);

            if (manifest == null)
            {
                throw new BuildFailedException($"No se pudieron armar los AssetBundles del arma para {target}.");
            }

            foreach (AssetBundleBuild build in builds)
            {
                Debug.Log($"AssetBundle '{build.assetBundleName}' para {target}: {build.assetNames.Length} assets en {Path.GetFullPath(output)}");
            }

            ValidateCatalogs(contents);
            return output;
        }

        private static void ValidateCatalogs(Dictionary<string, HashSet<string>> contents)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:ShopCatalog"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ShopCatalog catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(path);

                foreach (BundleSource source in Sources)
                {
                    if (!source.IsShopContent)
                    {
                        continue;
                    }

                    HashSet<string> assetNames = contents[source.BundleName];

                    if (assetNames.Count == 0)
                    {
                        continue;
                    }

                    List<string> problems = new List<string>();

                    foreach (ShopItem item in catalog.GetItems(source.Category))
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(item.AssetName))
                        {
                            problems.Add($"{item.name} (sin Asset Name)");
                        }
                        else if (!assetNames.Contains(item.AssetName))
                        {
                            problems.Add($"{item.name} ('{item.AssetName}' no está en {source.Folder})");
                        }
                    }

                    if (problems.Count > 0)
                    {
                        Debug.LogWarning($"{catalog.name}, {source.Category}: estos items van a usar lo que el arma trae por defecto: " +
                                         string.Join(", ", problems), catalog);
                    }
                }
            }
        }

        internal sealed class BundleSource
        {
            public readonly ShopCategory Category;
            public readonly string BundleName;
            public readonly bool IsShopContent;
            public readonly string Folder;
            public readonly string Filter;
            public readonly bool Required;

            public BundleSource(ShopCategory category, string folder, string filter, bool required)
            {
                Category = category;
                BundleName = ShopBundles.For(category);
                IsShopContent = true;
                Folder = folder;
                Filter = filter;
                Required = required;
            }

            public BundleSource(string bundleName, string folder, string filter, bool required)
            {
                BundleName = bundleName;
                IsShopContent = false;
                Folder = folder;
                Filter = filter;
                Required = required;
            }

            public bool HasContent => AssetDatabase.IsValidFolder(Folder) &&
                                      AssetDatabase.FindAssets(Filter, new[] { Folder }).Length > 0;

            public HashSet<string> FindAssetNames(out AssetBundleBuild build)
            {
                HashSet<string> names = new HashSet<string>();
                List<string> assetPaths = new List<string>();
                List<string> addressableNames = new List<string>();

                if (AssetDatabase.IsValidFolder(Folder))
                {
                    foreach (string guid in AssetDatabase.FindAssets(Filter, new[] { Folder }))
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        string assetName = Path.GetFileNameWithoutExtension(path);

                        if (!names.Add(assetName))
                        {
                            throw new BuildFailedException($"Hay dos assets llamados '{assetName}' en {Folder}; el bundle '{BundleName}' los pide por nombre.");
                        }

                        assetPaths.Add(path);
                        addressableNames.Add(assetName);
                    }
                }

                build = new AssetBundleBuild
                {
                    assetBundleName = BundleName,
                    assetNames = assetPaths.ToArray(),
                    addressableNames = addressableNames.ToArray()
                };

                return names;
            }
        }
    }

    [InitializeOnLoad]
    internal static class GunBundlePlayModeCheck
    {
        static GunBundlePlayModeCheck()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode || !IsAnyBundleMissing())
            {
                return;
            }

            Debug.Log("Falta algún AssetBundle del arma para el Editor; armándolos antes de entrar a Play.");

            try
            {
                GunBundleBuilder.BuildForEditor();
            }
            catch (BuildFailedException e)
            {
                Debug.LogError($"{e.Message} La tienda y el arma no van a mostrar su contenido en esta sesión de Play. " +
                               "(Si el motivo no está en este mensaje, está más arriba en la consola.)");
            }
        }

        private static bool IsAnyBundleMissing()
        {
            if (!File.Exists(ContentBundle.PathOf(ContentBundle.ManifestBundleName)))
            {
                return true;
            }

            foreach (GunBundleBuilder.BundleSource source in GunBundleBuilder.Sources)
            {
                if (source.HasContent && !File.Exists(ContentBundle.PathOf(source.BundleName)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
