using _Game.Scripts.Economy;
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
        }
    }
}
