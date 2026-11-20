using UnityEngine;

namespace GunJump.Core
{
    public sealed class CoroutineHost : MonoBehaviour
    {
        public static CoroutineHost Create(string name)
        {
            GameObject host = new GameObject(name);
            DontDestroyOnLoad(host);
            return host.AddComponent<CoroutineHost>();
        }
    }
}
