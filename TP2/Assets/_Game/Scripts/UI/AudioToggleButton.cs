using GunJump.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GunJump.UI
{
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

        void OnEnable()
        {
            button.interactable = GameAudio.IsAvailable;
            Refresh();
        }

        void Toggle()
        {
            GameAudio.Toggle(channel);
            Refresh();
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
