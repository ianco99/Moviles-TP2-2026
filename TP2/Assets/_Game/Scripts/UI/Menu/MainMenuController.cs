using System;
using System.Collections;
using System.Threading.Tasks;
using _Game.Scripts.Shop;
using GunJump.Core;
using GunJump.Platform;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GunJump.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button levelsBackButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button shopBackButton;
        [SerializeField] private Button logsButton;
        [SerializeField] private Button logsBackButton;
        [Tooltip("Abre los logros de Play Games. Se arma con GunJump > UI > Setup Play Games and Vibration.")]
        [SerializeField] private Button achievementsButton;
        [Tooltip("Abre el leaderboard Total Score de Play Games.")]
        [SerializeField] private Button leaderboardButton;

        [Header("Quit")]
        [Tooltip("Cartel de salir que abre el botón Atrás de Android en la pantalla principal. Se arma con GunJump > UI > Setup Quit Prompt.")]
        [SerializeField] private GameObject quitPanel;
        [SerializeField] private Button quitConfirmButton;
        [SerializeField] private Button quitCancelButton;

        [Header("References")]
        [SerializeField] private GameObject landingMenu;
        [SerializeField] private LevelsMenuController levelSelectMenu;
        [SerializeField] private ShopMenuController shopMenu;
        [Tooltip("Pantalla del plugin nativo de logs. Se arma con GunJump > UI > Setup Logs Screen.")]
        [SerializeField] private LogsMenuController logsMenu;

        [Header("Layout")]
        [Tooltip("Distancia del botón de volver de la tienda a la esquina superior izquierda del área segura.")]
        [SerializeField] private Vector2 shopBackMargin = new Vector2(50f, 50f);

        private bool picked = false;
        private EventBus eventBus;
        private LevelLoader levelLoader;
        private MobilePlatform mobilePlatform;

        private EventBus EventBus => eventBus ??= ServiceProvider.Instance.GetService<EventBus>();
        private LevelLoader LevelLoader => levelLoader ??= ServiceProvider.Instance.GetService<LevelLoader>();
        private MobilePlatform MobilePlatform => mobilePlatform ??= ServiceProvider.Instance.GetService<MobilePlatform>();

        private void Awake()
        {
            playButton.onClick.AddListener(PlayClicked);
            levelsBackButton.onClick.AddListener(LevelsBackClicked);
            shopButton.onClick.AddListener(ShopClicked);
            shopBackButton.onClick.AddListener(ShopBackClicked);
            logsButton.onClick.AddListener(LogsClicked);
            logsBackButton.onClick.AddListener(LogsBackClicked);
            achievementsButton.onClick.AddListener(AchievementsClicked);
            leaderboardButton.onClick.AddListener(LeaderboardClicked);
            quitConfirmButton.onClick.AddListener(QuitConfirmed);
            quitCancelButton.onClick.AddListener(QuitCanceled);
            quitPanel.SetActive(false);

            StartCoroutine(levelSelectMenu.Setup());
            StartCoroutine(shopMenu.Setup());
        }

        private void Start()
        {
            EventBus.Raise<ShowMoneyCountEvent>();
            EventBus.Raise<ShowGunPreviewEvent>();
            OpenRequestedScreen();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (picked || keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
                return;

            if (levelSelectMenu.gameObject.activeSelf)
                StartCoroutine(WaitForAnim("LevelsBack"));
            else if (shopMenu.gameObject.activeSelf)
                StartCoroutine(WaitForAnim("ShopBack"));
            else if (logsMenu.gameObject.activeSelf)
                StartCoroutine(WaitForAnim("LogsBack"));
            else if (quitPanel.activeSelf)
                QuitCanceled();
            else
                OpenQuitPrompt();
        }

        private void OpenQuitPrompt()
        {
            GameAudio.Play(SoundId.ButtonClick);
            quitPanel.SetActive(true);
        }

        private void QuitConfirmed()
        {
            GameAudio.Play(SoundId.ButtonClick);
            Application.Quit();
        }

        private void QuitCanceled()
        {
            GameAudio.Play(SoundId.ButtonClick);
            quitPanel.SetActive(false);
        }

        private void OpenRequestedScreen()
        {
            switch (LevelLoader.TakeRequestedMenuScreen())
            {
                case MenuScreen.LevelSelect:
                    ActivateKey("Play");
                    break;
                case MenuScreen.Shop:
                    ActivateKey("Shop");
                    break;
            }
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

        private void LevelsBackClicked()
        {
            if(!picked)
                StartCoroutine(WaitForAnim("LevelsBack"));
        }

        private void LogsClicked()
        {
            if(!picked)
                StartCoroutine(WaitForAnim("Logs"));
        }

        private void LogsBackClicked()
        {
            if(!picked)
                StartCoroutine(WaitForAnim("LogsBack"));
        }

        private void AchievementsClicked()
        {
            GameAudio.Play(SoundId.ButtonClick);
            MobilePlatform.ShowAchievements();
        }

        private void LeaderboardClicked()
        {
            GameAudio.Play(SoundId.ButtonClick);
            MobilePlatform.ShowLeaderboard();
        }

        private IEnumerator WaitForAnim(string key)
        {
            picked = true;
            GameAudio.Play(SoundId.ButtonClick);
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
                    EventBus.Raise<HideGunPreviewEvent>();
                    break;
                case "LevelsBack":
                    landingMenu.SetActive(true);
                    levelSelectMenu.gameObject.SetActive(false);
                    EventBus.Raise<ShowMoneyCountEvent>();
                    EventBus.Raise<ShowGunPreviewEvent>();
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
                case "Logs":
                    landingMenu.SetActive(false);
                    logsMenu.gameObject.SetActive(true);
                    logsMenu.WakeUp();
                    EventBus.Raise<HideMoneyCountEvent>();
                    EventBus.Raise<HideGunPreviewEvent>();
                    break;
                case "LogsBack":
                    landingMenu.SetActive(true);
                    logsMenu.gameObject.SetActive(false);
                    EventBus.Raise<ShowMoneyCountEvent>();
                    EventBus.Raise<ShowGunPreviewEvent>();
                    break;
            }
        }

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
