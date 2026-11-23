using UnityEngine;

namespace GunJump.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class MovingPlatform : MonoBehaviour
    {
        [Tooltip("Cuánto se aleja, en unidades, desde donde está puesta en la escena.")]
        [SerializeField] Vector3 travel = new Vector3(4f, 0f, 0f);
        [Tooltip("Segundos para ir y volver.")]
        [Min(0.1f)]
        [SerializeField] float cycleSeconds = 4f;

        Rigidbody body;
        Vector3 start;
        float elapsed;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            start = body.position;
        }

        void FixedUpdate()
        {
            elapsed += Time.fixedDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(elapsed * 2f / cycleSeconds, 1f));
            body.MovePosition(start + travel * progress);
        }

        void OnDrawGizmosSelected()
        {
            Vector3 from = Application.isPlaying ? start : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(from, from + travel);
            Gizmos.DrawWireCube(from + travel, transform.lossyScale);
        }
    }
}
