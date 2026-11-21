using System;
using _Game.Scripts.Economy;
using _Game.Scripts.Shop;
using GunJump.Core;
using GunJump.Platform;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Game.Scripts.Bootstrap
{
    [DefaultExecutionOrder(-100)]
    public class Bootstrapper : MonoBehaviour
    {
        private const int TARGET_FRAME_RATE = 60;

        [SerializeField] private ShopCatalog catalog;
        [Tooltip("Escena de la pantalla de carga entre el menú y los niveles. Tiene que estar en Build Settings.")]
        [SerializeField] private string loadingScene = "Loading";
        [Tooltip("Clips y volúmenes del juego. Se crea y se asigna con GunJump > Audio > Setup Audio.")]
        [SerializeField] private AudioLibrary audioLibrary;

        private void Awake()
        {
            Application.targetFrameRate = TARGET_FRAME_RATE;

            ServiceProvider services = ServiceProvider.Instance;

            if (!services.ContainsService<MobilePlatform>())
            {
                services.AddService<MobilePlatform>(new MobilePlatform());
            }

            if (!services.ContainsService<EventBus>())
            {
                services.AddService<EventBus>(new EventBus());
            }

            if (!services.ContainsService<LevelLoader>())
            {
                services.AddService<LevelLoader>(new LevelLoader(services.GetService<EventBus>(), loadingScene));
            }

            if (!services.ContainsService<AudioService>())
            {
                if (audioLibrary != null)
                {
                    services.AddService<AudioService>(new AudioService(services.GetService<EventBus>(), audioLibrary));
                }
                else
                {
                    Debug.LogWarning("Bootstrapper: falta asignar el AudioLibrary; el juego corre sin sonido. Corré GunJump > Audio > Setup Audio.", this);
                }
            }

            if (!services.ContainsService<ContentLibrary>())
            {
                services.AddService<ContentLibrary>(new ContentLibrary());
            }

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
            PlayerLevelProgress levelProgress = new PlayerLevelProgress();

            EconomySave save = new EconomySave(wallet, inventory, levelProgress);
            save.Load();

            OwnEquippedItems(equipment, inventory);

            services.AddService<PlayerWalletManager>(wallet);
            services.AddService<PlayerInventoryManager>(inventory);
            services.AddService<PlayerEquipment>(equipment);
            services.AddService<PlayerLevelProgress>(levelProgress);
        }

#if UNITY_EDITOR
        private void Start()
        {
            string requestedScenePath = UnityEditor.SessionState.GetString(LevelLoader.EDITOR_START_SCENE_KEY, string.Empty);

            if (string.IsNullOrEmpty(requestedScenePath))
            {
                return;
            }

            UnityEditor.SessionState.EraseString(LevelLoader.EDITOR_START_SCENE_KEY);
            int buildIndex = SceneUtility.GetBuildIndexByScenePath(requestedScenePath);

            if (buildIndex < 0)
            {
                Debug.LogWarning($"Bootstrapper: {requestedScenePath} no está en Build Settings; se queda en el menú.", this);
                return;
            }

            ServiceProvider.Instance.GetService<LevelLoader>().Load(buildIndex);
        }
#endif

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
