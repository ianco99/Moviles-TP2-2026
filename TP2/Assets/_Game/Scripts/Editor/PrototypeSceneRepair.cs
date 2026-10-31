using GunJump.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GunJump.EditorTools
{
    // Vuelve a conectar la escena Prototype. Si sus scripts desaparecen con la escena abierta,
    // Unity pierde las referencias en memoria y al guardar quedan vacías en el archivo.
    public static class PrototypeSceneRepair
    {
        const string ScenePath = GameSceneBuilder.ScenesFolder + "/Prototype.unity";
        const string SystemsName = "Systems";

        [MenuItem("GunJump/Repair Prototype Scene")]
        public static void Repair()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("GunJump", "Salí de Play antes de reparar la escena.", "Entendido");
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return;

                scene = EditorSceneManager.OpenScene(ScenePath);
            }

            var gun = Object.FindAnyObjectByType<Gun>(FindObjectsInactive.Include);
            if (gun == null)
            {
                EditorUtility.DisplayDialog("GunJump", "La escena no tiene un objeto con el componente Gun: no hay nada que conectar.", "Entendido");
                return;
            }

            GameObject systems = GameObject.Find(SystemsName);
            if (systems == null)
                systems = new GameObject(SystemsName);

            var input = FindOrAdd<InputReader>(systems);
            var installer = FindOrAdd<GameplayInstaller>(systems);
            var loop = FindOrAdd<PrototypeLoop>(systems);

            var eventSystem = Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (eventSystem == null)
                eventSystem = UiFactory.CreateEventSystem();

            GameSceneBuilder.SetReference(installer, "gun", gun);
            GameSceneBuilder.SetReference(installer, "input", input);
            GameSceneBuilder.SetReference(loop, "gun", gun.transform);
            GameSceneBuilder.SetReference(input, "eventSystem", eventSystem);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("GunJump: escena Prototype reparada y guardada.");
        }

        static T FindOrAdd<T>(GameObject host) where T : Component
        {
            var found = Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
            return found != null ? found : host.AddComponent<T>();
        }
    }
}
