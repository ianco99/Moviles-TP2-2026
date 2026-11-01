using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Shop
{
    public class ShopMenuController : MonoBehaviour
    {
        private static readonly Vector2 ItemSize = new Vector2(200f, 200f);

        [SerializeField] private Image itemPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField] private int initialPoolSize = 20;

        [Header("Items")]
        [SerializeField] private List<ShopItem> guns;
        [SerializeField] private List<ShopItem> gunSkins;
        [SerializeField] private List<ShopItem> bullets;
        [SerializeField] private List<ShopItem> shotVfx;

        private readonly List<Image> pool = new List<Image>();
        private ShopCategory currentCategory = ShopCategory.Guns;

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
                    pool[i].sprite = items[i].Sprite;
                }

                pool[i].gameObject.SetActive(isUsed);
            }

            ResizeContent(items.Count);
        }

        private void AddToPool()
        {
            Image item = Instantiate(itemPrefab, scrollRect.content);
            item.gameObject.SetActive(false);
            pool.Add(item);
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
