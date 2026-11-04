using System;
using System.Collections;
using System.Threading.Tasks;
using _Game.Scripts.Shop;
using GunJump.Core;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.UI;

namespace GunJump.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button shopBackButton;
        
        [Header("References")]
        [SerializeField] private GameObject landingMenu;
        [SerializeField] private LevelsMenuController levelSelectMenu;
        [SerializeField] private ShopMenuController shopMenu;

        [Header("Layout")]
        [Tooltip("Distancia del botón de volver de la tienda a la esquina superior izquierda del área segura.")]
        [SerializeField] private Vector2 shopBackMargin = new Vector2(50f, 50f);

        private bool picked = false;
        private EventBus eventBus;

        private EventBus EventBus => eventBus ??= ServiceProvider.Instance.GetService<EventBus>();

        private void Awake()
        {
            playButton.onClick.AddListener(PlayClicked);
            shopButton.onClick.AddListener(ShopClicked);
            shopBackButton.onClick.AddListener(ShopBackClicked);

            levelSelectMenu.Setup();
            shopMenu.Setup();
        }

        private void Start()
        {
            EventBus.Raise<ShowMoneyCountEvent>();
            EventBus.Raise<ShowGunPreviewEvent>();
        }

        private void ShopClicked()
        {
            if(!picked)
            StartCoroutine(WaitForAnim("Shop"));
        }

        private void ShopBackClicked()
        {
            if(!picked)
                StartCoroutine(WaitForAnim("ShopBack"));
        }
        
        private void PlayClicked()
        {
            if(!picked)
            StartCoroutine(WaitForAnim("Play"));
        }

        private IEnumerator WaitForAnim(string key)
        {
            picked = true;
            yield return new WaitForSeconds(0.1f);
            ActivateKey(key);
            picked = false;
        }

        private void ActivateKey(string key)
        {
            switch (key)
            {
                case "Play":
                    landingMenu.SetActive(false);
                    levelSelectMenu.gameObject.SetActive(true);
                    EventBus.Raise<HideMoneyCountEvent>();
                    // Además libera el bundle de armas, que el nivel vuelve a abrir.
                    EventBus.Raise<HideGunPreviewEvent>();
                    break;
                case "Shop":
                    landingMenu.SetActive(false);
                    shopMenu.gameObject.SetActive(true);
                    shopMenu.WakeUp();
                    KeepShopBackInSafeArea();
                    EventBus.Raise<ShowMoneyCountEvent>();
                    break;
                case "ShopBack":
                    landingMenu.SetActive(true);
                    shopMenu.gameObject.SetActive(false);
                    break;
            }
        }

        // Corre el botón para que no quede debajo del notch o de la cámara perforada (anchor y pivot arriba a la izquierda).
        private void KeepShopBackInSafeArea()
        {
            SafeAreaMath.Anchors(Screen.safeArea, Screen.width, Screen.height, out Vector2 anchorMin, out Vector2 anchorMax);

            RectTransform backRect = (RectTransform)shopBackButton.transform;
            Rect parentRect = ((RectTransform)backRect.parent).rect;
            float insetLeft = anchorMin.x * parentRect.width;
            float insetTop = (1f - anchorMax.y) * parentRect.height;

            backRect.anchoredPosition = new Vector2(shopBackMargin.x + insetLeft, -(shopBackMargin.y + insetTop));
        }
    }
}
