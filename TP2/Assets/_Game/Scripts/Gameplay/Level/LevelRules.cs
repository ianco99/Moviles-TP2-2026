using UnityEngine;

namespace GunJump.Gameplay
{
    // Reglas comunes a todos los niveles.
    [CreateAssetMenu(menuName = "GunJump/Level Rules", fileName = "LevelRules")]
    public sealed class LevelRules : ScriptableObject
    {
        [Tooltip("Segundos que se espera, ya sin balas, antes de dar el intento por perdido. Le da tiempo al último disparo de llegar a la meta.")]
        [SerializeField] float outOfAmmoDelay = 2.5f;

        public float OutOfAmmoDelay => outOfAmmoDelay;
    }
}
