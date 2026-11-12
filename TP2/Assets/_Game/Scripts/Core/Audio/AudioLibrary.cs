using System;
using System.Collections.Generic;
using UnityEngine;

namespace GunJump.Core
{
    // Todos los clips y volúmenes del juego. Para cambiar un sonido alcanza con reasignar su clip acá.
    [CreateAssetMenu(menuName = "GunJump/Audio Library", fileName = "AudioLibrary")]
    public sealed class AudioLibrary : ScriptableObject
    {
        [Serializable]
        public struct SoundEntry
        {
            public SoundId id;
            public AudioClip clip;
            [Range(0f, 1f)]
            public float volume;
            [Tooltip("Cuánto puede variar el tono al azar en cada reproducción, para que no suene repetido. 0 lo deja fijo.")]
            [Range(0f, 0.5f)]
            public float pitchVariation;
        }

        [Serializable]
        public struct SceneMusic
        {
            public string sceneName;
            public AudioClip clip;
        }

        [Header("Efectos")]
        [SerializeField] SoundEntry[] sounds = Array.Empty<SoundEntry>();

        [Header("Música")]
        [Tooltip("Música de escenas puntuales, por nombre de escena.")]
        [SerializeField] SceneMusic[] sceneMusic = Array.Empty<SceneMusic>();
        [Tooltip("Música de cualquier escena que no esté en la lista: los niveles.")]
        [SerializeField] AudioClip defaultMusic;
        [Range(0f, 1f)]
        [SerializeField] float musicVolume = 0.6f;
        [Tooltip("Segundos que tarda la música en apagarse y en volver a subir al cambiar de tema.")]
        [Min(0f)]
        [SerializeField] float musicFadeTime = 0.75f;

        public IReadOnlyList<SoundEntry> Sounds => sounds;
        public float MusicVolume => musicVolume;
        public float MusicFadeTime => musicFadeTime;

        public AudioClip GetMusicFor(string sceneName)
        {
            foreach (SceneMusic entry in sceneMusic)
            {
                if (entry.sceneName == sceneName)
                    return entry.clip;
            }

            return defaultMusic;
        }
    }
}
