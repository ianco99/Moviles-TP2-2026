using System.Collections;
using System.Collections.Generic;
using _Game.Scripts.Economy;
using GunJump.Core;
using GunJump.Platform;
using GunJump.UI;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Shop
{
    public class ShopMenuController : MonoBehaviour
    {
        [Tooltip("Nombre del prefab del item de la tienda en el AssetBundle 'prefabs' (carpeta Prefabs/Runtime).")]
        [SerializeField] private string itemAssetName = "ShopItem";
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField] private int initialPoolSize = 20;

        [Header("Items")]
        [SerializeField] private ShopCatalog catalog;

        private const int ARSENAL_GUN_COUNT = 3;

        private readonly List<ShopItemView> pool = new List<ShopItemView>();
        private ShopCategory currentCategory = ShopCategory.Guns;
        private ShopItemView itemPrefab;
        private bool isShowPending;
        private ContentLibrary content;
        private PlayerWalletManager wallet;
        private PlayerInventoryManager inventory;
        private PlayerEquipment equipment;
        private EventBus eventBus;

        private PlayerWalletManager Wallet => wallet ??= ServiceProvider.Instance.GetService<PlayerWalletManager>();
        private PlayerInventoryManager Inventory => inventory ??= ServiceProvider.Instance.GetService<PlayerInventoryManager>();
        private PlayerEquipment Equipment => equipment ??= ServiceProvider.Instance.GetService<PlayerEquipment>();
        private EventBus EventBus => eventBus ??= ServiceProvider.Instance.GetService<EventBus>();
        private MobilePlatform MobilePlatform => ServiceProvider.Instance.GetService<MobilePlatform>();
        private ContentLibrary Content => content ??= ServiceProvider.Instance.GetService<ContentLibrary>();

        public IEnumerator Setup()
        {
            if (itemPrefab != null)
            {
                yield break;
            }

            ContentBundle bundle = Content.Get(ContentLibrary.PREFABS_BUNDLE);
            yield return bundle.LoadBundle();
            yield return bundle.LoadAsset<GameObject>(itemAssetName, loaded => itemPrefab = loaded != null ? loaded.GetComponent<ShopItemView>() : null);

            if (itemPrefab == null)
            {
                Debug.LogError($"ShopMenuController: el AssetBundle '{ContentLibrary.PREFABS_BUNDLE}' no tiene un prefab '{itemAssetName}' con ShopItemView.", this);
                yield break;
            }

            for (int i = 0; i < initialPoolSize; i++)
            {
                AddToPool();
            }

            if (isShowPending)
            {
                isShowPending = false;
                ShowCategory();
            }
        }

        public void WakeUp()
        {
            ShowCategory();
        }

        public void ShowCategory()
        {
            if (itemPrefab == null)
            {
                isShowPending = true;
                return;
            }

            ShowItems(catalog.GetItems(currentCategory));
        }

        public void SelectCategory(int category)
        {
            currentCategory = (ShopCategory)category;
            GameAudio.Play(SoundId.ShopTab);
            ShowCategory();
        }

        private void ShowItems(IReadOnlyList<ShopItem> items)
        {
            while (pool.Count < items.Count)
            {
                AddToPool();
            }

            for (int i = 0; i < pool.Count; i++)
            {
                bool isUsed = i < items.Count;

                if (isUsed)
                {
                    pool[i].Bind(items[i]);
                }

                pool[i].gameObject.SetActive(isUsed);
            }

            RefreshStates();
            ResizeContent(items.Count);
        }

        private void AddToPool()
        {
            ShopItemView view = Instantiate(itemPrefab, scrollRect.content);
            view.gameObject.SetActive(false);
            view.Clicked += OnItemClicked;
            pool.Add(view);
        }

        private void OnItemClicked(ShopItemView view)
        {
            ShopItem item = view.Item;
            bool bought = false;

            if (!Inventory.Owns(item))
            {
                if (!Wallet.SpendMoney(item.Price))
                {
                    Debug.LogWarning($"Not enough money to buy {item.name} ({item.Price})");
                    GameAudio.Play(SoundId.ShopDenied);
                    return;
                }

                Inventory.Add(item);
                Debug.Log($"ShopMenuController: bought {item.Id} for {item.Price} coins, {Wallet.PlayerMoney} left.");
                EventBus.Raise<ShowMoneyCountEvent>();
                bought = true;
                CheckArsenal();
            }

            GameAudio.Play(bought ? SoundId.ShopBuy : SoundId.ShopEquip);
            Equipment.Equip(item);
            Debug.Log($"ShopMenuController: equipped {item.Id} in {item.Category}.");
            RefreshStates();
            EventBus.Raise<EquippedItemChangedEvent>();
        }

        private void CheckArsenal()
        {
            int ownedGuns = 0;

            foreach (ShopItem gun in catalog.GetItems(ShopCategory.Guns))
            {
                if (Inventory.Owns(gun))
                {
                    ownedGuns++;
                }
            }

            if (ownedGuns >= ARSENAL_GUN_COUNT)
            {
                MobilePlatform.UnlockAchievement(Achievement.Arsenal);
            }
        }

        private void RefreshStates()
        {
            foreach (ShopItemView view in pool)
            {
                if (view.gameObject.activeSelf)
                {
                    view.SetState(Inventory.Owns(view.Item), Equipment.IsEquipped(view.Item));
                }
            }
        }

        private void OnDestroy()
        {
            foreach (ShopItemView view in pool)
            {
                if (view != null)
                {
                    view.Clicked -= OnItemClicked;
                }
            }
        }

        private void ResizeContent(int itemCount)
        {
            RectTransform content = scrollRect.content;
            RectOffset padding = grid.padding;
            Vector2 spacing = grid.spacing;
            Vector2 cellSize = grid.cellSize;

            int columns;

            if (grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                columns = grid.constraintCount;
            }
            else
            {
                float usableWidth = content.rect.width - padding.horizontal + spacing.x;
                columns = Mathf.FloorToInt(usableWidth / (cellSize.x + spacing.x));
            }

            columns = Mathf.Max(1, columns);
            int rows = Mathf.CeilToInt((float)itemCount / columns);
            float height = padding.vertical + rows * cellSize.y + Mathf.Max(0, rows - 1) * spacing.y;

            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}
