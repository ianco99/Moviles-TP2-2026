using System.Collections.Generic;
using _Game.Scripts.Shop;
using ianco99.ToolBox.Services;

namespace _Game.Scripts.Economy
{
    public class PlayerInventoryManager : IService
    {
        public bool IsPersistance => true;

        private readonly HashSet<ShopItem> owned = new HashSet<ShopItem>();
        private readonly Dictionary<ShopCategory, ShopItem> equipped = new Dictionary<ShopCategory, ShopItem>();

        public bool Owns(ShopItem item)
        {
            return owned.Contains(item);
        }

        public void Add(ShopItem item)
        {
            owned.Add(item);
        }

        public bool IsEquipped(ShopItem item)
        {
            return equipped.TryGetValue(item.Category, out ShopItem current) && current == item;
        }

        public void Equip(ShopItem item)
        {
            equipped[item.Category] = item;
        }

        public ShopItem GetEquipped(ShopCategory category)
        {
            return equipped.TryGetValue(category, out ShopItem item) ? item : null;
        }
    }
}
