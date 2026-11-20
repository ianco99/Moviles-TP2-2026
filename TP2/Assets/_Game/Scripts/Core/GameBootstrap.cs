using System.IO;
using UnityEngine;

namespace GunJump.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        public const string EDITOR_START_SCENE_KEY = "GunJump.StartScene";

        const int TARGET_FRAME_RATE = 60;
        const string SAVE_FILE_NAME = "progress.json";

        [SerializeField] EconomyConfig economyConfig;
        [SerializeField] LevelCatalog levels;
        [SerializeField] string loadingScene = "Loading";

        GameContext context;

        void Awake()
        {
            Application.targetFrameRate = TARGET_FRAME_RATE;

            PlayerProgress progress = new PlayerProgress();
            SaveSystem save = new SaveSystem(Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME));
            ProgressSnapshot saved = save.Load();
            if (saved != null)
                progress.Restore(saved);

            SceneLoader loader = new SceneLoader(this, loadingScene);
            context = new GameContext(
                loader,
                new CommandInvoker(),
                progress,
                save,
                new Economy(progress, economyConfig),
                levels);
            loader.Bind(context);
        }

        void Start()
        {
            context.Loader.Load(StartScene());
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
                context.SaveProgress();
        }

        string StartScene()
        {
#if UNITY_EDITOR
            string requested = UnityEditor.SessionState.GetString(EDITOR_START_SCENE_KEY, string.Empty);
            if (!string.IsNullOrEmpty(requested))
                return requested;
#endif
            return levels.First;
        }
    }
}
