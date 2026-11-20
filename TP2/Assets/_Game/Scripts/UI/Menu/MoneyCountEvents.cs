using ianco99.ToolBox.Events;

namespace GunJump.UI
{
    public struct ShowMoneyCountEvent : IEvent
    {
        public void Assign(params object[] parameters) { }

        public void Reset() { }
    }

    public struct HideMoneyCountEvent : IEvent
    {
        public void Assign(params object[] parameters) { }

        public void Reset() { }
    }
}
