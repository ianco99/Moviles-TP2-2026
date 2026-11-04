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

        [Tooltip("Solo armas: nombre del modelo dentro del AssetBundle 'guns' (el nombre del .fbx sin extensión, p. ej. blaster-a).")]
        [SerializeField] private string modelName;

        // Identificador estable para guardar: el nombre del asset.
        public string Id => name;
        public Sprite Sprite => sprite;
        public ShopCategory Category => category;
        public int Price => price;
        public string ModelName => modelName;
    }
}
