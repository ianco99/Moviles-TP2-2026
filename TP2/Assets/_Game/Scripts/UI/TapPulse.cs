using UnityEngine;

namespace GunJump.UI
{
    public sealed class TapPulse : MonoBehaviour
    {
        [Tooltip("Segundos de un latido completo. Usa tiempo sin escalar: late igual en cámara lenta.")]
        [Min(0.1f)]
        [SerializeField] float period = 0.9f;
        [SerializeField] float minScale = 0.8f;
        [SerializeField] float maxScale = 1.15f;

        void Update()
        {
            float progress = Mathf.PingPong(Time.unscaledTime * 2f / period, 1f);
            transform.localScale = Vector3.one * Mathf.Lerp(minScale, maxScale, progress);
        }
    }
}
