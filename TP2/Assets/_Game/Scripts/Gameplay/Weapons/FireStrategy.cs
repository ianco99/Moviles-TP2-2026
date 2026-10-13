using UnityEngine;

namespace GunJump.Gameplay
{
    // Unity no serializa interfaces: esta base permite asignar la estrategia como asset.
    public abstract class FireStrategy : ScriptableObject, IFireStrategy
    {
        public abstract void Fire(Gun gun, BulletSpawner spawner);
    }
}
