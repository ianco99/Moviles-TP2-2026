using System.Collections.Generic;
using GunJump.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GunJump.UI
{
    public class LevelsMenuController : MonoBehaviour
    {
        [SerializeField] private LevelListSO levelListSO;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        [SerializeField] private Button levelButton;

        //TODO: Optimize with asset batching/asset  
        public void Data()
        {
            List<int> datita = levelListSO.levelBuildIDs;

            for (int i = 0; i < datita.Count; i++)
            {
                Button button = Instantiate(levelButton, gridLayoutGroup.transform);
            }
            
            
        }
    }
}