using System.Collections.Generic;
using _Game.Scripts.Economy;
using GunJump.UI;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Shop
{
    public class ShopMenuController : MonoBehaviour
    {
        [SerializeField] private ShopItemView itemPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField] private int initialPoolSize = 20;

        [Header("Items")]
        [SerializeField] private ShopCatalog catalog;

        private readonly List<ShopItemView> pool = new List<ShopItemView>();
        private ShopCategory currentCategory = ShopCategory.Guns;
        private PlayerWalletManager wallet;
        private PlayerInventoryManager inventory;
        private PlayerEquipment equipment;
        private EventBus eventBus;

        private PlayerWalletManager Wallet => wallet ??= ServiceProvider.Instance.GetService<PlayerWalletManager>();
        private PlayerInventoryManager Inventory => inventory ??= ServiceProvider.Instance.GetService<PlayerInventoryManager>();
        private PlayerEquipment Equipment => equipment ??= ServiceProvider.Instance.GetService<PlayerEquipment>();
        private EventBus EventBus => eventBus ??= ServiceProvider.Instance.GetService<EventBus>();

        public void Setup()
        {
            if (pool.Count > 0)
            {
                return;
            }

            for (int i = 0; i < initialPoolSize; i++)
            {
                AddToPool();
            }
        }

        public void WakeUp()
        {
            ShowCategory();
        }

        public void ShowCategory()
        {
            ShowItems(catalog.GetItems(currentCategory));
        }

        public void SelectCategory(int category)
        {
            currentCategory = (ShopCategory)category;
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

            if (!Inventory.Owns(item))
            {
                if (!Wallet.SpendMoney(item.Price))
                {
                    Debug.LogWarning($"Not enough money to buy {item.name} ({item.Price})");
                    return;
                }

                Inventory.Add(item);
                EventBus.Raise<ShowMoneyCountEvent>();
            }

            Equipment.Equip(item);
            RefreshStates();
            EventBus.Raise<EquippedItemChangedEvent>();
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
