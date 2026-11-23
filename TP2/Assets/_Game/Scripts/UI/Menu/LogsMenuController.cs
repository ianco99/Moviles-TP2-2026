using GunJump.Core;
using GunJump.Platform;
using ianco99.ToolBox.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GunJump.UI
{
    public class LogsMenuController : MonoBehaviour
    {
        private const string NO_PARSE_OPEN = "<noparse>";
        private const string NO_PARSE_CLOSE = "</noparse>";
        private const string ESCAPED_NO_PARSE_CLOSE = "</ noparse>";
        private const string EMPTY_TEXT = "No hay logs para mostrar.";
        private const string CLEARED_TEXT = "Logs borrados.";

        [SerializeField] private TMP_Text logText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Button clearButton;
        [SerializeField] private TMP_Text statusText;

        [Header("Display")]
        [Tooltip("Tope de caracteres en pantalla, para que TextMeshPro no arme una malla enorme. Se muestran las entradas más nuevas.")]
        [SerializeField] private int maxVisibleCharacters = 15000;

        private bool isWaitingForClear;

        private MobilePlatform MobilePlatform => ServiceProvider.Instance.GetService<MobilePlatform>();

        private void Awake()
        {
            clearButton.onClick.AddListener(ClearClicked);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus || !isWaitingForClear)
                return;

            isWaitingForClear = false;

            string logs = MobilePlatform.GetLogs();
            if (logs.Length == 0)
                statusText.text = CLEARED_TEXT;

            ShowLogs(logs);
        }

        public void WakeUp()
        {
            isWaitingForClear = false;
            statusText.text = string.Empty;
            ShowLogs(MobilePlatform.GetLogs());
        }

        private void ClearClicked()
        {
            GameAudio.Play(SoundId.ButtonClick);
            statusText.text = string.Empty;
            isWaitingForClear = true;
            MobilePlatform.ClearLogs();
        }

        private void ShowLogs(string logs)
        {
            if (logs.Length == 0)
            {
                logText.text = EMPTY_TEXT;
            }
            else
            {
                string visibleLogs = logs.Length > maxVisibleCharacters ? logs.Substring(logs.Length - maxVisibleCharacters) : logs;
                logText.text = $"{NO_PARSE_OPEN}{visibleLogs.Replace(NO_PARSE_CLOSE, ESCAPED_NO_PARSE_CLOSE)}{NO_PARSE_CLOSE}";
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
