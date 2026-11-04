using System;

namespace _Game.Scripts.Shop
{
    // Cada categoría de la tienda tiene su propio AssetBundle; ShopItem.AssetName es el nombre del asset dentro de él.
    // GunBundleBuilder arma los bundles con estos nombres y el juego los abre con ContentBundle.
    public static class ShopBundles
    {
        public const string Guns = "guns";
        public const string Skins = "skins";
        public const string Bullets = "bullets";
        public const string ShotVfx = "shotvfx";

        public static string For(ShopCategory category)
        {
            switch (category)
            {
                case ShopCategory.Guns:
                    return Guns;
                case ShopCategory.GunSkins:
                    return Skins;
                case ShopCategory.Bullets:
                    return Bullets;
                case ShopCategory.ShotVFX:
                    return ShotVfx;
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
        }
    }
}
