using UnityEngine;

namespace GunJump.Gameplay
{
    [CreateAssetMenu(menuName = "GunJump/Camera Tuning", fileName = "CameraTuning")]
    public sealed class CameraTuning : ScriptableObject
    {
        [Header("Ángulo")]
        [Tooltip("Grados de giro hacia adelante. 0 es vista de costado pura; cuanto más alto, más desde atrás del arma se mira.")]
        [Range(0f, 80f)]
        [SerializeField] float yaw = 35f;
        [Tooltip("Grados de inclinación hacia abajo. 0 mira horizontal.")]
        [Range(-10f, 60f)]
        [SerializeField] float pitch = 12f;

        [Header("Encuadre")]
        [Tooltip("Distancia de la cámara al punto que mira.")]
        [SerializeField] float distance = 16f;
        [Tooltip("Cuánto se adelanta al arma el punto que mira la cámara, para ver lo que viene.")]
        [SerializeField] float lookAhead = 3f;
        [Tooltip("Altura del punto que mira la cámara respecto del arma.")]
        [SerializeField] float verticalOffset = 1f;
        [Tooltip("Ancho que se ve a la distancia del punto mirado, igual en cualquier teléfono.")]
        [SerializeField] float visibleWidth = 12f;

        [Header("Seguimiento")]
        [SerializeField] float smoothTime = 0.15f;

        public float Yaw => yaw;
        public float Pitch => pitch;
        public float Distance => distance;
        public float LookAhead => lookAhead;
        public float VerticalOffset => verticalOffset;
        public float VisibleWidth => visibleWidth;
        public float SmoothTime => smoothTime;
    }
}
