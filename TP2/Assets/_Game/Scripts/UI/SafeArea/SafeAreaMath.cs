using UnityEngine;

namespace GunJump.UI
{
    public static class SafeAreaMath
    {
        // Convierte el área segura, que viene en píxeles, en anchors de 0 a 1.
        public static void Anchors(Rect safeArea, int screenWidth, int screenHeight, out Vector2 anchorMin, out Vector2 anchorMax)
        {
            if (screenWidth <= 0 || screenHeight <= 0)
            {
                anchorMin = Vector2.zero;
                anchorMax = Vector2.one;
                return;
            }

            anchorMin = new Vector2(safeArea.xMin / screenWidth, safeArea.yMin / screenHeight);
            anchorMax = new Vector2(safeArea.xMax / screenWidth, safeArea.yMax / screenHeight);
        }
    }
}
