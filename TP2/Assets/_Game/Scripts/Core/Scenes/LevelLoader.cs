using System.Collections;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GunJump.Core
{
    public sealed class LevelLoader : IService
    {
        const float READY_PROGRESS = 0.9f;
        const float SCENE_PROGRESS_SHARE = 0.8f;
        const float MAX_HOLD_SECONDS = 15f;

        readonly EventBus eventBus;
        readonly string loadingScene;
        readonly MonoBehaviour runner;
        int holds;

        bool isLoading;
        public bool IsLoading => isLoading;

        public LevelLoader(EventBus eventBus, string loadingSceneName)
        {
            this.eventBus = eventBus;
            loadingScene = loadingSceneName;
            runner = CoroutineHost.Create(nameof(LevelLoader));
        }

        public bool IsPersistance => true;

        public void Load(int buildIndex)
        {
            if (IsLoading)
                return;

            isLoading = true;
            runner.StartCoroutine(LoadRoutine(buildIndex));
        }

        public void Restart()
        {
            if (IsLoading)
                return;

            isLoading = true;
            runner.StartCoroutine(LoadInPlaceRoutine(SceneManager.GetActiveScene().buildIndex));
        }

        public void Hold()
        {
            holds++;
        }

        public void Release()
        {
            if (holds > 0)
                holds--;
        }

        IEnumerator LoadRoutine(int buildIndex)
        {
            if (!Application.CanStreamedLevelBeLoaded(loadingScene))
            {
                Debug.LogError($"LevelLoader: la escena '{loadingScene}' no está en Build Settings; se carga sin pantalla de carga. " +
                               "Creala con GunJump > Build > Loading Scene.");
                yield return LoadInPlaceRoutine(buildIndex);
                yield break;
            }

            eventBus.Raise<LevelLoadStartedEvent>();
            Scene previous = SceneManager.GetActiveScene();

            yield return SceneManager.LoadSceneAsync(loadingScene, LoadSceneMode.Additive);
            Scene loading = SceneManager.GetSceneByName(loadingScene);
            ILoadingView view = FindInScene<ILoadingView>(loading);
            view?.SetProgress(0f);

            yield return SceneManager.UnloadSceneAsync(previous);
            yield return Resources.UnloadUnusedAssets();

            AsyncOperation load = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            load.allowSceneActivation = false;
            while (load.progress < READY_PROGRESS)
            {
                view?.SetProgress(load.progress / READY_PROGRESS * SCENE_PROGRESS_SHARE);
                yield return null;
            }

            yield return Activate(load);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(buildIndex));
            view?.SetProgress(SCENE_PROGRESS_SHARE);

            yield return WaitUntilReady();
            view?.SetProgress(1f);

            yield return SceneManager.UnloadSceneAsync(loading);
            Finish();
        }

        IEnumerator LoadInPlaceRoutine(int buildIndex)
        {
            eventBus.Raise<LevelLoadStartedEvent>();

            AsyncOperation load = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
            load.allowSceneActivation = false;
            while (load.progress < READY_PROGRESS)
                yield return null;

            yield return Activate(load);
            yield return WaitUntilReady();
            Finish();
        }

        static IEnumerator Activate(AsyncOperation load)
        {
            Time.timeScale = 0f;
            load.allowSceneActivation = true;
            while (!load.isDone)
                yield return null;
        }

        IEnumerator WaitUntilReady()
        {
            holds = 0;
            eventBus.Raise<LevelSceneLoadedEvent>();

            float deadline = Time.realtimeSinceStartup + MAX_HOLD_SECONDS;
            while (holds > 0)
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    Debug.LogError($"LevelLoader: después de {MAX_HOLD_SECONDS} s todavía hay {holds} Hold sin Release; se muestra el nivel igual.");
                    holds = 0;
                    break;
                }

                yield return null;
            }
        }

        void Finish()
        {
            Time.timeScale = 1f;
            isLoading = false;
            eventBus.Raise<LevelLoadFinishedEvent>();
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
