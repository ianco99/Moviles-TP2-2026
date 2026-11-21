using System;
using System.Collections.Generic;
using _Game.Scripts.Shop;
using UnityEngine;

namespace GunJump.Gameplay
{
    [CreateAssetMenu(menuName = "GunJump/Weapon Catalog", fileName = "WeaponCatalog")]
    public sealed class WeaponCatalog : ScriptableObject
    {
        [Tooltip("Cómo dispara cada arma de la tienda.")]
        [SerializeField] List<Entry> entries = new List<Entry>();
        [Tooltip("Estrategia para un arma que no está en la lista.")]
        [SerializeField] FireStrategy fallbackStrategy;

        public FireStrategy StrategyFor(ShopItem gun)
        {
            if (gun == null)
                return fallbackStrategy;

            foreach (Entry entry in entries)
            {
                if (entry.gun == gun && entry.strategy != null)
                    return entry.strategy;
            }

            return fallbackStrategy;
        }

        [Serializable]
        struct Entry
        {
            public ShopItem gun;
            public FireStrategy strategy;
        }
    }
}
