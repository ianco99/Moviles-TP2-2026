using System;
using System.Collections.Generic;
using UnityEngine;

namespace GunJump.Core
{
    [Serializable]
    sealed class LevelRecord
    {
        public string levelId;
        public int stars;
        public int bestScore;
    }

    // Memento: una foto del progreso. Fuera de PlayerProgress nadie puede leerla ni modificarla;
    // SaveSystem solo la pasa a JSON y la vuelve a levantar.
    [Serializable]
    public sealed class ProgressSnapshot
    {
        [SerializeField] internal int version = 1;
        [SerializeField] internal int coins;
        [SerializeField] internal List<LevelRecord> levels = new List<LevelRecord>();
    }
}
