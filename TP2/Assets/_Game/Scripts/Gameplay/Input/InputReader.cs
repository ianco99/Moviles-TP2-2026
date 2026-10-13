using System.Collections.Generic;
using GunJump.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace GunJump.Gameplay
{
    // Convierte cada toque en un comando. Cada dedo que baja cuenta, para que se pueda tocar rápido con dos pulgares.
    public sealed class InputReader : MonoBehaviour
    {
        [Tooltip("Opcional. Si queda vacío se usa el EventSystem activo de la escena.")]
        [SerializeField] EventSystem eventSystem;

        readonly List<RaycastResult> uiHits = new List<RaycastResult>(4);
        PointerEventData pointerData;
        CommandInvoker invoker;
        ICommand fireCommand;

        public void Init(CommandInvoker commandInvoker, ICommand fire)
        {
            invoker = commandInvoker;
            fireCommand = fire;
        }

        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        void Update()
        {
            if (fireCommand == null)
                return;

            ReadOnlyArray<Touch> touches = Touch.activeTouches;
            for (int i = 0; i < touches.Count; i++)
            {
                if (touches[i].began)
                    Fire(touches[i].screenPosition);
            }

            // El mouse solo existe en el Editor y en PC.
            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                Fire(mouse.position.ReadValue());
        }

        void Fire(Vector2 screenPosition)
        {
            if (IsOverUi(screenPosition))
                return;

            invoker.Execute(fireCommand, Time.time);
        }

        bool IsOverUi(Vector2 screenPosition)
        {
            EventSystem system = eventSystem != null ? eventSystem : EventSystem.current;
            if (system == null)
                return false;

            pointerData ??= new PointerEventData(system);
            pointerData.position = screenPosition;
            uiHits.Clear();
            system.RaycastAll(pointerData, uiHits);
            return uiHits.Count > 0;
        }
    }
}
