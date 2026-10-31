using GunJump.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace GunJump.EditorTools
{
    // Al dar Play desde una escena del juego, arranca por Boot y Boot carga la escena que estaba abierta.
    // Sin esto, una escena de nivel abierta sola no tendría quién le entregue los sistemas.
    [InitializeOnLoad]
    public static class PlayFromBoot
    {
        const string MenuPath = "GunJump/Play From Boot";
        const string PrefKey = "GunJump.PlayFromBoot";

        static PlayFromBoot()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static bool Enabled => EditorPrefs.GetBool(PrefKey, true);

        [MenuItem(MenuPath)]
        static void Toggle()
        {
            EditorPrefs.SetBool(PrefKey, !Enabled);
        }

        [MenuItem(MenuPath, true)]
        static bool ValidateToggle()
        {
            Menu.SetChecked(MenuPath, Enabled);
            return true;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode)
                return;

            Scene active = SceneManager.GetActiveScene();
            var boot = AssetDatabase.LoadAssetAtPath<SceneAsset>(GameSceneBuilder.BootScenePath);
            bool route = Enabled && boot != null && IsGameScene(active.path);

            // Las escenas que no son del juego (pruebas, samples) se abren tal cual.
            EditorSceneManager.playModeStartScene = route ? boot : null;

            bool isContent = route
                             && active.path != GameSceneBuilder.BootScenePath
                             && active.path != GameSceneBuilder.LoadingScenePath;
            SessionState.SetString(GameBootstrap.EditorStartSceneKey, isContent ? active.name : string.Empty);
        }

        static bool IsGameScene(string path)
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled && scene.path == path)
                    return path.StartsWith(GameSceneBuilder.ScenesFolder);
            }

            return false;
        }
    }
}
