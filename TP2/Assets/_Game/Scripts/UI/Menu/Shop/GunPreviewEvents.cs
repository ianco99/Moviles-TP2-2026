using ianco99.ToolBox.Events;

namespace _Game.Scripts.Shop
{
    // El menú principal levanta estos eventos en el EventBus para mostrar u ocultar la vista previa del arma
    // (se ve en el menú y en la tienda; en la selección de nivel no). No llevan datos, así que Assign y Reset quedan vacíos.
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
