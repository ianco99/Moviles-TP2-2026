using UnityEngine;

namespace GunJump.Gameplay
{
    public sealed class CoinBob : MonoBehaviour
    {
        [Tooltip("El modelo que sube y baja. El trigger de CoinPickup queda quieto en la raíz.")]
        [SerializeField] Transform visual;
        [Tooltip("Unidades que sube y baja desde donde está puesto en el prefab.")]
        [SerializeField] float amplitude = 0.12f;
        [Tooltip("Segundos de una subida y bajada completa. Usa tiempo escalado: se frena en cámara lenta y en pausa.")]
        [Min(0.1f)]
        [SerializeField] float period = 1.6f;

        Vector3 restPosition;

        void Awake()
        {
            restPosition = visual.localPosition;
        }

        void Update()
        {
            float wave = Mathf.Sin(Time.time * 2f * Mathf.PI / period);
            visual.localPosition = restPosition + Vector3.up * (wave * amplitude);
        }
    }
}
