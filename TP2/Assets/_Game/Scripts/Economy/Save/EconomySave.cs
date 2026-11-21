using System;
using UnityEngine;

namespace _Game.Scripts.Economy
{
    // Cuidador del Memento: cada vez que la billetera o el inventario cambian, les pide una foto y la guarda
    // en PlayerPrefs como JSON. Se escribe a disco en el momento, así que cerrar la app de golpe no pierde nada.
    // No sabe qué hay adentro de cada foto: solo la guarda y se la devuelve a quien la armó.
    public sealed class EconomySave
    {
        private const string WALLET_KEY = "Save.Wallet";
        private const string INVENTORY_KEY = "Save.Inventory";
        private const string LEVELS_KEY = "Save.Levels";

        private readonly PlayerWalletManager wallet;
        private readonly PlayerInventoryManager inventory;
        private readonly PlayerLevelProgress levelProgress;

        public EconomySave(PlayerWalletManager wallet, PlayerInventoryManager inventory, PlayerLevelProgress levelProgress)
        {
            this.wallet = wallet;
            this.inventory = inventory;
            this.levelProgress = levelProgress;

            wallet.Changed += SaveWallet;
            inventory.Changed += SaveInventory;
            levelProgress.Changed += SaveLevelProgress;
        }

        public bool Load()
        {
            InventorySnapshot savedInventory = Read<InventorySnapshot>(INVENTORY_KEY);

            if (savedInventory != null)
            {
                inventory.Restore(savedInventory);
            }

            LevelProgressSnapshot savedLevels = Read<LevelProgressSnapshot>(LEVELS_KEY);

            if (savedLevels != null)
            {
                levelProgress.Restore(savedLevels);
            }

            WalletSnapshot savedWallet = Read<WalletSnapshot>(WALLET_KEY);

            if (savedWallet == null)
            {
                return false;
            }

            wallet.Restore(savedWallet);
            return true;
        }

        private void SaveWallet()
        {
            Write(WALLET_KEY, wallet.CreateSnapshot());
        }

        private void SaveInventory()
        {
            Write(INVENTORY_KEY, inventory.CreateSnapshot());
        }

        private void SaveLevelProgress()
        {
            Write(LEVELS_KEY, levelProgress.CreateSnapshot());
        }

        private static void Write<SnapshotType>(string key, SnapshotType snapshot)
        {
            PlayerPrefs.SetString(key, JsonUtility.ToJson(snapshot));
            PlayerPrefs.Save();
        }

        private static SnapshotType Read<SnapshotType>(string key) where SnapshotType : class
        {
            string json = PlayerPrefs.GetString(key, string.Empty);

            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<SnapshotType>(json);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
