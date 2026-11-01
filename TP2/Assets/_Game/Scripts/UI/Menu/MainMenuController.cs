using System;
using System.Collections;
using System.Threading.Tasks;
using _Game.Scripts.Shop;
using GunJump.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GunJump.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button shopButton;
        
        [Header("References")]
        [SerializeField] private GameObject landingMenu;
        [SerializeField] private LevelsMenuController levelSelectMenu;
        [SerializeField] private ShopMenuController shopMenu;

        private bool picked = false;
        
        private void Awake()
        {
            playButton.onClick.AddListener(PlayClicked);
            shopButton.onClick.AddListener(ShopClicked);
            
            levelSelectMenu.Data();
            shopMenu.Setup();
        }

        private void ShopClicked()
        {
            if(!picked)
            StartCoroutine(WaitForAnim("Shop"));
        }

        public void PlayClicked()
        {
            if(!picked)
            StartCoroutine(WaitForAnim("Play"));
        }

        private IEnumerator WaitForAnim(string key)
        {
            picked = true;
            yield return new WaitForSeconds(0.1f);
            ActivateKey(key);
            picked = false;
        }

        private void ActivateKey(string key)
        {
            switch (key)
            {
                case "Play":
                    landingMenu.SetActive(false);
                    levelSelectMenu.gameObject.SetActive(true);
                    break;
                case "Shop":
                    landingMenu.SetActive(false);
                    shopMenu.gameObject.SetActive(true);
                    shopMenu.WakeUp();
                    break;
            }
        }
    }
}
