using UnityEngine;

namespace GunJump.Core
{
    // Corre las corrutinas de las clases que no son MonoBehaviour y tienen que sobrevivir a los cambios
    // de escena (LevelLoader). Se crea por código en un objeto con DontDestroyOnLoad.
    public sealed class CoroutineHost : MonoBehaviour
    {
        public static CoroutineHost Create(string name)
        {
            var host = new GameObject(name);
            DontDestroyOnLoad(host);
            return host.AddComponent<CoroutineHost>();
        }
    }
}
