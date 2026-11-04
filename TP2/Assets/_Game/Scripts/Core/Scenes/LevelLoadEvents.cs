using ianco99.ToolBox.Events;

namespace GunJump.Core
{
    // Eventos que levanta LevelLoader en el EventBus, en este orden. No llevan datos: quien los escucha
    // lee lo que necesite (por ejemplo, lo equipado en PlayerEquipment).

    // Empieza a cargarse un nivel. Lo reciben los objetos de la escena que se va: los del nivel nuevo todavía no existen.
    public struct LevelLoadStartedEvent : IEvent
    {
        public void Assign(params object[] parameters) { }

        public void Reset() { }
    }

    // El nivel nuevo ya está activo (todos sus Awake corrieron) pero todavía no se ve y está congelado.
    // Lo que tenga que cargar algo antes de mostrarse llama a LevelLoader.Hold y, al terminar, a Release.
    public struct LevelSceneLoadedEvent : IEvent
    {
        public void Assign(params object[] parameters) { }

        public void Reset() { }
    }

    // Todo está listo: el nivel ya se ve y el tiempo corre.
    public struct LevelLoadFinishedEvent : IEvent
    {
        public void Assign(params object[] parameters) { }

        public void Reset() { }
    }
}
