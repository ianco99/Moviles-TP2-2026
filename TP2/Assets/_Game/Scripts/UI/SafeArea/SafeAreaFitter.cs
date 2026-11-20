using UnityEngine;

namespace GunJump.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        RectTransform rect;
        Rect appliedArea;
        int appliedWidth;
        int appliedHeight;

        void Awake()
        {
            rect = (RectTransform)transform;
            Apply();
        }

        void Update()
        {
            if (Screen.safeArea != appliedArea || Screen.width != appliedWidth || Screen.height != appliedHeight)
                Apply();
        }

        void Apply()
        {
            appliedArea = Screen.safeArea;
            appliedWidth = Screen.width;
            appliedHeight = Screen.height;

            SafeAreaMath.Anchors(appliedArea, appliedWidth, appliedHeight, out Vector2 anchorMin, out Vector2 anchorMax);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
