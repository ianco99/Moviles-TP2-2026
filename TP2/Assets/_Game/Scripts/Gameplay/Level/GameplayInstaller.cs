using GunJump.Core;
using UnityEngine;

namespace GunJump.Gameplay
{
    public sealed class GameplayInstaller : MonoBehaviour
    {
        const int INFINITE_AMMO = 0;
        const int TRACKED_ENEMIES = 0;

        [SerializeField] Gun gun;
        [SerializeField] InputReader input;

        CommandInvoker invoker;

        void Awake()
        {
            invoker = new CommandInvoker();
            input.Init(invoker, new FireCommand(gun, new LevelSession(INFINITE_AMMO, TRACKED_ENEMIES)));
        }
    }
}
