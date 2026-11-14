using GunJump.Core;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GunJump.UI
{
    // El botón de volver de un nivel: congela el juego y pregunta antes de salir al menú.
    // Mientras el panel está abierto, su fondo tapa toda la pantalla y ningún toque llega a disparar.
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
            // Durante una carga (por ejemplo, el reinicio al caerse) el tiempo lo maneja LevelLoader.
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

            // El tiempo sigue detenido mientras carga: LevelLoader lo devuelve cuando el menú ya se ve.
            ServiceProvider services = ServiceProvider.Instance;
            if (services.ContainsService<LevelLoader>())
            {
                services.GetService<LevelLoader>().Load(menuBuildIndex);
                return;
            }

            // Dando Play directo en el nivel no hay servicios: se carga el menú a secas.
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
