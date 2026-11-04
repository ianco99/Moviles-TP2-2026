using System.Collections.Generic;
using _Game.Scripts.Shop;
using ianco99.ToolBox.Services;
using UnityEngine;

namespace _Game.Scripts.Economy
{
    // Lo que el jugador tiene equipado en cada categoría. Se guarda en PlayerPrefs
    // (el id del ShopItem) para que sobreviva a cerrar la app.
    public class PlayerEquipment : IService
    {
        private const string KeyPrefix = "Equipped.";

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
            PlayerPrefs.SetString(KeyPrefix + item.Category, item.Id);
            PlayerPrefs.Save();
        }

        public bool IsEquipped(ShopItem item)
        {
            return item != null && GetEquipped(item.Category) == item;
        }

        // Devuelve null si no hay nada equipado en esa categoría (salvo armas, que caen en DefaultGun).
        public ShopItem GetEquipped(ShopCategory category)
        {
            if (equipped.TryGetValue(category, out ShopItem item))
            {
                return item;
            }

            item = catalog.Find(category, PlayerPrefs.GetString(KeyPrefix + category, string.Empty));

            if (item == null && category == ShopCategory.Guns)
            {
                item = catalog.DefaultGun;
            }

            if (item != null)
            {
                equipped[category] = item;
            }

            return item;
        }
    }
}
