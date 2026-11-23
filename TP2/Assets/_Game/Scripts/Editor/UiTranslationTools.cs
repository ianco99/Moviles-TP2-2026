using System.Collections.Generic;
using System.IO;
using GunJump.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GunJump.EditorTools
{
    public static class UiTranslationTools
    {
        const string ROOT = "Assets/_Game";
        const string TEXT_FIELD = "m_text";
        const string HINT_FIELD = "hint";
        const string START_HINT_FIELD = "startHint";

        static readonly string[] PREFAB_PATHS =
        {
            ROOT + "/Prefabs/Level/LevelCore.prefab",
            ROOT + "/Prefabs/Level/TutorialZone.prefab",
            ROOT + "/Prefabs/UI/LevelMenu.prefab"
        };

        static readonly string[] SCENE_PATHS =
        {
            ROOT + "/Scenes/MainMenu.unity",
            ROOT + "/Scenes/Loading.unity",
            ROOT + "/Scenes/Level1.unity",
            ROOT + "/Scenes/Level2.unity",
            ROOT + "/Scenes/Level3.unity",
            ROOT + "/Scenes/Level4.unity",
            ROOT + "/Scenes/Level5.unity"
        };

        static readonly Dictionary<string, string> englishBySpanish = new Dictionary<string, string>
        {
            { "Balas 0", "Ammo 0" },
            { "Monedas 0", "Coins 0" },
            { "Enemigos 0", "Enemies 0" },
            { "Pausa", "Paused" },
            { "Seguir", "Resume" },
            { "Reintentar", "Retry" },
            { "Menú", "Menu" },
            { "¡Nivel completado!", "Level complete!" },
            { "Intento fallido", "Level failed" },
            { "+0 monedas", "+0 coins" },
            { "Multiplicador x1", "Multiplier x1" },
            { "Puntaje 0", "Score 0" },
            { "Siguiente nivel", "Next level" },
            { "Tienda", "Shop" },
            { "Cargando", "Loading" },
            { "¿SALIR DEL JUEGO?", "QUIT GAME?" },
            { "SÍ", "YES" },
            { "LOGROS", "AWARDS" },
            { "BORRAR", "CLEAR" },
            { "Texto de ayuda", "Hint text" },
            { "Tocá la pantalla para disparar", "Tap the screen to shoot" },
            { "Cada disparo te empuja al revés del cañón", "Each shot pushes you away from the barrel" },
            { "Disparale a los enemigos: dan monedas", "Shoot enemies: they drop coins" },
            { "Apuntá hacia abajo para subir", "Aim down to go up" },
            { "¡Llegá a la meta y acertale al multiplicador!", "Reach the goal and hit the multiplier!" }
        };

        [MenuItem("GunJump/UI/Translate UI To English")]
        public static void TranslateUiToEnglish()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("GunJump", "Salí de Play antes de traducir la UI.", "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string previous = SceneManager.GetActiveScene().path;
            List<string> changes = new List<string>();

            foreach (string prefabPath in PREFAB_PATHS)
            {
                if (!File.Exists(prefabPath))
                    continue;

                GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
                if (TranslateHierarchy(contents, Path.GetFileName(prefabPath), changes) > 0)
                    PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);

                PrefabUtility.UnloadPrefabContents(contents);
            }

            foreach (string scenePath in SCENE_PATHS)
            {
                if (!File.Exists(scenePath))
                    continue;

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int translated = 0;

                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    translated += TranslateHierarchy(root, scene.name, changes);
                }

                if (translated > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }

            if (!string.IsNullOrEmpty(previous) && File.Exists(previous))
                EditorSceneManager.OpenScene(previous);

            Debug.Log(changes.Count > 0
                ? $"GunJump: {changes.Count} textos pasados a inglés:\n" + string.Join("\n", changes)
                : "GunJump: no quedaban textos en español en las escenas ni en los prefabs de la UI.");
        }

        static int TranslateHierarchy(GameObject root, string source, List<string> changes)
        {
            int translated = 0;

            foreach (TMP_Text label in root.GetComponentsInChildren<TMP_Text>(true))
            {
                translated += TranslateField(label, TEXT_FIELD, source, changes);
            }

            foreach (TutorialZone zone in root.GetComponentsInChildren<TutorialZone>(true))
            {
                translated += TranslateField(zone, HINT_FIELD, source, changes);
            }

            foreach (LevelSettings settings in root.GetComponentsInChildren<LevelSettings>(true))
            {
                translated += TranslateField(settings, START_HINT_FIELD, source, changes);
            }

            return translated;
        }

        static int TranslateField(Object target, string field, string source, List<string> changes)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null || !englishBySpanish.TryGetValue(property.stringValue, out string english))
                return 0;

            changes.Add($"{source} / {target.name}: '{property.stringValue}' → '{english}'");
            property.stringValue = english;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return 1;
        }
    }
}
