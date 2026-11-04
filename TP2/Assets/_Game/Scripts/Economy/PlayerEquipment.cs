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

        // Si el jugador nunca eligió nada en esa categoría, cae en el default del catálogo (arma y skin);
        // devuelve null si la categoría no tiene default.
        public ShopItem GetEquipped(ShopCategory category)
        {
            if (equipped.TryGetValue(category, out ShopItem item))
            {
                return item;
            }

            item = catalog.Find(category, PlayerPrefs.GetString(KeyPrefix + category, string.Empty));

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
