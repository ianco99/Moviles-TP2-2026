using ianco99.ToolBox.Events;

namespace GunJump.UI
{
    // Cualquier menú puede levantar estos eventos en el EventBus para mostrar u ocultar el contador de dinero.
    // No llevan datos, así que Assign y Reset quedan vacíos.
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
