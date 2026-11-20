using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GunJump.Core
{
    public sealed class SceneLoader
    {
        const float READY_PROGRESS = 0.9f;

        readonly MonoBehaviour runner;
        readonly string loadingScene;
        GameContext context;

        string current;
        public string Current => current;

        bool isLoading;
        public bool IsLoading => isLoading;

        public SceneLoader(MonoBehaviour coroutineRunner, string loadingSceneName)
        {
            runner = coroutineRunner;
            loadingScene = loadingSceneName;
        }

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
            isLoading = true;
            Time.timeScale = 1f;

            yield return SceneManager.LoadSceneAsync(loadingScene, LoadSceneMode.Additive);
            ILoadingView view = FindInScene<ILoadingView>(SceneManager.GetSceneByName(loadingScene));
            view?.SetProgress(0f);

            if (Current != null)
            {
                yield return SceneManager.UnloadSceneAsync(Current);
                current = null;
            }

            yield return Resources.UnloadUnusedAssets();

            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            load.allowSceneActivation = false;
            while (load.progress < READY_PROGRESS)
            {
                view?.SetProgress(load.progress / READY_PROGRESS);
                yield return null;
            }

            view?.SetProgress(1f);
            load.allowSceneActivation = true;
            while (!load.isDone)
                yield return null;

            Scene scene = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(scene);
            current = sceneName;

            SceneEntryPoint entry = FindInScene<SceneEntryPoint>(scene);
            if (entry != null)
                entry.Enter(context);

            yield return SceneManager.UnloadSceneAsync(loadingScene);
            isLoading = false;
        }

        static ComponentType FindInScene<ComponentType>(Scene scene) where ComponentType : class
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                ComponentType found = root.GetComponentInChildren<ComponentType>(true);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}
