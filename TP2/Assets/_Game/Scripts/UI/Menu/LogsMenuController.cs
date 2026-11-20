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
        private const string EXPORTED_TEXT = "Guardado en ";
        private const string EXPORT_FAILED_TEXT = "No se pudo exportar el archivo.";
        private const string CLEARED_TEXT = "Logs borrados.";

        private static readonly string[] SEVERITY_LABELS = { "TODOS", "ADVERTENCIAS", "ERRORES" };

        [SerializeField] private TMP_Text logText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Button filterButton;
        [SerializeField] private TMP_Text filterLabel;
        [SerializeField] private Button exportButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private TMP_Text statusText;

        [Header("Display")]
        [Tooltip("Entradas que se muestran como máximo. El archivo exportado tiene todas las de la sesión.")]
        [SerializeField] private int maxVisibleLines = 200;
        [Tooltip("Tope de caracteres en pantalla, para que TextMeshPro no arme una malla enorme. Se muestran las entradas más nuevas.")]
        [SerializeField] private int maxVisibleCharacters = 15000;
        [SerializeField] private Color infoColor = Color.white;
        [SerializeField] private Color warningColor = new Color(1f, 0.84f, 0.2f);
        [SerializeField] private Color errorColor = new Color(1f, 0.42f, 0.42f);

        private readonly List<LogLine> lines = new List<LogLine>();
        private readonly StringBuilder textBuilder = new StringBuilder();
        private LogSeverity minimumSeverity = LogSeverity.Info;
        private string infoHex;
        private string warningHex;
        private string errorHex;

        private MobilePlatform MobilePlatform => ServiceProvider.Instance.GetService<MobilePlatform>();

        private void Awake()
        {
            infoHex = ColorUtility.ToHtmlStringRGB(infoColor);
            warningHex = ColorUtility.ToHtmlStringRGB(warningColor);
            errorHex = ColorUtility.ToHtmlStringRGB(errorColor);

            filterButton.onClick.AddListener(FilterClicked);
            exportButton.onClick.AddListener(ExportClicked);
            clearButton.onClick.AddListener(ClearClicked);
        }

        public void WakeUp()
        {
            statusText.text = string.Empty;
            Refresh();
        }

        private void FilterClicked()
        {
            GameAudio.Play(SoundId.ButtonClick);
            minimumSeverity = minimumSeverity == LogSeverity.Error ? LogSeverity.Info : minimumSeverity + 1;
            Refresh();
        }

        private void ExportClicked()
        {
            GameAudio.Play(SoundId.ButtonClick);

            if (MobilePlatform.TryExportLogs(out string location))
            {
                statusText.text = EXPORTED_TEXT + location;
            }
            else
            {
                statusText.text = EXPORT_FAILED_TEXT;
            }

            Refresh();
        }

        private void ClearClicked()
        {
            GameAudio.Play(SoundId.ButtonClick);
            MobilePlatform.ClearLogs();
            statusText.text = CLEARED_TEXT;
            Refresh();
        }

        private void Refresh()
        {
            filterLabel.text = SEVERITY_LABELS[(int)minimumSeverity];
            MobilePlatform.ReadLogs(minimumSeverity, maxVisibleLines, lines);

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
            int firstVisible = lines.Count - 1;
            int characters = lines[firstVisible].text.Length;

            while (firstVisible > 0 && characters + lines[firstVisible - 1].text.Length <= maxVisibleCharacters)
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
