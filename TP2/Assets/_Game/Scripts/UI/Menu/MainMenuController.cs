using UnityEngine;

namespace GunJump.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject landingMenu;
        [SerializeField] private GameObject levelSelectMenu;

        void Update()
        {
        
        }

        public void PlayClicked()
        {
            landingMenu.SetActive(false);
            levelSelectMenu.SetActive(true);
        }
    }
}
