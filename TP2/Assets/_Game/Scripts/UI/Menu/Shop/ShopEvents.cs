using ianco99.ToolBox.Events;

namespace _Game.Scripts.Shop
{
    // Eventos que levanta ShopMenuController en el EventBus. No llevan datos: quien los escucha
    // lee lo que necesite (por ejemplo, el arma equipada en PlayerEquipment).

    public struct ShopOpenedEvent : IEvent
    {
        public void Assign(params object[] parameters) { }

        public void Reset() { }
    }

    public struct ShopClosedEvent : IEvent
    {
        public void Assign(params object[] parameters) { }

        public void Reset() { }
    }

    public struct EquippedItemChangedEvent : IEvent
    {
        public void Assign(params object[] parameters) { }

        public void Reset() { }
    }
}
