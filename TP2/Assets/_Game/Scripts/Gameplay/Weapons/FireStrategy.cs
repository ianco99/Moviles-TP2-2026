using UnityEngine;

namespace GunJump.Gameplay
{
    public abstract class FireStrategy : ScriptableObject, IFireStrategy
    {
        public abstract void Fire(Gun gun, BulletSpawner spawner);
    }
}
