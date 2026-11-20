using UnityEngine;

namespace GunJump.Core
{
    public abstract class SceneEntryPoint : MonoBehaviour
    {
        public abstract void Enter(GameContext context);
    }
}
