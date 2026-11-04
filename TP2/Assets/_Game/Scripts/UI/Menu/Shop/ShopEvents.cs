using ianco99.ToolBox.Events;

namespace _Game.Scripts.Shop
{
    // Lo levanta ShopMenuController en el EventBus. No lleva datos: quien lo escucha
    // lee lo que necesite (por ejemplo, el arma equipada en PlayerEquipment).
    public struct EquippedItemChangedEvent : IEvent
    {
        public void Assign(params object[] parameters) { }

        public void Reset() { }
    }
}
