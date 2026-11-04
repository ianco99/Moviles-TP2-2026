using _Game.Scripts.Economy;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using TMPro;
using UnityEngine;

namespace GunJump.UI
{
    public class MoneyCountController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform moneyCount;
        [SerializeField] private RectTransform backgroundPanel;
        [SerializeField] private RectTransform coinIcon;
        [SerializeField] private TMP_Text moneyText;

        [Header("Layout")]
        [Tooltip("Margen interno del panel: x a la izquierda del ícono, y a la derecha del número.")]
        [SerializeField] private Vector2 padding = new Vector2(20f, 30f);
        [SerializeField] private float iconSpacing = 10f;
        [Tooltip("Distancia a la esquina superior derecha del área segura.")]
        [SerializeField] private Vector2 screenMargin = new Vector2(50f, 50f);

        private EventBus eventBus;
        private PlayerWalletManager wallet;
        private EventBus.EventCallback<ShowMoneyCountEvent> showCallback;
        private EventBus.EventCallback<HideMoneyCountEvent> hideCallback;

        private EventBus EventBus => eventBus ??= ServiceProvider.Instance.GetService<EventBus>();
        private PlayerWalletManager Wallet => wallet ??= ServiceProvider.Instance.GetService<PlayerWalletManager>();

        private void Awake()
        {
            showCallback = OnShowRequested;
            hideCallback = OnHideRequested;

            EventBus.Subscribe(showCallback);
            EventBus.Subscribe(hideCallback);
        }

        private void OnDestroy()
        {
            if (eventBus == null)
            {
                return;
            }

            eventBus.UnSubscribe(showCallback);
            eventBus.UnSubscribe(hideCallback);
        }

        private void OnShowRequested(in ShowMoneyCountEvent e)
        {
            Show();
        }

        private void OnHideRequested(in HideMoneyCountEvent e)
        {
            Hide();
        }

        private void Show()
        {
            moneyText.text = Wallet.PlayerMoney.ToString("N0");
            FitToNumber();
            KeepInSafeArea();
            moneyCount.gameObject.SetActive(true);
        }

        private void Hide()
        {
            moneyCount.gameObject.SetActive(false);
        }

        // El panel mide lo justo para el ícono y el número, más el margen interno. Crece hacia la izquierda (pivot arriba a la derecha).
        private void FitToNumber()
        {
            float iconWidth = coinIcon.rect.width;
            float textWidth = moneyText.GetPreferredValues(moneyText.text).x;
            float textLeft = padding.x + iconWidth + iconSpacing;

            RectTransform textRect = moneyText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(textLeft, 0f);
            textRect.offsetMax = new Vector2(-padding.y, 0f);

            moneyCount.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textLeft + textWidth + padding.y);

            backgroundPanel.anchorMin = Vector2.zero;
            backgroundPanel.anchorMax = Vector2.one;
            backgroundPanel.offsetMin = Vector2.zero;
            backgroundPanel.offsetMax = Vector2.zero;
        }

        // Corre el panel para que no quede debajo del notch o de la cámara perforada.
        private void KeepInSafeArea()
        {
            SafeAreaMath.Anchors(Screen.safeArea, Screen.width, Screen.height, out _, out Vector2 anchorMax);

            Rect parentRect = ((RectTransform)moneyCount.parent).rect;
            float insetRight = (1f - anchorMax.x) * parentRect.width;
            float insetTop = (1f - anchorMax.y) * parentRect.height;

            moneyCount.anchoredPosition = new Vector2(-(screenMargin.x + insetRight), -(screenMargin.y + insetTop));
        }
    }
}
