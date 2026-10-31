using GunJump.Core;
using UnityEngine;

namespace GunJump.Gameplay
{
    // Solo para la escena Prototype: conecta a mano sus piezas, sin Singleton ni Service Locator.
    // Los niveles reales se conectan en LevelCore.
    public sealed class GameplayInstaller : MonoBehaviour
    {
        // El prototipo no tiene objetivos: munición infinita y ningún enemigo que contar.
        const int InfiniteAmmo = 0;
        const int TrackedEnemies = 0;

        [SerializeField] Gun gun;
        [SerializeField] InputReader input;

        CommandInvoker invoker;

        void Awake()
        {
            invoker = new CommandInvoker();
            input.Init(invoker, new FireCommand(gun, new LevelSession(InfiniteAmmo, TrackedEnemies)));
        }
    }
}
