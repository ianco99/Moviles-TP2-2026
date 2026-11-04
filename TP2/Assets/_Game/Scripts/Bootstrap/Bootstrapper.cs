using System;
using _Game.Scripts.Economy;
using _Game.Scripts.Shop;
using GunJump.Core;
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
        [Tooltip("Monedas con las que arranca una partida nueva. Solo se dan si todavía no hay billetera guardada.")]
        [SerializeField] private double startingMoney;
        [SerializeField] private ShopCatalog catalog;
        [Tooltip("Escena de la pantalla de carga entre el menú y los niveles. Tiene que estar en Build Settings.")]
        [SerializeField] private string loadingScene = "Loading";

        private void Awake()
        {
            ServiceProvider services = ServiceProvider.Instance;

            if (!services.ContainsService<EventBus>())
            {
                services.AddService<EventBus>(new EventBus());
            }

            if (!services.ContainsService<LevelLoader>())
            {
                services.AddService<LevelLoader>(new LevelLoader(services.GetService<EventBus>(), loadingScene));
            }

            if (!services.ContainsService<ContentLibrary>())
            {
                services.AddService<ContentLibrary>(new ContentLibrary());
            }

            // Los servicios de la economía viven toda la sesión: se crean y se cargan del guardado una sola vez.
            if (services.ContainsService<PlayerWalletManager>())
            {
                return;
            }

            if (catalog == null)
            {
                Debug.LogError("Bootstrapper: falta asignar el ShopCatalog; no se registran la billetera, el inventario ni el equipamiento.", this);
                return;
            }

            PlayerWalletManager wallet = new PlayerWalletManager();
            PlayerInventoryManager inventory = new PlayerInventoryManager(catalog);
            PlayerEquipment equipment = new PlayerEquipment(catalog);

            // Desde acá, cada cambio en la billetera o el inventario se guarda solo.
            EconomySave save = new EconomySave(wallet, inventory);

            if (!save.Load())
            {
                wallet.AddMoney(startingMoney);
            }

            OwnEquippedItems(equipment, inventory);

            services.AddService<PlayerWalletManager>(wallet);
            services.AddService<PlayerInventoryManager>(inventory);
            services.AddService<PlayerEquipment>(equipment);
        }

        // Lo equipado siempre figura como comprado. Cubre el arma por defecto, que se equipa sin comprarla,
        // y los guardados de antes de que se guardara el inventario. Si alguno era nuevo, el inventario lo guarda.
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
