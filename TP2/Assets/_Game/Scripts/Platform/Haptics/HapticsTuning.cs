using System;
using System.Collections.Generic;
using UnityEngine;

namespace GunJump.Platform
{
    [CreateAssetMenu(menuName = "GunJump/Haptics Tuning", fileName = "HapticsTuning")]
    public sealed class HapticsTuning : ScriptableObject
    {
        [Tooltip("Vibración de cada momento del juego. Un momento que no está en la lista no vibra.")]
        [SerializeField] private List<PulseEntry> pulses = new List<PulseEntry>();

        public bool TryGetPulse(HapticPulse pulse, out long milliseconds, out int amplitude)
        {
            foreach (PulseEntry entry in pulses)
            {
                if (entry.pulse != pulse)
                    continue;

                milliseconds = entry.milliseconds;
                amplitude = entry.amplitude;
                return true;
            }

            milliseconds = 0;
            amplitude = 0;
            return false;
        }

        [Serializable]
        private struct PulseEntry
        {
            public HapticPulse pulse;
            [Tooltip("Duración en milisegundos.")]
            [Min(1)]
            public long milliseconds;
            [Tooltip("Intensidad de 1 a 255. Los teléfonos sin control de intensidad usan la suya.")]
            [Range(1, 255)]
            public int amplitude;
        }
    }
}
