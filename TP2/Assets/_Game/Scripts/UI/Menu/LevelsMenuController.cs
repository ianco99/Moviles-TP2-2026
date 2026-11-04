using System.Collections.Generic;
using GunJump.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GunJump.UI
{
    public class LevelsMenuController : MonoBehaviour
    {
        [SerializeField] private LevelListSO levelListSO;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        [SerializeField] private Button levelButton;

        private bool isLoading;

        //TODO: Optimize with asset batching/asset
        public void Setup()
        {
            List<int> buildIndices = levelListSO.levelBuildIDs;

            for (int i = 0; i < buildIndices.Count; i++)
            {
                int levelNumber = i + 1;
                int buildIndex = buildIndices[i];

                Button button = Instantiate(levelButton, gridLayoutGroup.transform);
                button.GetComponentInChildren<TMP_Text>().text = levelNumber.ToString();

                if (!IsInBuildSettings(buildIndex))
                {
                    Debug.LogError($"LevelListSO: el nivel {levelNumber} apunta al índice de build {buildIndex}, que no está en Build Settings.", levelListSO);
                    button.interactable = false;
                    continue;
                }

                button.onClick.AddListener(() => LoadLevel(buildIndex));
            }
        }

        // Todas las cargas de nivel pasan por acá. Después va la carga asíncrona con pantalla de transición.
        private void LoadLevel(int buildIndex)
        {
            if (isLoading)
            {
                return;
            }

            isLoading = true;
            SceneManager.LoadScene(buildIndex);
        }

        private static bool IsInBuildSettings(int buildIndex)
        {
            return buildIndex >= 0 && buildIndex < SceneManager.sceneCountInBuildSettings;
        }
    }
}
