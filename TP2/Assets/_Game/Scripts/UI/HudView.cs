using TMPro;
using UnityEngine;

namespace GunJump.UI
{
    // Muestra lo que le digan. Solo toca el texto cuando el valor cambia, así se la puede llamar en cada frame.
    public sealed class HudView : MonoBehaviour
    {
        const int Unset = int.MinValue;

        [SerializeField] TMP_Text ammoLabel;
        [SerializeField] TMP_Text coinsLabel;
        [SerializeField] TMP_Text enemiesLabel;
        [SerializeField] CommandButton pauseButton;

        int shownAmmo = Unset;
        int shownCoins = Unset;
        int shownEnemies = Unset;

        public CommandButton PauseButton => pauseButton;

        // Con munición infinita el contador se oculta.
        public void SetAmmo(int ammo, bool infinite)
        {
            if (ammoLabel.gameObject.activeSelf == infinite)
                ammoLabel.gameObject.SetActive(!infinite);

            if (infinite || ammo == shownAmmo)
                return;

            shownAmmo = ammo;
            ammoLabel.SetText("Balas {0}", ammo);
        }

        public void SetCoins(int coins)
        {
            if (coins == shownCoins)
                return;

            shownCoins = coins;
            coinsLabel.SetText("Monedas {0}", coins);
        }

        public void SetEnemiesRemaining(int remaining)
        {
            if (remaining == shownEnemies)
                return;

            shownEnemies = remaining;
            enemiesLabel.SetText("Enemigos {0}", remaining);
        }
    }
}
