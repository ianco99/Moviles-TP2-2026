using UnityEngine;

namespace GunJump.Core
{
    // Punto de entrada de una escena de contenido. SceneLoader lo busca al terminar de cargar
    // y le entrega el contexto: así las escenas reciben sus dependencias sin Singleton.
    public abstract class SceneEntryPoint : MonoBehaviour
    {
        public abstract void Enter(GameContext context);
    }
}
