using UnityEngine;

namespace GunJump.Gameplay
{
    [CreateAssetMenu(menuName = "GunJump/Camera Tuning", fileName = "CameraTuning")]
    public sealed class CameraTuning : ScriptableObject
    {
        [Tooltip("Ancho del mundo que se ve en el plano de juego, igual en cualquier teléfono.")]
        [SerializeField] float visibleWidth = 12f;
        [Tooltip("Distancia de la cámara al plano de juego.")]
        [SerializeField] float distance = 20f;
        [Tooltip("Cuánto se adelanta la cámara al arma, para ver lo que viene.")]
        [SerializeField] float lookAhead = 3f;
        [SerializeField] float verticalOffset = 2f;
        [SerializeField] float smoothTime = 0.15f;

        public float VisibleWidth => visibleWidth;
        public float Distance => distance;
        public float LookAhead => lookAhead;
        public float VerticalOffset => verticalOffset;
        public float SmoothTime => smoothTime;
    }
}
