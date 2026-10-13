using GunJump.Core;
using UnityEngine;

namespace GunJump.Gameplay
{
    // Conecta a mano las piezas de la escena de juego: sin Singleton ni Service Locator.
    public sealed class GameplayInstaller : MonoBehaviour
    {
        [SerializeField] Gun gun;
        [SerializeField] InputReader input;

        CommandInvoker invoker;

        void Awake()
        {
            invoker = new CommandInvoker();
            input.Init(invoker, new FireCommand(gun));
        }
    }
}
