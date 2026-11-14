using GunJump.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GunJump.UI
{
    // Prende y apaga un canal de audio (efectos o música) y muestra cómo quedó.
    // El estado vive en AudioService: el botón del menú y el del nivel muestran siempre lo mismo.
    [RequireComponent(typeof(Button))]
    public sealed class AudioToggleButton : MonoBehaviour
    {
        [SerializeField] AudioChannel channel;
        [SerializeField] Graphic icon;
        [Tooltip("Lo que se ve encima del ícono cuando el canal está apagado.")]
        [SerializeField] GameObject offMark;
        [Tooltip("Opacidad del ícono con el canal apagado.")]
        [Range(0f, 1f)]
        [SerializeField] float offIconAlpha = 0.4f;

        Button button;

        void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(Toggle);
        }

        // También al volver a mostrarse: el canal pudo cambiar desde otro botón mientras tanto.
        void OnEnable()
        {
            // Dando Play directo en una escena de nivel no hay AudioService y no hay nada que prender.
            button.interactable = GameAudio.IsAvailable;
            Refresh();
        }

        void Toggle()
        {
            GameAudio.Toggle(channel);
            Refresh();
            // Después de cambiar: prender los efectos se escucha, apagarlos no.
            GameAudio.Play(SoundId.ButtonClick);
        }

        void Refresh()
        {
            bool isOn = GameAudio.IsEnabled(channel);
            offMark.SetActive(!isOn);

            Color color = icon.color;
            color.a = isOn ? 1f : offIconAlpha;
            icon.color = color;
        }
    }
}
