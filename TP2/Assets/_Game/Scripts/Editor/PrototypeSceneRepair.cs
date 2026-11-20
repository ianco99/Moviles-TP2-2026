using GunJump.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GunJump.EditorTools
{
    public static class PrototypeSceneRepair
    {
        const string SCENE_PATH = GameSceneBuilder.SCENES_FOLDER + "/Prototype.unity";
        const string SYSTEMS_NAME = "Systems";

        [MenuItem("GunJump/Repair Prototype Scene")]
        public static void Repair()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("GunJump", "Salí de Play antes de reparar la escena.", "Entendido");
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != SCENE_PATH)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return;

                scene = EditorSceneManager.OpenScene(SCENE_PATH);
            }

            Gun gun = Object.FindAnyObjectByType<Gun>(FindObjectsInactive.Include);
            if (gun == null)
            {
                EditorUtility.DisplayDialog("GunJump", "La escena no tiene un objeto con el componente Gun: no hay nada que conectar.", "Entendido");
                return;
            }

            GameObject systems = GameObject.Find(SYSTEMS_NAME);
            if (systems == null)
                systems = new GameObject(SYSTEMS_NAME);

            InputReader input = FindOrAdd<InputReader>(systems);
            GameplayInstaller installer = FindOrAdd<GameplayInstaller>(systems);
            PrototypeLoop loop = FindOrAdd<PrototypeLoop>(systems);

            EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);
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

        static ComponentType FindOrAdd<ComponentType>(GameObject host) where ComponentType : Component
        {
            ComponentType found = Object.FindAnyObjectByType<ComponentType>(FindObjectsInactive.Include);
            return found != null ? found : host.AddComponent<ComponentType>();
        }
    }
}
