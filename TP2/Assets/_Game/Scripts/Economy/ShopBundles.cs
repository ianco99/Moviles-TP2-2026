using System;

namespace _Game.Scripts.Shop
{
    public static class ShopBundles
    {
        public const string GUNS = "guns";
        public const string SKINS = "skins";
        public const string BULLETS = "bullets";
        public const string SHOT_VFX = "shotvfx";

        public static string For(ShopCategory category)
        {
            switch (category)
            {
                case ShopCategory.Guns:
                    return GUNS;
                case ShopCategory.GunSkins:
                    return SKINS;
                case ShopCategory.Bullets:
                    return BULLETS;
                case ShopCategory.ShotVFX:
                    return SHOT_VFX;
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
        }
    }
}
