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

        [Header("Tutorial")]
        [Tooltip("Velocidad del tiempo hasta el primer disparo. 1 = normal, sin cámara lenta.")]
        [Range(0.05f, 1f)]
        [SerializeField] float startTimeScale = 1f;
        [Tooltip("Texto que se muestra, con el círculo de toque, hasta el primer disparo. Vacío = nada.")]
        [TextArea]
        [SerializeField] string startHint;

        public int Ammo => ammo;
        public float TargetTime => targetTime;
        public float FallLimitY => fallLimitY;
        public float StartTimeScale => startTimeScale;
        public string StartHint => startHint;
    }
}
