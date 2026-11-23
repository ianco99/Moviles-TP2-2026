using UnityEngine;

namespace GunJump.Gameplay
{
    [CreateAssetMenu(menuName = "GunJump/Level Rules", fileName = "LevelRules")]
    public sealed class LevelRules : ScriptableObject
    {
        [Tooltip("Segundos que se espera, ya sin balas, antes de dar el intento por perdido. Le da tiempo al último disparo de llegar a la meta.")]
        [SerializeField] float outOfAmmoDelay = 2.5f;
        [Tooltip("Multiplicador más alto de la pared final. Acertarle da el logro Top Multiplier.")]
        [Min(1)]
        [SerializeField] int topMultiplier = 5;

        public float OutOfAmmoDelay => outOfAmmoDelay;
        public int TopMultiplier => topMultiplier;
    }
}
