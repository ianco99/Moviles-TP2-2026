using UnityEngine;

namespace GunJump.Gameplay
{
    public static class CameraFraming
    {
        // Yaw 0 mira de costado (hacia +Z); al subir, la cámara gira hacia el sentido de avance (+X)
        // y queda detrás y al costado del arma.
        public static Quaternion Rotation(float yaw, float pitch)
        {
            return Quaternion.Euler(pitch, yaw, 0f);
        }

        public static Vector3 Position(Vector3 focus, Quaternion rotation, float distance)
        {
            return focus - rotation * Vector3.forward * distance;
        }

        // Mantienen constante el ancho visible a la distancia del foco,
        // para que el nivel no cambie de dificultad según el teléfono.
        public static float OrthographicSize(float visibleWidth, float aspect)
        {
            return visibleWidth / (2f * aspect);
        }

        public static float VerticalFieldOfView(float visibleWidth, float distance, float aspect)
        {
            float halfHeight = visibleWidth / (2f * aspect);
            return 2f * Mathf.Atan2(halfHeight, distance) * Mathf.Rad2Deg;
        }
    }
}
