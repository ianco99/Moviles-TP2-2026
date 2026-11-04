using _Game.Scripts.Economy;
using _Game.Scripts.Shop;

namespace GunJump.Gameplay
{
    // Lo que el jugador tiene equipado para el arma, una pieza por categoría de la tienda.
    // Cualquiera puede ser null: esa parte queda como la trae el arma.
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
    }
}
