using System;
using System.IO;
using GunJump.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GunJump.EditorTools
{
    public static class AudioSetupTools
    {
        private const string AUDIO_FOLDER = "Assets/_Game/Audio";
        private const string DATA_FOLDER = "Assets/_Game/Data";
        private const string LIBRARY_FOLDER = DATA_FOLDER + "/Audio";
        private const string LIBRARY_PATH = LIBRARY_FOLDER + "/AudioLibrary.asset";
        private const string MAIN_MENU_SCENE_PATH = "Assets/_Game/Scenes/MainMenu.unity";
        private const string MAIN_MENU_SCENE_NAME = "MainMenu";
        private const string MENU_MUSIC_FILE = "music_menu";
        private const string GAMEPLAY_MUSIC_FILE = "music_gameplay";

        private const string BOOTSTRAPPER_TYPE_NAME = "Bootstrapper";
        private const string BOOTSTRAPPER_LIBRARY_FIELD = "audioLibrary";

        private const int SAMPLE_RATE = 44100;
        private const float ATTACK_SECONDS = 0.003f;
        private const float RELEASE_SECONDS = 0.01f;
        private const float PEAK_LEVEL = 0.85f;
        private const int NOISE_SEED = 1234;

        private enum Wave
        {
            Sine,
            Triangle,
            Square
        }

        private readonly struct SoundDefault
        {
            public readonly SoundId Id;
            public readonly string File;
            public readonly float Volume;
            public readonly float PitchVariation;

            public SoundDefault(SoundId id, string file, float volume, float pitchVariation)
            {
                Id = id;
                File = file;
                Volume = volume;
                PitchVariation = pitchVariation;
            }
        }

        private static readonly SoundDefault[] defaults =
        {
            new SoundDefault(SoundId.ButtonClick, "ui_click", 0.7f, 0f),
            new SoundDefault(SoundId.ShopTab, "ui_tab", 0.7f, 0f),
            new SoundDefault(SoundId.ShopBuy, "shop_buy", 0.8f, 0f),
            new SoundDefault(SoundId.ShopEquip, "shop_equip", 0.8f, 0f),
            new SoundDefault(SoundId.ShopDenied, "shop_denied", 0.7f, 0f),
            new SoundDefault(SoundId.Shot, "shot", 0.8f, 0.06f),
            new SoundDefault(SoundId.DryFire, "dry_fire", 0.6f, 0.03f),
            new SoundDefault(SoundId.GunImpact, "gun_impact", 0.9f, 0.08f),
            new SoundDefault(SoundId.BulletImpact, "bullet_impact", 0.5f, 0.1f),
            new SoundDefault(SoundId.EnemyHit, "enemy_hit", 0.7f, 0.06f),
            new SoundDefault(SoundId.EnemyDeath, "enemy_death", 0.8f, 0.05f),
            new SoundDefault(SoundId.MultiplierHit, "multiplier_hit", 0.8f, 0f),
            new SoundDefault(SoundId.CoinPickup, "coin_pickup", 0.7f, 0.04f),
            new SoundDefault(SoundId.LevelWin, "level_win", 0.9f, 0f),
            new SoundDefault(SoundId.LevelLose, "level_lose", 0.9f, 0f)
        };

        [MenuItem("GunJump/Audio/Setup Audio")]
        public static void SetupAudio()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("GunJump", "Salí de Play antes de correr Setup Audio.", "OK");
                return;
            }

            int created = CreateMissingClips();
            AudioLibrary library = CreateOrUpdateLibrary();
            bool assigned = AssignToBootstrapper(library);

            Selection.activeObject = library;
            EditorGUIUtility.PingObject(library);

            string pending = assigned
                ? "El Bootstrapper de MainMenu ya lo tiene asignado."
                : $"No se pudo asignar al Bootstrapper: abrí {MAIN_MENU_SCENE_PATH} y arrastrá el AudioLibrary a su campo 'Audio Library'.";
            Debug.Log($"Audio listo: {created} clips provisorios nuevos en {AUDIO_FOLDER}, biblioteca en {LIBRARY_PATH}. {pending}", library);
        }

        private static int CreateMissingClips()
        {
            Directory.CreateDirectory(AUDIO_FOLDER);
            int created = 0;

            foreach (SoundDefault sound in defaults)
            {
                if (WriteIfMissing(sound.File, () => RenderSound(sound.Id)))
                    created++;
            }

            if (WriteIfMissing(MENU_MUSIC_FILE, RenderMenuMusic))
                created++;
            if (WriteIfMissing(GAMEPLAY_MUSIC_FILE, RenderGameplayMusic))
                created++;

            AssetDatabase.Refresh();
            StreamMusic(MENU_MUSIC_FILE);
            StreamMusic(GAMEPLAY_MUSIC_FILE);
            return created;
        }

        private static string ClipPath(string file)
        {
            return $"{AUDIO_FOLDER}/{file}.wav";
        }

        private static bool WriteIfMissing(string file, Func<float[]> render)
        {
            string path = ClipPath(file);
            if (File.Exists(path))
                return false;

            WriteWav(path, Normalize(render()));
            return true;
        }

        private static void StreamMusic(string file)
        {
            if (!(AssetImporter.GetAtPath(ClipPath(file)) is AudioImporter importer))
                return;

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            if (settings.loadType == AudioClipLoadType.Streaming)
                return;

            settings.loadType = AudioClipLoadType.Streaming;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }

        private static AudioLibrary CreateOrUpdateLibrary()
        {
            AudioLibrary library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(LIBRARY_PATH);
            if (library == null)
            {
                if (!AssetDatabase.IsValidFolder(LIBRARY_FOLDER))
                    AssetDatabase.CreateFolder(DATA_FOLDER, "Audio");

                library = ScriptableObject.CreateInstance<AudioLibrary>();
                AssetDatabase.CreateAsset(library, LIBRARY_PATH);
            }

            SerializedObject serialized = new SerializedObject(library);
            SerializedProperty sounds = serialized.FindProperty("sounds");

            foreach (SoundDefault sound in defaults)
            {
                SerializedProperty entry = FindEntry(sounds, sound.Id);
                if (entry == null)
                {
                    sounds.arraySize++;
                    entry = sounds.GetArrayElementAtIndex(sounds.arraySize - 1);
                    entry.FindPropertyRelative("id").intValue = (int)sound.Id;
                    entry.FindPropertyRelative("clip").objectReferenceValue = null;
                    entry.FindPropertyRelative("volume").floatValue = sound.Volume;
                    entry.FindPropertyRelative("pitchVariation").floatValue = sound.PitchVariation;
                }

                SerializedProperty clip = entry.FindPropertyRelative("clip");
                if (clip.objectReferenceValue == null)
                    clip.objectReferenceValue = LoadClip(sound.File);
            }

            SerializedProperty sceneMusic = serialized.FindProperty("sceneMusic");
            if (sceneMusic.arraySize == 0)
            {
                sceneMusic.arraySize = 1;
                SerializedProperty menu = sceneMusic.GetArrayElementAtIndex(0);
                menu.FindPropertyRelative("sceneName").stringValue = MAIN_MENU_SCENE_NAME;
                menu.FindPropertyRelative("clip").objectReferenceValue = LoadClip(MENU_MUSIC_FILE);
            }

            SerializedProperty defaultMusic = serialized.FindProperty("defaultMusic");
            if (defaultMusic.objectReferenceValue == null)
                defaultMusic.objectReferenceValue = LoadClip(GAMEPLAY_MUSIC_FILE);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return library;
        }

        private static SerializedProperty FindEntry(SerializedProperty sounds, SoundId id)
        {
            for (int i = 0; i < sounds.arraySize; i++)
            {
                SerializedProperty entry = sounds.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("id").intValue == (int)id)
                    return entry;
            }

            return null;
        }

        private static AudioClip LoadClip(string file)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath(file));
        }

        private static bool AssignToBootstrapper(AudioLibrary library)
        {
            if (!File.Exists(MAIN_MENU_SCENE_PATH))
                return false;

            Scene scene = SceneManager.GetSceneByPath(MAIN_MENU_SCENE_PATH);
            bool wasOpen = scene.IsValid() && scene.isLoaded;
            if (!wasOpen)
                scene = EditorSceneManager.OpenScene(MAIN_MENU_SCENE_PATH, OpenSceneMode.Additive);

            bool assigned = false;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behaviour == null || behaviour.GetType().Name != BOOTSTRAPPER_TYPE_NAME)
                        continue;

                    SerializedObject serialized = new SerializedObject(behaviour);
                    SerializedProperty field = serialized.FindProperty(BOOTSTRAPPER_LIBRARY_FIELD);
                    if (field == null)
                        continue;

                    field.objectReferenceValue = library;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    assigned = true;
                }
            }

            if (assigned)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            if (!wasOpen)
                EditorSceneManager.CloseScene(scene, true);

            return assigned;
        }

        private static float[] RenderSound(SoundId id)
        {
            System.Random random = new System.Random(NOISE_SEED + (int)id);
            float[] b;

            switch (id)
            {
                case SoundId.ButtonClick:
                    b = NewBuffer(0.07f);
                    Tone(b, false, 0f, 0.07f, 950f, 620f, 0.8f, 55f, Wave.Sine);
                    return b;

                case SoundId.ShopTab:
                    b = NewBuffer(0.08f);
                    Tone(b, false, 0f, 0.08f, 560f, 820f, 0.8f, 40f, Wave.Sine);
                    return b;

                case SoundId.ShopBuy:
                    b = NewBuffer(0.5f);
                    Tone(b, false, 0f, 0.2f, Midi(76), Midi(76), 0.6f, 14f, Wave.Triangle);
                    Tone(b, false, 0.07f, 0.2f, Midi(81), Midi(81), 0.6f, 14f, Wave.Triangle);
                    Tone(b, false, 0.14f, 0.36f, Midi(88), Midi(88), 0.7f, 9f, Wave.Triangle);
                    Tone(b, false, 0.14f, 0.36f, Midi(100), Midi(100), 0.2f, 12f, Wave.Sine);
                    return b;

                case SoundId.ShopEquip:
                    b = NewBuffer(0.22f);
                    Noise(b, false, random, 0f, 0.025f, 0.5f, 120f, 0.2f);
                    Tone(b, false, 0f, 0.1f, Midi(72), Midi(72), 0.6f, 25f, Wave.Triangle);
                    Tone(b, false, 0.06f, 0.16f, Midi(79), Midi(79), 0.7f, 18f, Wave.Triangle);
                    return b;

                case SoundId.ShopDenied:
                    b = NewBuffer(0.3f);
                    Tone(b, false, 0f, 0.1f, 165f, 160f, 0.6f, 6f, Wave.Square);
                    Tone(b, false, 0.14f, 0.16f, 130f, 120f, 0.6f, 6f, Wave.Square);
                    return b;

                case SoundId.Shot:
                    b = NewBuffer(0.28f);
                    Noise(b, false, random, 0f, 0.28f, 1f, 20f, 0.35f);
                    Noise(b, false, random, 0f, 0.03f, 0.8f, 90f, 0f);
                    Tone(b, false, 0f, 0.18f, 190f, 45f, 0.9f, 16f, Wave.Sine);
                    return b;

                case SoundId.DryFire:
                    b = NewBuffer(0.05f);
                    Noise(b, false, random, 0f, 0.03f, 0.7f, 140f, 0f);
                    Tone(b, false, 0f, 0.03f, 2100f, 1700f, 0.4f, 110f, Wave.Sine);
                    return b;

                case SoundId.GunImpact:
                    b = NewBuffer(0.18f);
                    Tone(b, false, 0f, 0.18f, 150f, 55f, 1f, 24f, Wave.Sine);
                    Noise(b, false, random, 0f, 0.05f, 0.5f, 70f, 0.7f);
                    return b;

                case SoundId.BulletImpact:
                    b = NewBuffer(0.1f);
                    Noise(b, false, random, 0f, 0.1f, 0.8f, 55f, 0.3f);
                    Tone(b, false, 0f, 0.05f, 900f, 300f, 0.4f, 60f, Wave.Sine);
                    return b;

                case SoundId.EnemyHit:
                    b = NewBuffer(0.1f);
                    Tone(b, false, 0f, 0.1f, 320f, 190f, 0.6f, 30f, Wave.Square);
                    Noise(b, false, random, 0f, 0.04f, 0.4f, 80f, 0.4f);
                    return b;

                case SoundId.EnemyDeath:
                    b = NewBuffer(0.38f);
                    Tone(b, false, 0f, 0.38f, 440f, 80f, 0.6f, 8f, Wave.Square);
                    Noise(b, false, random, 0f, 0.25f, 0.5f, 14f, 0.6f);
                    return b;

                case SoundId.MultiplierHit:
                    b = NewBuffer(0.55f);
                    Tone(b, false, 0f, 0.22f, Midi(81), Midi(81), 0.6f, 12f, Wave.Sine);
                    Tone(b, false, 0.06f, 0.22f, Midi(85), Midi(85), 0.6f, 12f, Wave.Sine);
                    Tone(b, false, 0.12f, 0.22f, Midi(88), Midi(88), 0.6f, 12f, Wave.Sine);
                    Tone(b, false, 0.18f, 0.36f, Midi(93), Midi(93), 0.7f, 8f, Wave.Sine);
                    return b;

                case SoundId.CoinPickup:
                    b = NewBuffer(0.36f);
                    Tone(b, false, 0f, 0.08f, Midi(83), Midi(83), 0.6f, 10f, Wave.Sine);
                    Tone(b, false, 0.07f, 0.29f, Midi(88), Midi(88), 0.7f, 11f, Wave.Sine);
                    Tone(b, false, 0.07f, 0.29f, Midi(100), Midi(100), 0.15f, 16f, Wave.Sine);
                    return b;

                case SoundId.LevelWin:
                    b = NewBuffer(1.2f);
                    Tone(b, false, 0f, 0.2f, Midi(72), Midi(72), 0.6f, 9f, Wave.Triangle);
                    Tone(b, false, 0.12f, 0.2f, Midi(76), Midi(76), 0.6f, 9f, Wave.Triangle);
                    Tone(b, false, 0.24f, 0.2f, Midi(79), Midi(79), 0.6f, 9f, Wave.Triangle);
                    Tone(b, false, 0.36f, 0.84f, Midi(84), Midi(84), 0.7f, 3.5f, Wave.Triangle);
                    Tone(b, false, 0.36f, 0.84f, Midi(76), Midi(76), 0.35f, 3.5f, Wave.Sine);
                    Tone(b, false, 0.36f, 0.84f, Midi(79), Midi(79), 0.35f, 3.5f, Wave.Sine);
                    return b;

                case SoundId.LevelLose:
                    b = NewBuffer(1f);
                    Tone(b, false, 0f, 0.22f, Midi(67), Midi(67), 0.6f, 7f, Wave.Triangle);
                    Tone(b, false, 0.17f, 0.22f, Midi(63), Midi(63), 0.6f, 7f, Wave.Triangle);
                    Tone(b, false, 0.34f, 0.66f, Midi(60), Midi(55), 0.7f, 4f, Wave.Triangle);
                    return b;

                default:
                    throw new ArgumentOutOfRangeException(nameof(id), id, "SoundId sin sonido provisorio.");
            }
        }

        private static float[] RenderMenuMusic()
        {
            const float BPM = 96f;
            int[][] chords =
            {
                new[] { 45, 57, 60, 64 },
                new[] { 41, 57, 60, 65 },
                new[] { 48, 55, 60, 64 },
                new[] { 43, 55, 59, 62 }
            };
            int[] arpeggio = { 0, 1, 2, 1, 0, 2, 1, 2 };

            float beat = 60f / BPM;
            float[] b = NewBuffer(chords.Length * 4 * beat);

            for (int bar = 0; bar < chords.Length; bar++)
            {
                int[] chord = chords[bar];
                float barStart = bar * 4 * beat;

                Tone(b, true, barStart, beat * 2f, Midi(chord[0]), Midi(chord[0]), 0.5f, 1.6f, Wave.Triangle);
                Tone(b, true, barStart + beat * 2f, beat * 2f, Midi(chord[0]), Midi(chord[0]), 0.4f, 1.6f, Wave.Triangle);

                for (int step = 0; step < arpeggio.Length; step++)
                {
                    float hz = Midi(chord[1 + arpeggio[step]] + 12);
                    Tone(b, true, barStart + step * beat * 0.5f, beat * 1.5f, hz, hz, 0.22f, 5f, Wave.Sine);
                }
            }

            return b;
        }

        private static float[] RenderGameplayMusic()
        {
            const float BPM = 132f;
            const int REPEATS = 2;
            int[][] chords =
            {
                new[] { 40, 64, 67, 71 },
                new[] { 36, 64, 67, 72 },
                new[] { 43, 62, 67, 71 },
                new[] { 38, 62, 66, 69 }
            };
            int[] lead = { 0, 1, 2, 1, 0, 1, 2, 1, 0, 1, 2, 1, 2, 1, 0, 1 };
            int[] leadHigh = { 2, 1, 0, 1, 2, 1, 2, 0, 2, 1, 0, 1, 2, 2, 1, 0 };

            System.Random random = new System.Random(NOISE_SEED);
            float beat = 60f / BPM;
            int bars = chords.Length * REPEATS;
            float[] b = NewBuffer(bars * 4 * beat);

            for (int bar = 0; bar < bars; bar++)
            {
                int[] chord = chords[bar % chords.Length];
                bool secondPass = bar >= chords.Length;
                float barStart = bar * 4 * beat;

                for (int eighth = 0; eighth < 8; eighth++)
                {
                    float start = barStart + eighth * beat * 0.5f;
                    Tone(b, true, start, beat * 0.45f, Midi(chord[0]), Midi(chord[0]), 0.45f, 7f, Wave.Triangle);

                    if (eighth % 2 == 1)
                        Noise(b, true, random, start, 0.03f, 0.12f, 110f, 0f);
                }

                for (int quarter = 0; quarter < 4; quarter++)
                {
                    float start = barStart + quarter * beat;
                    Tone(b, true, start, 0.14f, 115f, 42f, 0.7f, 22f, Wave.Sine);

                    if (quarter % 2 == 1)
                        Noise(b, true, random, start, 0.12f, 0.28f, 28f, 0.3f);
                }

                int[] pattern = secondPass ? leadHigh : lead;
                int octave = secondPass ? 12 : 0;
                for (int step = 0; step < pattern.Length; step++)
                {
                    float hz = Midi(chord[1 + pattern[step]] + octave);
                    Tone(b, true, barStart + step * beat * 0.25f, beat * 0.5f, hz, hz, 0.13f, 9f, Wave.Square);
                }
            }

            return b;
        }

        private static float[] NewBuffer(float seconds)
        {
            return new float[Mathf.RoundToInt(seconds * SAMPLE_RATE)];
        }

        private static float Midi(int note)
        {
            return 440f * Mathf.Pow(2f, (note - 69) / 12f);
        }

        private static void Tone(float[] buffer, bool wrap, float start, float duration, float fromHz, float toHz,
            float amplitude, float decay, Wave wave)
        {
            int first = Mathf.RoundToInt(start * SAMPLE_RATE);
            int count = Mathf.RoundToInt(duration * SAMPLE_RATE);
            double phase = 0.0;

            for (int i = 0; i < count; i++)
            {
                float time = i / (float)SAMPLE_RATE;
                phase += Mathf.Lerp(fromHz, toHz, i / (float)count) / SAMPLE_RATE;
                float cycle = (float)(phase - Math.Floor(phase));

                float value;
                switch (wave)
                {
                    case Wave.Triangle:
                        value = 4f * Mathf.Abs(cycle - 0.5f) - 1f;
                        break;
                    case Wave.Square:
                        value = cycle < 0.5f ? 0.6f : -0.6f;
                        break;
                    default:
                        value = Mathf.Sin(cycle * 2f * Mathf.PI);
                        break;
                }

                Add(buffer, wrap, first + i, value * amplitude * Envelope(time, duration, decay));
            }
        }

        private static void Noise(float[] buffer, bool wrap, System.Random random, float start, float duration,
            float amplitude, float decay, float smoothing)
        {
            int first = Mathf.RoundToInt(start * SAMPLE_RATE);
            int count = Mathf.RoundToInt(duration * SAMPLE_RATE);
            float last = 0f;

            for (int i = 0; i < count; i++)
            {
                float white = (float)(random.NextDouble() * 2.0 - 1.0);
                last = Mathf.Lerp(white, last, smoothing);
                Add(buffer, wrap, first + i, last * amplitude * Envelope(i / (float)SAMPLE_RATE, duration, decay));
            }
        }

        private static float Envelope(float time, float duration, float decay)
        {
            float attack = Mathf.Clamp01(time / ATTACK_SECONDS);
            float release = Mathf.Clamp01((duration - time) / RELEASE_SECONDS);
            return attack * release * Mathf.Exp(-decay * time);
        }

        private static void Add(float[] buffer, bool wrap, int index, float value)
        {
            if (index >= buffer.Length)
            {
                if (!wrap)
                    return;

                index %= buffer.Length;
            }

            buffer[index] += value;
        }

        private static float[] Normalize(float[] samples)
        {
            float peak = 0f;
            foreach (float sample in samples)
                peak = Mathf.Max(peak, Mathf.Abs(sample));

            if (peak <= 0f)
                return samples;

            float gain = PEAK_LEVEL / peak;
            for (int i = 0; i < samples.Length; i++)
                samples[i] *= gain;

            return samples;
        }

        private static void WriteWav(string path, float[] samples)
        {
            const short CHANNELS = 1;
            const short BITS_PER_SAMPLE = 16;
            const int HEADER_SIZE_AFTER_RIFF = 36;
            int dataSize = samples.Length * (BITS_PER_SAMPLE / 8);

            using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(HEADER_SIZE_AFTER_RIFF + dataSize);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write(CHANNELS);
                writer.Write(SAMPLE_RATE);
                writer.Write(SAMPLE_RATE * CHANNELS * (BITS_PER_SAMPLE / 8));
                writer.Write((short)(CHANNELS * (BITS_PER_SAMPLE / 8)));
                writer.Write(BITS_PER_SAMPLE);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(dataSize);

                foreach (float sample in samples)
                    writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue));
            }
        }
    }
}
