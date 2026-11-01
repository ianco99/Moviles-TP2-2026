using System.Collections.Generic;
using UnityEngine;

namespace GunJump.Core
{
    [CreateAssetMenu(fileName = "LevelListSO", menuName = "ScriptableObjects/Levels/LevelListSO")]
    public class LevelListSO : ScriptableObject
    {
        public List<int> levelBuildIDs;
    }
}
