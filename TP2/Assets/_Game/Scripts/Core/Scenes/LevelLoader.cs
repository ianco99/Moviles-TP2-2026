using System.Collections;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GunJump.Core
{
    // Todas las cargas de nivel pasan por acá: desde el menú (con pantalla de carga) y al reintentar (sin ella).
    // El nivel nuevo se muestra recién cuando todo lo que pidió Hold terminó de cargar; hasta entonces
    // está tapado y con el tiempo detenido. Avisa cada paso en el EventBus (ver LevelLoadEvents).
    public sealed class LevelLoader : IService
    {
        // Unity deja el progreso en 0.9 hasta que se permite activar la escena.
        const float ReadyProgress = 0.9f;

        // Parte de la barra que corresponde a cargar la escena; el resto es lo que piden los Hold.
        const float SceneProgressShare = 0.8f;

        // Si algo nunca suelta la pantalla de carga, el nivel se muestra igual para no quedar trabados.
        const float MaxHoldSeconds = 15f;

        readonly EventBus eventBus;
        readonly string loadingScene;
        readonly MonoBehaviour runner;
        int holds;

        public LevelLoader(EventBus eventBus, string loadingSceneName)
        {
            this.eventBus = eventBus;
            loadingScene = loadingSceneName;
            // El menú, el nivel y la escena de carga se descargan en el medio de la carga: las corrutinas
            // corren en un objeto que no se va con ninguno de ellos.
            runner = CoroutineHost.Create(nameof(LevelLoader));
        }

        public bool IsPersistance => true;
        public bool IsLoading { get; private set; }

        // Va a un nivel (o al menú) pasando por la pantalla de carga.
        public void Load(int buildIndex)
        {
            if (IsLoading)
                return;

            IsLoading = true;
            runner.StartCoroutine(LoadRoutine(buildIndex));
        }

        // Vuelve a cargar el nivel actual sin pantalla de carga: el nivel viejo se sigue viendo hasta que el nuevo está listo.
        public void Restart()
        {
            if (IsLoading)
                return;

            IsLoading = true;
            runner.StartCoroutine(LoadInPlaceRoutine(SceneManager.GetActiveScene().buildIndex));
        }

        // Lo llama, al recibir LevelSceneLoadedEvent, lo que tenga que cargar algo antes de que se vea el nivel.
        // Cada Hold necesita su Release.
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
            while (load.progress < ReadyProgress)
            {
                view?.SetProgress(load.progress / ReadyProgress * SceneProgressShare);
                yield return null;
            }

            yield return Activate(load);
            // La luz y el cielo salen de la escena activa.
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(buildIndex));
            view?.SetProgress(SceneProgressShare);

            yield return WaitUntilReady();
            view?.SetProgress(1f);

            yield return SceneManager.UnloadSceneAsync(loading);
            Finish();
        }

        // Reemplaza la escena actual sin pantalla de carga: la vieja se sigue viendo hasta que la nueva se activa.
        IEnumerator LoadInPlaceRoutine(int buildIndex)
        {
            eventBus.Raise<LevelLoadStartedEvent>();

            AsyncOperation load = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
            load.allowSceneActivation = false;
            while (load.progress < ReadyProgress)
                yield return null;

            yield return Activate(load);
            yield return WaitUntilReady();
            Finish();
        }

        // El nivel arranca congelado: mientras se termina de preparar, la física no lo mueve.
        // Las cargas asíncronas no dependen de timeScale.
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

            float deadline = Time.realtimeSinceStartup + MaxHoldSeconds;
            while (holds > 0)
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    Debug.LogError($"LevelLoader: después de {MaxHoldSeconds} s todavía hay {holds} Hold sin Release; se muestra el nivel igual.");
                    holds = 0;
                    break;
                }

                yield return null;
            }
        }

        void Finish()
        {
            Time.timeScale = 1f;
            IsLoading = false;
            eventBus.Raise<LevelLoadFinishedEvent>();
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
