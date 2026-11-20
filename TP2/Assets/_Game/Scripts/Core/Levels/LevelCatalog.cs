using System;
using UnityEngine;

namespace GunJump.Core
{
    [CreateAssetMenu(menuName = "GunJump/Level Catalog", fileName = "LevelCatalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField] string[] sceneNames = Array.Empty<string>();

        public int Count => sceneNames.Length;
        public string First => sceneNames.Length > 0 ? sceneNames[0] : null;

        public string NextAfter(string sceneName)
        {
            return NextAfter(sceneNames, sceneName);
        }

        public static string NextAfter(string[] names, string sceneName)
        {
            int index = Array.IndexOf(names, sceneName);
            if (index < 0 || index + 1 >= names.Length)
                return null;

            return names[index + 1];
        }
    }
}
