using ianco99.ToolBox.Events;

namespace GunJump.Core
{
    public struct LevelLoadStartedEvent : IEvent
    {
        public void Assign(params object[] parameters) { }

        public void Reset() { }
    }

    public struct LevelSceneLoadedEvent : IEvent
    {
        public void Assign(params object[] parameters) { }

        public void Reset() { }
    }

    public struct LevelLoadFinishedEvent : IEvent
    {
        public void Assign(params object[] parameters) { }

        public void Reset() { }
    }
}
