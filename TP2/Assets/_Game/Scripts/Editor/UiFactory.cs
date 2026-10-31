using GunJump.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GunJump.EditorTools
{
    // Piezas de UI que usan los armadores de escenas. Todo lo que no es un botón queda sin raycast:
    // si un texto bloqueara los toques, tocar la pantalla dejaría de disparar.
    static class UiFactory
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        public static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        public static readonly Vector2 TopRight = new Vector2(1f, 1f);
        public static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        static readonly Color ButtonColor = new Color(0.16f, 0.45f, 0.85f);
        static readonly Color DimColor = new Color(0f, 0f, 0f, 0.7f);

        public static EventSystem CreateEventSystem()
        {
            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            var uiModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();

            // Lo mismo que hace el Inspector: deja asignado el asset de acciones del paquete, que sí se guarda en la escena.
            Unsupported.SmartReset(uiModule);
            return eventSystemObject.GetComponent<EventSystem>();
        }

        public static GameObject CreateCanvas(string name, Transform parent)
        {
            var canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            if (parent != null)
                canvasObject.transform.SetParent(parent, false);

            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            return canvasObject;
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var rectObject = new GameObject(name, typeof(RectTransform));
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

        // Ancla el elemento a un punto de su padre: una esquina, un borde o el centro.
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
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
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
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = blocksTouches;
            return image;
        }

        // Fondo oscuro a pantalla completa que bloquea los toques mientras está visible.
        public static GameObject CreateOverlay(string name, Transform parent)
        {
            Image dim = CreateImage(name, parent, DimColor, true);
            Stretch(dim.rectTransform);
            return dim.gameObject;
        }

        public static CommandButton CreateButton(string name, Transform parent, string text, float fontSize)
        {
            Image image = CreateImage(name, parent, ButtonColor, true);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var command = image.gameObject.AddComponent<CommandButton>();

            TMP_Text label = CreateLabel("Label", image.transform, text, fontSize, TextAlignmentOptions.Center, Color.white);
            Stretch(label.rectTransform);
            return command;
        }
    }
}
