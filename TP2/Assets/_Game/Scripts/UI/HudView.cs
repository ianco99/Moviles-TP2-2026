using TMPro;
using UnityEngine;

namespace GunJump.UI
{
    public sealed class HudView : MonoBehaviour
    {
        const int UNSET = int.MinValue;

        [SerializeField] TMP_Text ammoLabel;
        [SerializeField] TMP_Text coinsLabel;
        [SerializeField] TMP_Text enemiesLabel;
        [SerializeField] CommandButton pauseButton;
        [Tooltip("Cartel del tutorial. Lo muestran LevelSettings al empezar y los TutorialZone del nivel.")]
        [SerializeField] GameObject hintPanel;
        [SerializeField] TMP_Text hintLabel;
        [Tooltip("Círculo que late para indicar que hay que tocar la pantalla.")]
        [SerializeField] GameObject tapIndicator;

        int shownAmmo = UNSET;
        int shownCoins = UNSET;
        int shownEnemies = UNSET;

        public CommandButton PauseButton => pauseButton;

        public void SetAmmo(int ammo, bool infinite)
        {
            if (ammoLabel.gameObject.activeSelf == infinite)
                ammoLabel.gameObject.SetActive(!infinite);

            if (infinite || ammo == shownAmmo)
                return;

            shownAmmo = ammo;
            ammoLabel.SetText("Ammo {0}", ammo);
        }

        public void SetCoins(int coins)
        {
            if (coins == shownCoins)
                return;

            shownCoins = coins;
            coinsLabel.SetText("Coins {0}", coins);
        }

        public void ShowHint(string text, bool showTap)
        {
            hintLabel.text = text;
            tapIndicator.SetActive(showTap);
            hintPanel.SetActive(true);
        }

        public void HideHint()
        {
            hintPanel.SetActive(false);
            tapIndicator.SetActive(false);
        }

        public void SetEnemiesRemaining(int remaining)
        {
            if (remaining == shownEnemies)
                return;

            shownEnemies = remaining;
            enemiesLabel.SetText("Enemies {0}", remaining);
        }
    }
}
