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
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(snapshot));

            if (File.Exists(path))
                File.Delete(path);

            File.Move(temporary, path);
        }

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
