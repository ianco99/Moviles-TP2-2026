using System.Collections.Generic;
using System.Text;
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
        private const string COLOR_OPEN = "<color=#";
        private const string COLOR_CLOSE = "</color>";
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
        [Tooltip("Entradas más nuevas que se muestran como máximo. El plugin devuelve todas las de la sesión.")]
        [SerializeField] private int maxVisibleLines = 200;
        [Tooltip("Tope de caracteres en pantalla, para que TextMeshPro no arme una malla enorme. Se muestran las entradas más nuevas.")]
        [SerializeField] private int maxVisibleCharacters = 15000;
        [SerializeField] private Color infoColor = Color.white;
        [SerializeField] private Color warningColor = new Color(1f, 0.84f, 0.2f);
        [SerializeField] private Color errorColor = new Color(1f, 0.42f, 0.42f);

        private readonly List<LogLine> lines = new List<LogLine>();
        private readonly StringBuilder textBuilder = new StringBuilder();
        private string infoHex;
        private string warningHex;
        private string errorHex;

        private MobilePlatform MobilePlatform => ServiceProvider.Instance.GetService<MobilePlatform>();

        private void Awake()
        {
            infoHex = ColorUtility.ToHtmlStringRGB(infoColor);
            warningHex = ColorUtility.ToHtmlStringRGB(warningColor);
            errorHex = ColorUtility.ToHtmlStringRGB(errorColor);

            clearButton.onClick.AddListener(ClearClicked);
        }

        private void Update()
        {
            if (MobilePlatform.WereLogsClearedSinceLastCheck())
                OnLogsCleared();
        }

        public void WakeUp()
        {
            statusText.text = string.Empty;
            Refresh();
        }

        private void OnLogsCleared()
        {
            statusText.text = CLEARED_TEXT;
            Refresh();
        }

        private void ClearClicked()
        {
            GameAudio.Play(SoundId.ButtonClick);
            statusText.text = string.Empty;
            MobilePlatform.RequestClearLogs();
        }

        private void Refresh()
        {
            MobilePlatform.ReadLogs(lines);

            if (lines.Count == 0)
            {
                logText.text = EMPTY_TEXT;
            }
            else
            {
                textBuilder.Clear();

                for (int i = FirstVisibleLine(); i < lines.Count; i++)
                {
                    AppendLine(lines[i]);
                }

                logText.text = textBuilder.ToString();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            scrollRect.verticalNormalizedPosition = 0f;
        }

        private int FirstVisibleLine()
        {
            int oldestAllowedLine = Mathf.Max(0, lines.Count - maxVisibleLines);
            int firstVisible = lines.Count - 1;
            int characters = lines[firstVisible].text.Length;

            while (firstVisible > oldestAllowedLine && characters + lines[firstVisible - 1].text.Length <= maxVisibleCharacters)
            {
                firstVisible--;
                characters += lines[firstVisible].text.Length;
            }

            return firstVisible;
        }

        private void AppendLine(LogLine line)
        {
            textBuilder.Append(COLOR_OPEN).Append(HexOf(line.severity)).Append('>');
            textBuilder.Append(NO_PARSE_OPEN);
            textBuilder.Append(line.text.Replace(NO_PARSE_CLOSE, ESCAPED_NO_PARSE_CLOSE));
            textBuilder.Append(NO_PARSE_CLOSE);
            textBuilder.Append(COLOR_CLOSE).Append('\n');
        }

        private string HexOf(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Warning:
                    return warningHex;
                case LogSeverity.Error:
                    return errorHex;
                default:
                    return infoHex;
            }
        }
    }
}
