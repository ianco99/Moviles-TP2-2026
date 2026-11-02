using System;
using System.Collections.Generic;
using _Game.Scripts.Economy;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Shop
{
    public class ShopMenuController : MonoBehaviour
    {
        private static readonly Vector2 ItemSize = new Vector2(200f, 200f);

        [SerializeField] private ShopItemView itemPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField] private int initialPoolSize = 20;

        [Header("Items")]
        [SerializeField] private List<ShopItem> guns;
        [SerializeField] private List<ShopItem> gunSkins;
        [SerializeField] private List<ShopItem> bullets;
        [SerializeField] private List<ShopItem> shotVfx;

        private readonly List<ShopItemView> pool = new List<ShopItemView>();
        private ShopCategory currentCategory = ShopCategory.Guns;
        private PlayerWalletManager wallet;
        private PlayerInventoryManager inventory;

        private PlayerWalletManager Wallet => wallet ??= ServiceProvider.Instance.GetService<PlayerWalletManager>();
        private PlayerInventoryManager Inventory => inventory ??= ServiceProvider.Instance.GetService<PlayerInventoryManager>();

        public void Setup()
        {
            if (pool.Count > 0)
            {
                return;
            }

            grid.cellSize = ItemSize;

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
            ShowItems(GetItems(currentCategory));
        }

        public void SelectCategory(int category)
        {
            currentCategory = (ShopCategory)category;
            ShowCategory();
        }

        private IReadOnlyList<ShopItem> GetItems(ShopCategory category)
        {
            switch (category)
            {
                case ShopCategory.Guns:
                    return guns;
                case ShopCategory.GunSkins:
                    return gunSkins;
                case ShopCategory.Bullets:
                    return bullets;
                case ShopCategory.ShotVFX:
                    return shotVfx;
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
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
            }

            Inventory.Equip(item);
            RefreshStates();
        }

        private void RefreshStates()
        {
            foreach (ShopItemView view in pool)
            {
                if (view.gameObject.activeSelf)
                {
                    view.SetState(Inventory.Owns(view.Item), Inventory.IsEquipped(view.Item));
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

            float usableWidth = content.rect.width - padding.horizontal + spacing.x;
            int columns = Mathf.Max(1, Mathf.FloorToInt(usableWidth / (ItemSize.x + spacing.x)));
            int rows = Mathf.CeilToInt((float)itemCount / columns);
            float height = padding.vertical + rows * ItemSize.y + Mathf.Max(0, rows - 1) * spacing.y;

            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}
