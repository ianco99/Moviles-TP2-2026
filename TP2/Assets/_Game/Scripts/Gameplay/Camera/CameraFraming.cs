using UnityEngine;

namespace GunJump.Gameplay
{
    public static class CameraFraming
    {
        public static Quaternion Rotation(float yaw, float pitch)
        {
            return Quaternion.Euler(pitch, yaw, 0f);
        }

        public static Vector3 Position(Vector3 focus, Quaternion rotation, float distance)
        {
            return focus - rotation * Vector3.forward * distance;
        }

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
