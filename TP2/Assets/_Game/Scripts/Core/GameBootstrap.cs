using System.IO;
using UnityEngine;

namespace GunJump.Core
{
    // Raíz de composición: crea los sistemas una sola vez y arranca la primera escena.
    // Vive en la escena Boot, que nunca se descarga.
    public sealed class GameBootstrap : MonoBehaviour
    {
        // El Editor deja acá el nombre de la escena que estaba abierta al dar Play.
        public const string EditorStartSceneKey = "GunJump.StartScene";

        const int TargetFrameRate = 60;
        const string SaveFileName = "progress.json";

        [SerializeField] EconomyConfig economyConfig;
        [SerializeField] LevelCatalog levels;
        [SerializeField] string loadingScene = "Loading";

        GameContext context;

        void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;

            var progress = new PlayerProgress();
            var save = new SaveSystem(Path.Combine(Application.persistentDataPath, SaveFileName));
            ProgressSnapshot saved = save.Load();
            if (saved != null)
                progress.Restore(saved);

            var loader = new SceneLoader(this, loadingScene);
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
            string requested = UnityEditor.SessionState.GetString(EditorStartSceneKey, string.Empty);
            if (!string.IsNullOrEmpty(requested))
                return requested;
#endif
            return levels.First;
        }
    }
}
