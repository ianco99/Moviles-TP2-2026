using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GunJump.Core
{
    // Cambia de escena de contenido de forma asíncrona. La escena Boot queda siempre cargada;
    // el contenido y la pantalla de carga entran y salen como escenas aditivas.
    public sealed class SceneLoader
    {
        // Unity deja el progreso en 0.9 hasta que se permite activar la escena.
        const float ReadyProgress = 0.9f;

        readonly MonoBehaviour runner;
        readonly string loadingScene;
        GameContext context;

        public SceneLoader(MonoBehaviour coroutineRunner, string loadingSceneName)
        {
            runner = coroutineRunner;
            loadingScene = loadingSceneName;
        }

        public string Current { get; private set; }
        public bool IsLoading { get; private set; }

        public void Bind(GameContext gameContext)
        {
            context = gameContext;
        }

        public void Load(string sceneName)
        {
            if (IsLoading)
                return;

            runner.StartCoroutine(LoadRoutine(sceneName));
        }

        IEnumerator LoadRoutine(string sceneName)
        {
            IsLoading = true;
            // Se puede llegar acá desde el menú de pausa, con el tiempo detenido.
            Time.timeScale = 1f;

            yield return SceneManager.LoadSceneAsync(loadingScene, LoadSceneMode.Additive);
            ILoadingView view = FindInScene<ILoadingView>(SceneManager.GetSceneByName(loadingScene));
            view?.SetProgress(0f);

            if (Current != null)
            {
                yield return SceneManager.UnloadSceneAsync(Current);
                Current = null;
            }

            yield return Resources.UnloadUnusedAssets();

            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            load.allowSceneActivation = false;
            while (load.progress < ReadyProgress)
            {
                view?.SetProgress(load.progress / ReadyProgress);
                yield return null;
            }

            view?.SetProgress(1f);
            load.allowSceneActivation = true;
            while (!load.isDone)
                yield return null;

            Scene scene = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(scene);
            Current = sceneName;

            SceneEntryPoint entry = FindInScene<SceneEntryPoint>(scene);
            if (entry != null)
                entry.Enter(context);

            yield return SceneManager.UnloadSceneAsync(loadingScene);
            IsLoading = false;
        }

        static T FindInScene<T>(Scene scene) where T : class
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T found = root.GetComponentInChildren<T>(true);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}
