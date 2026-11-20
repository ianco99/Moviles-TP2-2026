using GunJump.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GunJump.EditorTools
{
    static class UiFactory
    {
        public static readonly Vector2 REFERENCE_RESOLUTION = new Vector2(1080f, 1920f);
        public static readonly Vector2 TOP_LEFT = new Vector2(0f, 1f);
        public static readonly Vector2 TOP_RIGHT = new Vector2(1f, 1f);
        public static readonly Vector2 CENTER = new Vector2(0.5f, 0.5f);

        static readonly Color BUTTON_COLOR = new Color(0.16f, 0.45f, 0.85f);
        static readonly Color DIM_COLOR = new Color(0f, 0f, 0f, 0.7f);

        public static EventSystem CreateEventSystem()
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            InputSystemUIInputModule uiModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();

            Unsupported.SmartReset(uiModule);
            return eventSystemObject.GetComponent<EventSystem>();
        }

        public static GameObject CreateCanvas(string name, Transform parent)
        {
            GameObject canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            if (parent != null)
                canvasObject.transform.SetParent(parent, false);

            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = REFERENCE_RESOLUTION;
            scaler.matchWidthOrHeight = 0.5f;
            return canvasObject;
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject rectObject = new GameObject(name, typeof(RectTransform));
            rectObject.transform.SetParent(parent, false);
            return (RectTransform)rectObject.transform;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        public static RectTransform CreateSafeArea(Transform parent)
        {
            RectTransform safeArea = CreateRect("SafeArea", parent);
            Stretch(safeArea);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();
            return safeArea;
        }

        public static TMP_Text CreateLabel(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment, Color color)
        {
            RectTransform rect = CreateRect(name, parent);
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }

        public static Image CreateImage(string name, Transform parent, Color color, bool blocksTouches)
        {
            RectTransform rect = CreateRect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = blocksTouches;
            return image;
        }

        public static GameObject CreateOverlay(string name, Transform parent)
        {
            Image dim = CreateImage(name, parent, DIM_COLOR, true);
            Stretch(dim.rectTransform);
            return dim.gameObject;
        }

        public static CommandButton CreateButton(string name, Transform parent, string text, float fontSize)
        {
            Image image = CreateImage(name, parent, BUTTON_COLOR, true);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            CommandButton command = image.gameObject.AddComponent<CommandButton>();

            TMP_Text label = CreateLabel("Label", image.transform, text, fontSize, TextAlignmentOptions.Center, Color.white);
            Stretch(label.rectTransform);
            return command;
        }
    }
}
