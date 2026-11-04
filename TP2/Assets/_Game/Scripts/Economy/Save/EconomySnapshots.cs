using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.Economy
{
    // Mementos de la economía: fotos de la billetera y del inventario. Fuera de este assembly nadie puede
    // leerlas ni modificarlas; EconomySave solo las pasa a JSON y las vuelve a levantar.
    [Serializable]
    public sealed class WalletSnapshot
    {
        [SerializeField] internal int version = 1;
        [SerializeField] internal double money;
    }

    [Serializable]
    public sealed class InventorySnapshot
    {
        [SerializeField] internal int version = 1;
        [SerializeField] internal List<OwnedItem> items = new List<OwnedItem>();
    }

    // La categoría va por nombre y no por número: reordenar ShopCategory no rompe los guardados.
    [Serializable]
    internal sealed class OwnedItem
    {
        public string category;
        public string id;
    }
}
