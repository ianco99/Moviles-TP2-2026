using System.Collections.Generic;
using _Game.Scripts.Shop;
using ianco99.ToolBox.Services;

namespace _Game.Scripts.Economy
{
    // Lo que el jugador compró. Lo equipado vive en PlayerEquipment.
    public class PlayerInventoryManager : IService
    {
        public bool IsPersistance => true;

        private readonly HashSet<ShopItem> owned = new HashSet<ShopItem>();

        public bool Owns(ShopItem item)
        {
            return owned.Contains(item);
        }

        public void Add(ShopItem item)
        {
            owned.Add(item);
        }
    }
}
