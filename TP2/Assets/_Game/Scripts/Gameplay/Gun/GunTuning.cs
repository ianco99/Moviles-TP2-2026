using UnityEngine;

namespace GunJump.Gameplay
{
    [CreateAssetMenu(menuName = "GunJump/Gun Tuning", fileName = "GunTuning")]
    public sealed class GunTuning : ScriptableObject
    {
        [Header("Giro")]
        [Tooltip("Grados por segundo de giro que deja cada disparo. El cañón siempre patea hacia arriba: con el arma derecha gira en un sentido y dada vuelta (mango arriba), en el contrario.")]
        [Min(0f)]
        [SerializeField] float recoilSpin = 540f;
        [Tooltip("Grados por segundo al cuadrado que el giro pierde solo entre disparos.")]
        [SerializeField] float spinDamping = 120f;

        [Header("Retroceso")]
        [Tooltip("Cuánta velocidad previa se conserva al disparar. 1 suma todo; 0 la reemplaza.")]
        [Range(0f, 1f)]
        [SerializeField] float recoilCarryOver = 0.5f;
        [SerializeField] float maxSpeed = 14f;
        [Tooltip("Velocidad mínima hacia arriba que deja un disparo hecho con el arma apoyada. La despega del piso para que el giro del retroceso pueda darla vuelta.")]
        [SerializeField] float groundLift = 5f;
        [Tooltip("Segundos después del último contacto en que el arma todavía cuenta como apoyada.")]
        [SerializeField] float groundedGrace = 0.08f;

        [Header("Disparo")]
        [Tooltip("Segundos mínimos entre dos disparos.")]
        [SerializeField] float fireInterval = 0.12f;

        [Header("Sonido de golpe")]
        [Tooltip("Velocidad mínima del choque, en unidades por segundo, para que el arma suene al golpear. Por debajo no suena: apoyarse o rodar despacio es silencioso.")]
        [SerializeField] float impactSoundMinSpeed = 1.5f;
        [Tooltip("Velocidad del choque a la que el golpe suena a volumen completo.")]
        [SerializeField] float impactSoundMaxSpeed = 10f;
        [Tooltip("Volumen del golpe más suave que llega a sonar, como fracción del volumen completo.")]
        [Range(0f, 1f)]
        [SerializeField] float impactSoundMinVolume = 0.35f;
        [Tooltip("Segundos mínimos entre dos sonidos de golpe, para que un rebote corto no suene varias veces.")]
        [SerializeField] float impactSoundInterval = 0.08f;

        public float RecoilSpin => recoilSpin;
        public float SpinDamping => spinDamping;
        public float RecoilCarryOver => recoilCarryOver;
        public float MaxSpeed => maxSpeed;
        public float GroundLift => groundLift;
        public float GroundedGrace => groundedGrace;
        public float FireInterval => fireInterval;
        public float ImpactSoundMinSpeed => impactSoundMinSpeed;
        public float ImpactSoundMaxSpeed => impactSoundMaxSpeed;
        public float ImpactSoundMinVolume => impactSoundMinVolume;
        public float ImpactSoundInterval => impactSoundInterval;
    }
}
