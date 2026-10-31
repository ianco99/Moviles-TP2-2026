using System;
using System.IO;
using UnityEngine;

namespace GunJump.Core
{
    // Cuidador del Memento: guarda y levanta la foto del progreso sin mirar qué tiene adentro.
    public sealed class SaveSystem
    {
        readonly string path;

        public SaveSystem(string filePath)
        {
            path = filePath;
        }

        public void Save(ProgressSnapshot snapshot)
        {
            // Se escribe a un temporal y se reemplaza: si la app muere a mitad de camino, el guardado anterior sigue entero.
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(snapshot));

            if (File.Exists(path))
                File.Delete(path);

            File.Move(temporary, path);
        }

        // Devuelve null si no hay guardado o si el archivo está dañado.
        public ProgressSnapshot Load()
        {
            if (!File.Exists(path))
                return null;

            try
            {
                return JsonUtility.FromJson<ProgressSnapshot>(File.ReadAllText(path));
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
