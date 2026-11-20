using UnityEngine;

namespace GunJump.Gameplay
{
    public static class GunMath
    {
        const float VERTICAL_DEAD_ZONE = 0.1f;

        public static Vector2 ApplyRecoil(Vector2 velocity, Vector2 barrelDirection, float recoil, float carryOver, float maxSpeed)
        {
            Vector2 result = velocity * carryOver - barrelDirection.normalized * recoil;
            return Vector2.ClampMagnitude(result, maxSpeed);
        }

        public static Vector2 EnsureLift(Vector2 velocity, float lift, float maxSpeed)
        {
            velocity.y = Mathf.Max(velocity.y, lift);
            return Vector2.ClampMagnitude(velocity, maxSpeed);
        }

        public static float RecoilSpinDirection(float barrelX, float currentSpin)
        {
            if (Mathf.Abs(barrelX) > VERTICAL_DEAD_ZONE)
                return Mathf.Sign(barrelX);

            return currentSpin < 0f ? -1f : 1f;
        }

        public static float DampSpin(float spin, float damping, float deltaTime)
        {
            return Mathf.MoveTowards(spin, 0f, damping * deltaTime);
        }

        public static void FitToLength(Bounds bounds, float length, out float scale, out Vector3 position)
        {
            float size = bounds.size.x;
            scale = size > 0f ? length / size : 1f;
            position = -bounds.center * scale;
        }

        public static Vector3 UndoParentScale(Vector3 childScale, Vector3 parentScale)
        {
            return new Vector3(
                parentScale.x != 0f ? childScale.x / parentScale.x : childScale.x,
                parentScale.y != 0f ? childScale.y / parentScale.y : childScale.y,
                parentScale.z != 0f ? childScale.z / parentScale.z : childScale.z);
        }
    }
}
