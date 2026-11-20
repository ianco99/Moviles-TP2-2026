using ianco99.ToolBox.Events;

namespace _Game.Scripts.Shop
{
    public struct ShowGunPreviewEvent : IEvent
    {
        public void Assign(params object[] parameters) { }

        public void Reset() { }
    }

    public struct HideGunPreviewEvent : IEvent
    {
        public void Assign(params object[] parameters) { }

        public void Reset() { }
    }
}
