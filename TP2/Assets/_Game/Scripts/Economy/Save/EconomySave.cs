using System;
using UnityEngine;

namespace _Game.Scripts.Economy
{
    // Cuidador del Memento: cada vez que la billetera o el inventario cambian, les pide una foto y la guarda
    // en PlayerPrefs como JSON. Se escribe a disco en el momento, así que cerrar la app de golpe no pierde nada.
    // No sabe qué hay adentro de cada foto: solo la guarda y se la devuelve a quien la armó.
    public sealed class EconomySave
    {
        private const string WalletKey = "Save.Wallet";
        private const string InventoryKey = "Save.Inventory";

        private readonly PlayerWalletManager wallet;
        private readonly PlayerInventoryManager inventory;

        public EconomySave(PlayerWalletManager wallet, PlayerInventoryManager inventory)
        {
            this.wallet = wallet;
            this.inventory = inventory;

            // Restore no avisa cambios, así que cargar no vuelve a escribir lo que se acaba de leer.
            wallet.Changed += SaveWallet;
            inventory.Changed += SaveInventory;
        }

        // Devuelve false si no había billetera guardada: es la primera vez que se abre el juego.
        public bool Load()
        {
            InventorySnapshot savedInventory = Read<InventorySnapshot>(InventoryKey);

            if (savedInventory != null)
            {
                inventory.Restore(savedInventory);
            }

            WalletSnapshot savedWallet = Read<WalletSnapshot>(WalletKey);

            if (savedWallet == null)
            {
                return false;
            }

            wallet.Restore(savedWallet);
            return true;
        }

        private void SaveWallet()
        {
            Write(WalletKey, wallet.CreateSnapshot());
        }

        private void SaveInventory()
        {
            Write(InventoryKey, inventory.CreateSnapshot());
        }

        private static void Write<T>(string key, T snapshot)
        {
            PlayerPrefs.SetString(key, JsonUtility.ToJson(snapshot));
            PlayerPrefs.Save();
        }

        // Devuelve null si no hay nada guardado o si el JSON está dañado.
        private static T Read<T>(string key) where T : class
        {
            string json = PlayerPrefs.GetString(key, string.Empty);

            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
