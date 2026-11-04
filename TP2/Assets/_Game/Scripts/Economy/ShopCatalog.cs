using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.Shop
{
    // Todo lo que se vende en la tienda, por categoría. Lo usan la tienda para listar
    // y PlayerEquipment para convertir el id guardado de vuelta en un ShopItem.
    [CreateAssetMenu(menuName = "GunJump/Shop Catalog", fileName = "ShopCatalog")]
    public sealed class ShopCatalog : ScriptableObject
    {
        [SerializeField] private List<ShopItem> guns = new List<ShopItem>();
        [SerializeField] private List<ShopItem> gunSkins = new List<ShopItem>();
        [SerializeField] private List<ShopItem> bullets = new List<ShopItem>();
        [SerializeField] private List<ShopItem> shotVfx = new List<ShopItem>();

        [Tooltip("Arma equipada cuando el jugador todavía no eligió ninguna.")]
        [SerializeField] private ShopItem defaultGun;

        [Tooltip("Skin equipada cuando el jugador todavía no eligió ninguna. Vacío = el arma con sus propios materiales.")]
        [SerializeField] private ShopItem defaultSkin;

        // Lo que queda equipado en una categoría si el jugador nunca eligió nada; null si esa categoría no tiene.
        public ShopItem GetDefault(ShopCategory category)
        {
            switch (category)
            {
                case ShopCategory.Guns:
                    return defaultGun;
                case ShopCategory.GunSkins:
                    return defaultSkin;
                default:
                    return null;
            }
        }

        public IReadOnlyList<ShopItem> GetItems(ShopCategory category)
        {
            switch (category)
            {
                case ShopCategory.Guns:
                    return guns;
                case ShopCategory.GunSkins:
                    return gunSkins;
                case ShopCategory.Bullets:
                    return bullets;
                case ShopCategory.ShotVFX:
                    return shotVfx;
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
        }

        public ShopItem Find(ShopCategory category, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            IReadOnlyList<ShopItem> items = GetItems(category);

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].Id == id)
                {
                    return items[i];
                }
            }

            return null;
        }
    }
}
