using GunJump.Core;
using GunJump.Platform;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.UI;

namespace GunJump.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class VibrationToggleButton : MonoBehaviour
    {
        [SerializeField] Graphic icon;
        [Tooltip("Lo que se ve encima del ícono cuando la vibración está apagada.")]
        [SerializeField] GameObject offMark;
        [Tooltip("Opacidad del ícono con la vibración apagada.")]
        [Range(0f, 1f)]
        [SerializeField] float offIconAlpha = 0.4f;

        Button button;

        MobilePlatform MobilePlatform => ServiceProvider.Instance.GetService<MobilePlatform>();

        void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(Toggle);
        }

        void OnEnable()
        {
            button.interactable = ServiceProvider.Instance.ContainsService<MobilePlatform>();
            Refresh();
        }

        void Toggle()
        {
            MobilePlatform platform = MobilePlatform;
            platform.SetVibrationEnabled(!platform.IsVibrationEnabled);
            Refresh();
            GameAudio.Play(SoundId.ButtonClick);
            platform.Vibrate(HapticPulse.Kill);
        }

        void Refresh()
        {
            bool isOn = button.interactable && MobilePlatform.IsVibrationEnabled;
            offMark.SetActive(!isOn);

            Color color = icon.color;
            color.a = isOn ? 1f : offIconAlpha;
            icon.color = color;
        }
    }
}
