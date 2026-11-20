using GunJump.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace GunJump.EditorTools
{
    [InitializeOnLoad]
    public static class PlayFromBoot
    {
        const string MENU_PATH = "GunJump/Play From Boot";
        const string PREF_KEY = "GunJump.PlayFromBoot";

        static PlayFromBoot()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static bool Enabled => EditorPrefs.GetBool(PREF_KEY, true);

        [MenuItem(MENU_PATH)]
        static void Toggle()
        {
            EditorPrefs.SetBool(PREF_KEY, !Enabled);
        }

        [MenuItem(MENU_PATH, true)]
        static bool ValidateToggle()
        {
            Menu.SetChecked(MENU_PATH, Enabled);
            return true;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode)
                return;

            Scene active = SceneManager.GetActiveScene();
            SceneAsset boot = AssetDatabase.LoadAssetAtPath<SceneAsset>(GameSceneBuilder.BOOT_SCENE_PATH);
            bool route = Enabled && boot != null && IsGameScene(active.path);

            EditorSceneManager.playModeStartScene = route ? boot : null;

            bool isContent = route
                             && active.path != GameSceneBuilder.BOOT_SCENE_PATH
                             && active.path != GameSceneBuilder.LOADING_SCENE_PATH;
            SessionState.SetString(GameBootstrap.EDITOR_START_SCENE_KEY, isContent ? active.name : string.Empty);
        }

        static bool IsGameScene(string path)
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled && scene.path == path)
                    return path.StartsWith(GameSceneBuilder.SCENES_FOLDER);
            }

            return false;
        }
    }
}
