using UnityEngine;

namespace GunJump.Gameplay
{
    // Mantiene constante el ancho visible del mundo, para que el nivel no cambie de dificultad según el teléfono.
    public static class CameraFraming
    {
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
