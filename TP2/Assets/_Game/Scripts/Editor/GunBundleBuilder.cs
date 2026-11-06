using System.Collections.Generic;
using System.IO;
using _Game.Scripts.Shop;
using GunJump.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace GunJump.EditorTools
{
    // Arma los AssetBundles del arma, uno por categoría de la tienda (ver Sources).
    // - Para Play mode: GunJump > Asset Bundles > Build Gun Bundles (Editor), o solo, al dar Play si falta alguno.
    // - Para el build: el botón Build (o Build And Run) los arma ANTES de empezar el build del jugador,
    //   y PrepareForBuild solo los copia a StreamingAssets. Así nunca hay que commitearlos ni se puede subir uno viejo.
    //
    // No se pueden armar adentro de PrepareForBuild: BuildPipeline.BuildAssetBundles abre y cierra su propio
    // BuildReport en medio del build del jugador, y el Editor se cae apenas el build sigue
    // (crash nativo en BuildReport::BeginBuildStep / AddMessage, Unity 6000.3).
    [InitializeOnLoad]
    public sealed class GunBundleBuilder : BuildPlayerProcessor
    {
        // Carpeta armada por BuildBundlesThenPlayer para el build en curso; PrepareForBuild la consume.
        private static string s_preparedOutput;

        static GunBundleBuilder()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(BuildBundlesThenPlayer);
        }

        // De dónde sale cada bundle. Solo los modelos de armas son obligatorios: una categoría
        // sin contenido no arma bundle y sus items usan lo que el arma trae por defecto.
        internal static readonly BundleSource[] Sources =
        {
            new BundleSource(ShopCategory.Guns, "Assets/_Game/Art/Models", "t:Model", required: true),
            new BundleSource(ShopCategory.GunSkins, "Assets/_Game/Art/Materials/Skins", "t:Material", required: false),
            new BundleSource(ShopCategory.Bullets, "Assets/_Game/Prefabs/Loadout/Bullets", "t:Prefab", required: false),
            new BundleSource(ShopCategory.ShotVFX, "Assets/_Game/Prefabs/Loadout/ShotVfx", "t:Prefab", required: false),
        };

        public override int callbackOrder => 0;

        // Reemplaza lo que hace el botón Build de File > Build Profiles: primero los bundles, después el jugador.
        // Son dos builds seguidos, no uno adentro del otro.
        private static void BuildBundlesThenPlayer(BuildPlayerOptions options)
        {
            try
            {
                s_preparedOutput = Build(options.target, ContentBundle.PlayerPlatformFolder);
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

            buildPlayerContext.AddAdditionalPathToStreamingAssets(s_preparedOutput, ContentBundle.StreamingFolder);
        }

        [MenuItem("GunJump/Asset Bundles/Build Gun Bundles (Editor)")]
        private static void BuildForEditorMenu()
        {
            try
            {
                string output = BuildForEditor();
                EditorUtility.RevealInFinder(Path.Combine(output, ShopBundles.Guns));
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
        // Los shaders de un bundle de Android no andan en el Editor; ContentBundle los reemplaza al cargar.
        public static string BuildForEditor()
        {
            BuildTarget active = EditorUserBuildSettings.activeBuildTarget;

            if (active != BuildTarget.Android)
            {
                throw new BuildFailedException(
                    $"La plataforma activa es {active}, no Android. Cambiá a Android en File > Build Profiles " +
                    "y volvé a armar los bundles del arma.");
            }

            return Build(BuildTarget.Android, ContentBundle.EditorPlatformFolder);
        }

        // Devuelve la carpeta donde quedaron los bundles.
        private static string Build(BuildTarget target, string platformFolder)
        {
            string output = Path.Combine(ContentBundle.OutputRoot, platformFolder);
            Directory.CreateDirectory(output);

            var builds = new List<AssetBundleBuild>(Sources.Length);
            var contents = new Dictionary<ShopCategory, HashSet<string>>();

            foreach (BundleSource source in Sources)
            {
                HashSet<string> assetNames = source.FindAssetNames(out AssetBundleBuild build);
                contents.Add(source.Category, assetNames);

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
                              $"los items de {source.Category} usan lo que el arma trae por defecto.");
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

        // Avisa de los items del catálogo cuyo Asset Name no está en el bundle de su categoría (no frena el build).
        // Las categorías sin contenido no avisan: todavía no se espera que sus items tengan Asset Name.
        private static void ValidateCatalogs(Dictionary<ShopCategory, HashSet<string>> contents)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:ShopCatalog"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ShopCatalog catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(path);

                foreach (BundleSource source in Sources)
                {
                    HashSet<string> assetNames = contents[source.Category];

                    if (assetNames.Count == 0)
                    {
                        continue;
                    }

                    var problems = new List<string>();

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
            public readonly string Folder;
            public readonly string Filter;
            public readonly bool Required;

            public BundleSource(ShopCategory category, string folder, string filter, bool required)
            {
                Category = category;
                Folder = folder;
                Filter = filter;
                Required = required;
            }

            public string BundleName => ShopBundles.For(Category);

            public bool HasContent => AssetDatabase.IsValidFolder(Folder) &&
                                      AssetDatabase.FindAssets(Filter, new[] { Folder }).Length > 0;

            // Devuelve los nombres con los que se piden los assets (el nombre del archivo sin extensión).
            public HashSet<string> FindAssetNames(out AssetBundleBuild build)
            {
                var names = new HashSet<string>();
                var assetPaths = new List<string>();
                var addressableNames = new List<string>();

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

    // Si se da Play sin haber armado alguno de los bundles del Editor, los arma antes de entrar.
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
            // Sin el manifest, ContentLibrary no sabe qué bundles cargar antes de cada uno.
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
