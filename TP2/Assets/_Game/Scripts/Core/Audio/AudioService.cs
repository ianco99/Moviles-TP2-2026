using System.Collections;
using System.Collections.Generic;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GunJump.Core
{
    // Facade del audio: el resto del juego pide un sonido por su SoundId y no sabe de AudioSource,
    // clips ni fundidos. Vive toda la sesión en un objeto propio que no se va con ninguna escena.
    public sealed class AudioService : IService
    {
        // Varias fuentes en ronda: el tono de un disparo no cambia el de los sonidos que siguen sonando.
        const int SfxSourceCount = 4;

        // Lo que el jugador eligió queda guardado entre sesiones. Sin nada guardado, todo suena.
        const string SfxEnabledKey = "Audio.Sfx";
        const string MusicEnabledKey = "Audio.Music";

        readonly AudioLibrary library;
        readonly MonoBehaviour runner;
        readonly AudioListener listener;
        readonly AudioSource musicSource;
        readonly AudioSource[] sfxSources = new AudioSource[SfxSourceCount];
        readonly AudioLibrary.SoundEntry[] sounds;
        readonly List<AudioListener> sceneListeners = new List<AudioListener>(2);
        readonly EventBus.EventCallback<LevelLoadFinishedEvent> loadFinishedCallback;

        AudioClip targetMusic;
        Coroutine musicFade;
        int nextSfxSource;
        bool sfxEnabled;
        bool musicEnabled;

        public AudioService(EventBus eventBus, AudioLibrary audioLibrary)
        {
            library = audioLibrary;
            runner = CoroutineHost.Create(nameof(AudioService));
            GameObject host = runner.gameObject;

            // El único oído del juego: durante una carga la escena vieja ya se fue y la nueva todavía no llegó,
            // y con el listener en una cámara de escena la música se cortaría.
            listener = host.AddComponent<AudioListener>();

            musicSource = AddSource(host);
            musicSource.loop = true;
            musicSource.volume = 0f;

            for (int i = 0; i < sfxSources.Length; i++)
                sfxSources[i] = AddSource(host);

            sounds = BuildTable(audioLibrary);

            sfxEnabled = ReadEnabled(SfxEnabledKey);
            musicEnabled = ReadEnabled(MusicEnabledKey);
            musicSource.mute = !musicEnabled;

            loadFinishedCallback = OnLevelLoadFinished;
            eventBus.Subscribe(loadFinishedCallback);
            SceneManager.sceneLoaded += OnSceneLoaded;

            DisableLoadedSceneListeners();
            runner.StartCoroutine(Begin());
        }

        public bool IsPersistance => true;

        public bool IsEnabled(AudioChannel channel)
        {
            return channel == AudioChannel.Music ? musicEnabled : sfxEnabled;
        }

        public void SetEnabled(AudioChannel channel, bool enabled)
        {
            if (channel == AudioChannel.Music)
            {
                musicEnabled = enabled;
                // Silenciada sigue avanzando: al volver a prenderla no arranca de cero ni corta un fundido.
                musicSource.mute = !enabled;
                WriteEnabled(MusicEnabledKey, enabled);
                return;
            }

            sfxEnabled = enabled;
            if (!enabled)
            {
                foreach (AudioSource source in sfxSources)
                    source.Stop();
            }

            WriteEnabled(SfxEnabledKey, enabled);
        }

        public void Play(SoundId id, float volumeScale = 1f)
        {
            if (!sfxEnabled)
                return;

            int index = (int)id;
            if (index < 0 || index >= sounds.Length)
                return;

            AudioLibrary.SoundEntry entry = sounds[index];
            if (entry.clip == null)
                return;

            AudioSource source = sfxSources[nextSfxSource];
            nextSfxSource = (nextSfxSource + 1) % sfxSources.Length;

            source.pitch = 1f + Random.Range(-entry.pitchVariation, entry.pitchVariation);
            source.PlayOneShot(entry.clip, entry.volume * volumeScale);
        }

        static bool ReadEnabled(string key)
        {
            return PlayerPrefs.GetInt(key, 1) != 0;
        }

        static void WriteEnabled(string key, bool enabled)
        {
            PlayerPrefs.SetInt(key, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        static AudioSource AddSource(GameObject host)
        {
            AudioSource source = host.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }

        static AudioLibrary.SoundEntry[] BuildTable(AudioLibrary audioLibrary)
        {
            int size = 0;
            foreach (SoundId id in System.Enum.GetValues(typeof(SoundId)))
                size = Mathf.Max(size, (int)id + 1);

            var table = new AudioLibrary.SoundEntry[size];
            foreach (AudioLibrary.SoundEntry entry in audioLibrary.Sounds)
            {
                int index = (int)entry.id;
                if (index >= 0 && index < size)
                    table[index] = entry;
            }

            return table;
        }

        // Un frame después del arranque la escena inicial ya terminó de cargar: se puede leer cuál es
        // y apagar el listener de su cámara si sceneLoaded no llegó a avisar.
        IEnumerator Begin()
        {
            yield return null;
            DisableLoadedSceneListeners();
            PlayMusicForActiveScene();
        }

        void OnLevelLoadFinished(in LevelLoadFinishedEvent e)
        {
            PlayMusicForActiveScene();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            DisableSceneListeners(scene);
        }

        void DisableLoadedSceneListeners()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                DisableSceneListeners(SceneManager.GetSceneAt(i));
        }

        void DisableSceneListeners(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                root.GetComponentsInChildren(true, sceneListeners);
                foreach (AudioListener sceneListener in sceneListeners)
                {
                    if (sceneListener != listener)
                        sceneListener.enabled = false;
                }
            }

            sceneListeners.Clear();
        }

        void PlayMusicForActiveScene()
        {
            PlayMusic(library.GetMusicFor(SceneManager.GetActiveScene().name));
        }

        // Si el tema pedido ya es el que suena (reintentar un nivel, pasar al siguiente), sigue sin cortarse.
        void PlayMusic(AudioClip clip)
        {
            if (clip == targetMusic)
                return;

            targetMusic = clip;
            if (musicFade != null)
                runner.StopCoroutine(musicFade);
            musicFade = runner.StartCoroutine(CrossfadeMusic(clip));
        }

        IEnumerator CrossfadeMusic(AudioClip clip)
        {
            if (musicSource.isPlaying)
                yield return FadeMusicTo(0f);

            musicSource.Stop();
            musicSource.clip = clip;
            if (clip == null)
                yield break;

            musicSource.Play();
            yield return FadeMusicTo(library.MusicVolume);
        }

        // Con tiempo sin escalar: el fundido sigue aunque la carga o la pausa tengan el tiempo detenido.
        IEnumerator FadeMusicTo(float target)
        {
            float start = musicSource.volume;
            float duration = library.MusicFadeTime;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(start, target, elapsed / duration);
                yield return null;
            }

            musicSource.volume = target;
        }
    }
}
