using _Game.Scripts.Economy;
using _Game.Scripts.Shop;

namespace GunJump.Gameplay
{
    public readonly struct GunLoadout
    {
        public readonly ShopItem Gun;
        public readonly ShopItem Skin;
        public readonly ShopItem Bullet;
        public readonly ShopItem ShotVfx;

        public GunLoadout(ShopItem gun, ShopItem skin, ShopItem bullet, ShopItem shotVfx)
        {
            Gun = gun;
            Skin = skin;
            Bullet = bullet;
            ShotVfx = shotVfx;
        }

        public static GunLoadout Read(PlayerEquipment equipment)
        {
            return new GunLoadout(
                equipment.GetEquipped(ShopCategory.Guns),
                equipment.GetEquipped(ShopCategory.GunSkins),
                equipment.GetEquipped(ShopCategory.Bullets),
                equipment.GetEquipped(ShopCategory.ShotVFX));
        }

        public override string ToString()
        {
            return $"gun {IDOf(Gun)}, skin {IDOf(Skin)}, bullet {IDOf(Bullet)}, shot VFX {IDOf(ShotVfx)}";
        }

        private static string IDOf(ShopItem item)
        {
            return item != null ? item.Id : "none";
        }
    }
}
