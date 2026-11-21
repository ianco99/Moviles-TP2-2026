using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GunJump.UI
{
    public class LevelButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text numberLabel;
        [SerializeField] private Graphic[] stars;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Color starOn = new Color(1f, 0.82f, 0.2f);
        [SerializeField] private Color starOff = new Color(1f, 1f, 1f, 0.25f);
        [Tooltip("Opacidad del botón mientras el nivel está bloqueado.")]
        [Range(0f, 1f)]
        [SerializeField] private float lockedAlpha = 0.4f;

        public Button Button => button;

        public void Show(int levelNumber, int earnedStars, bool isUnlocked)
        {
            numberLabel.text = levelNumber.ToString();
            button.interactable = isUnlocked;
            canvasGroup.alpha = isUnlocked ? 1f : lockedAlpha;

            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].color = i < earnedStars ? starOn : starOff;
            }
        }
    }
}
