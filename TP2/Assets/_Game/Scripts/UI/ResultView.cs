using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GunJump.UI
{
    public readonly struct ResultData
    {
        public readonly bool Won;
        public readonly int Stars;
        public readonly int Coins;
        public readonly int Multiplier;
        public readonly bool HasNext;

        public ResultData(bool won, int stars, int coins, int multiplier, bool hasNext)
        {
            Won = won;
            Stars = stars;
            Coins = coins;
            Multiplier = multiplier;
            HasNext = hasNext;
        }
    }

    public sealed class ResultView : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text coinsLabel;
        [SerializeField] TMP_Text multiplierLabel;
        [SerializeField] GameObject starsRow;
        [SerializeField] Graphic[] stars;
        [SerializeField] Color starOn = new Color(1f, 0.82f, 0.2f);
        [SerializeField] Color starOff = new Color(1f, 1f, 1f, 0.2f);
        [SerializeField] CommandButton nextButton;
        [SerializeField] CommandButton retryButton;

        public CommandButton NextButton => nextButton;
        public CommandButton RetryButton => retryButton;

        public void Hide()
        {
            panel.SetActive(false);
        }

        public void Show(in ResultData data)
        {
            panel.SetActive(true);
            titleLabel.text = data.Won ? "¡Nivel completado!" : "Intento fallido";

            starsRow.SetActive(data.Won);
            for (int i = 0; i < stars.Length; i++)
                stars[i].color = i < data.Stars ? starOn : starOff;

            coinsLabel.gameObject.SetActive(data.Won);
            multiplierLabel.gameObject.SetActive(data.Won);
            if (data.Won)
            {
                coinsLabel.SetText("+{0} monedas", data.Coins);
                multiplierLabel.SetText("Multiplicador x{0}", data.Multiplier);
            }

            nextButton.gameObject.SetActive(data.HasNext);
        }
    }
}
