using System;
using UnityEngine;

namespace GunJump.Core
{
    // Orden de los niveles. El nombre de la escena es también el id del nivel en el guardado.
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

        // Devuelve null si es el último nivel o si no está en la lista.
        public static string NextAfter(string[] names, string sceneName)
        {
            int index = Array.IndexOf(names, sceneName);
            if (index < 0 || index + 1 >= names.Length)
                return null;

            return names[index + 1];
        }
    }
}
