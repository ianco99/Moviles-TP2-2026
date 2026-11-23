using System.Collections;
using System.Collections.Generic;
using System.IO;
using _Game.Scripts.Economy;
using GunJump.Core;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GunJump.UI
{
    public class LevelsMenuController : MonoBehaviour
    {
        [SerializeField] private LevelListSO levelListSO;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        [Tooltip("Nombre del prefab del botón de nivel en el AssetBundle 'prefabs' (carpeta Prefabs/Runtime). Se arma con GunJump > UI > Setup Level Select.")]
        [SerializeField] private string levelButtonAssetName = "LevelButton";

        private LevelLoader levelLoader;
        private PlayerLevelProgress levelProgress;
        private ContentLibrary content;

        private LevelLoader LevelLoader => levelLoader ??= ServiceProvider.Instance.GetService<LevelLoader>();
        private PlayerLevelProgress LevelProgress => levelProgress ??= ServiceProvider.Instance.GetService<PlayerLevelProgress>();
        private ContentLibrary Content => content ??= ServiceProvider.Instance.GetService<ContentLibrary>();

        //TODO: Optimize with asset batching/asset
        public IEnumerator Setup()
        {
            LevelButtonView levelButton = null;
            ContentBundle bundle = Content.Get(ContentLibrary.PREFABS_BUNDLE);
            yield return bundle.LoadBundle();
            yield return bundle.LoadAsset<GameObject>(levelButtonAssetName, loaded => levelButton = loaded != null ? loaded.GetComponent<LevelButtonView>() : null);

            if (levelButton == null)
            {
                Debug.LogError($"LevelsMenuController: el AssetBundle '{ContentLibrary.PREFABS_BUNDLE}' no tiene un prefab '{levelButtonAssetName}' con LevelButtonView.", this);
                yield break;
            }

            List<int> buildIndices = levelListSO.levelBuildIDs;
            bool isPreviousCompleted = true;

            for (int i = 0; i < buildIndices.Count; i++)
            {
                int levelNumber = i + 1;
                int buildIndex = buildIndices[i];

                LevelButtonView button = Instantiate(levelButton, gridLayoutGroup.transform);

                if (!IsInBuildSettings(buildIndex))
                {
                    Debug.LogError($"LevelListSO: el nivel {levelNumber} apunta al índice de build {buildIndex}, que no está en Build Settings.", levelListSO);
                    button.Show(levelNumber, 0, false);
                    isPreviousCompleted = false;
                    continue;
                }

                string levelID = LevelIDOf(buildIndex);
                button.Show(levelNumber, LevelProgress.StarsFor(levelID), isPreviousCompleted);
                isPreviousCompleted = LevelProgress.IsCompleted(levelID);

                button.Button.onClick.AddListener(() => LoadLevel(buildIndex));
            }
        }

        private void LoadLevel(int buildIndex)
        {
            GameAudio.Play(SoundId.ButtonClick);
            LevelLoader.Load(buildIndex);
        }

        private static bool IsInBuildSettings(int buildIndex)
        {
            return buildIndex >= 0 && buildIndex < SceneManager.sceneCountInBuildSettings;
        }

        private static string LevelIDOf(int buildIndex)
        {
            return Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(buildIndex));
        }
    }
}
