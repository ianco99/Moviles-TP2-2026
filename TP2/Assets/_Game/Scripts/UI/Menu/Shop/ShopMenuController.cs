using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Shop
{
    public enum ShopCategory
    {
        Guns,
        GunSkins,
        Bullets,
        ShotVFX
    }

    public class ShopMenuController : MonoBehaviour
    {
        private static readonly Vector2 ItemSize = new Vector2(200f, 200f);

        [SerializeField] private Image itemPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField] private int initialPoolSize = 20;
        //TODO: Replace with item ScriptableObjects
        [SerializeField] private List<Sprite> placeholderSprites;

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
            ShowItems(GetSprites(currentCategory));
        }

        public void SelectCategory(int category)
        {
            currentCategory = (ShopCategory)category;
            ShowCategory();
        }

        //TODO: Filter by category once item ScriptableObjects exist
        private IReadOnlyList<Sprite> GetSprites(ShopCategory category)
        {
            return placeholderSprites;
        }

        private void ShowItems(IReadOnlyList<Sprite> sprites)
        {
            while (pool.Count < sprites.Count)
            {
                AddToPool();
            }

            for (int i = 0; i < pool.Count; i++)
            {
                bool isUsed = i < sprites.Count;

                if (isUsed)
                {
                    pool[i].sprite = sprites[i];
                }

                pool[i].gameObject.SetActive(isUsed);
            }

            ResizeContent(sprites.Count);
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
