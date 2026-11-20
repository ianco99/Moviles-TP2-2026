using GunJump.Core;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GunJump.UI
{
    public sealed class BackToMenuPrompt : MonoBehaviour
    {
        [SerializeField] Button openButton;
        [SerializeField] GameObject panel;
        [SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;
        [Tooltip("Índice del menú principal en Build Settings.")]
        [SerializeField] int menuBuildIndex;

        bool leaving;

        void Awake()
        {
            openButton.onClick.AddListener(Open);
            confirmButton.onClick.AddListener(Confirm);
            cancelButton.onClick.AddListener(Cancel);
            panel.SetActive(false);
        }

        void Open()
        {
            if (leaving || panel.activeSelf || IsLoading())
                return;

            GameAudio.Play(SoundId.ButtonClick);
            Time.timeScale = 0f;
            panel.SetActive(true);
        }

        void Cancel()
        {
            if (leaving)
                return;

            GameAudio.Play(SoundId.ButtonClick);
            panel.SetActive(false);
            Time.timeScale = 1f;
        }

        void Confirm()
        {
            if (leaving)
                return;

            leaving = true;
            GameAudio.Play(SoundId.ButtonClick);

            ServiceProvider services = ServiceProvider.Instance;
            if (services.ContainsService<LevelLoader>())
            {
                services.GetService<LevelLoader>().Load(menuBuildIndex);
                return;
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(menuBuildIndex);
        }

        static bool IsLoading()
        {
            ServiceProvider services = ServiceProvider.Instance;
            return services.ContainsService<LevelLoader>() && services.GetService<LevelLoader>().IsLoading;
        }
    }
}
