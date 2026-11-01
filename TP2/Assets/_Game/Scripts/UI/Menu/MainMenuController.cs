using System;
using System.Collections;
using System.Threading.Tasks;
using GunJump.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GunJump.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        
        [SerializeField] private GameObject landingMenu;
        [SerializeField] private LevelsMenuController levelSelectMenu;

        private void Awake()
        {
            playButton.onClick.AddListener(PlayClicked);
            
            levelSelectMenu.Data();
        }

        public void PlayClicked()
        {
            StartCoroutine(Play());
        }

        private IEnumerator Play()
        {
            yield return new WaitForSeconds(0.1f);
            landingMenu.SetActive(false);
            levelSelectMenu.gameObject.SetActive(true);
        }
    }
}
