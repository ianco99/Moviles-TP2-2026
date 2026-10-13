namespace GunJump.Gameplay
{
    // Strategy: cada arma dispara distinto y Gun no sabe cuál tiene puesta.
    public interface IFireStrategy
    {
        void Fire(Gun gun, BulletSpawner spawner);
    }
}
