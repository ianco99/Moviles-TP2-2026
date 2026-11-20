using System.Collections.Generic;
using _Game.Scripts.Shop;
using ianco99.ToolBox.Services;
using UnityEngine;

namespace _Game.Scripts.Economy
{
    public class PlayerEquipment : IService
    {
        private const string KEY_PREFIX = "Equipped.";

        public bool IsPersistance => true;

        private readonly ShopCatalog catalog;
        private readonly Dictionary<ShopCategory, ShopItem> equipped = new Dictionary<ShopCategory, ShopItem>();

        public PlayerEquipment(ShopCatalog catalog)
        {
            this.catalog = catalog;
        }

        public void Equip(ShopItem item)
        {
            equipped[item.Category] = item;
            PlayerPrefs.SetString(KEY_PREFIX + item.Category, item.Id);
            PlayerPrefs.Save();
        }

        public bool IsEquipped(ShopItem item)
        {
            return item != null && GetEquipped(item.Category) == item;
        }

        public ShopItem GetEquipped(ShopCategory category)
        {
            if (equipped.TryGetValue(category, out ShopItem item))
            {
                return item;
            }

            item = catalog.Find(category, PlayerPrefs.GetString(KEY_PREFIX + category, string.Empty));

            if (item == null)
            {
                item = catalog.GetDefault(category);
            }

            if (item != null)
            {
                equipped[category] = item;
            }

            return item;
        }
    }
}
