using UnityEngine;

namespace _Game.Scripts.Shop
{
    public enum ShopCategory
    {
        Guns,
        GunSkins,
        Bullets,
        ShotVFX
    }

    [CreateAssetMenu(menuName = "GunJump/Shop Item", fileName = "ShopItem")]
    public sealed class ShopItem : ScriptableObject
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private ShopCategory category;
        [SerializeField] private int price;

        public Sprite Sprite => sprite;
        public ShopCategory Category => category;
        public int Price => price;
    }
}
