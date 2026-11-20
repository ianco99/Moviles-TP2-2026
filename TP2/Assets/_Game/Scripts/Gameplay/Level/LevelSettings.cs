using UnityEngine;

namespace GunJump.Gameplay
{
    public sealed class LevelSettings : MonoBehaviour
    {
        [Tooltip("Balas disponibles. 0 es munición infinita.")]
        [Min(0)]
        [SerializeField] int ammo;
        [Tooltip("Segundos para la tercera estrella.")]
        [SerializeField] float targetTime = 35f;
        [Tooltip("Altura por debajo de la cual el arma se da por caída.")]
        [SerializeField] float fallLimitY = -12f;

        public int Ammo => ammo;
        public float TargetTime => targetTime;
        public float FallLimitY => fallLimitY;
    }
}
