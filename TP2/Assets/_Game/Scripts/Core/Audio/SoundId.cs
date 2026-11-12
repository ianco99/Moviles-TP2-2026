namespace GunJump.Core
{
    // Cada sonido del juego. AudioLibrary guarda el número de cada valor:
    // los nuevos van al final y los existentes no se reordenan.
    public enum SoundId
    {
        ButtonClick = 0,
        ShopTab = 1,
        ShopBuy = 2,
        ShopEquip = 3,
        ShopDenied = 4,
        Shot = 5,
        DryFire = 6,
        GunImpact = 7,
        BulletImpact = 8,
        EnemyHit = 9,
        EnemyDeath = 10,
        MultiplierHit = 11,
        CoinPickup = 12,
        LevelWin = 13,
        LevelLose = 14
    }
}
