using UnityEngine;
using UnityEngine.Serialization;

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

        [Tooltip("Nombre del asset dentro del AssetBundle de su categoría, sin extensión: el modelo del arma (p. ej. blaster-a), " +
                 "el material de la skin, o el prefab de la bala o del efecto de disparo. Vacío = se usa lo que el arma trae por defecto.")]
        [FormerlySerializedAs("modelName")]
        [SerializeField] private string assetName;

        // Identificador estable para guardar: el nombre del asset.
        public string Id => name;
        public Sprite Sprite => sprite;
        public ShopCategory Category => category;
        public int Price => price;
        public string AssetName => assetName;
        public string BundleName => ShopBundles.For(category);
    }
}
