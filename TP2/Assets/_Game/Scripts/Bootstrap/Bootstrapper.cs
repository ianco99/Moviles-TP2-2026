using System;
using _Game.Scripts.Economy;
using _Game.Scripts.Shop;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using UnityEngine;

namespace _Game.Scripts.Bootstrap
{
    // Corre antes que los menús para que encuentren los servicios registrados en su Awake.
    [DefaultExecutionOrder(-100)]
    public class Bootstrapper : MonoBehaviour
    {
        //TODO: Remove once levels pay into the wallet
        [SerializeField] private double startingMoney;
        [SerializeField] private ShopCatalog catalog;

        private void Awake()
        {
            ServiceProvider services = ServiceProvider.Instance;

            if (!services.ContainsService<EventBus>())
            {
                services.AddService<EventBus>(new EventBus());
            }

            if (!services.ContainsService<PlayerWalletManager>())
            {
                PlayerWalletManager wallet = new PlayerWalletManager();
                wallet.AddMoney(startingMoney);
                services.AddService<PlayerWalletManager>(wallet);
            }

            if (!services.ContainsService<PlayerInventoryManager>())
            {
                services.AddService<PlayerInventoryManager>(new PlayerInventoryManager());
            }

            if (!services.ContainsService<PlayerEquipment>())
            {
                if (catalog == null)
                {
                    Debug.LogError("Bootstrapper: falta asignar el ShopCatalog; PlayerEquipment no se registra.", this);
                    return;
                }

                PlayerEquipment equipment = new PlayerEquipment(catalog);
                services.AddService<PlayerEquipment>(equipment);
                OwnEquippedItems(equipment, services.GetService<PlayerInventoryManager>());
            }
        }

        //TODO: Remove once owned items are saved too. Hasta entonces, lo equipado (que sí se guarda)
        // tiene que figurar como comprado, si no la tienda lo mostraría equipado y con precio.
        private static void OwnEquippedItems(PlayerEquipment equipment, PlayerInventoryManager inventory)
        {
            foreach (ShopCategory category in Enum.GetValues(typeof(ShopCategory)))
            {
                ShopItem item = equipment.GetEquipped(category);

                if (item != null)
                {
                    inventory.Add(item);
                }
            }
        }
    }
}
