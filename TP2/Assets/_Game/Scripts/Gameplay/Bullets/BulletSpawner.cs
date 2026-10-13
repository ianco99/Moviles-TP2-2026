using System;
using UnityEngine;
using UnityEngine.Pool;

namespace GunJump.Gameplay
{
    public sealed class BulletSpawner : MonoBehaviour
    {
        [SerializeField] Bullet prefab;
        [SerializeField] int defaultCapacity = 16;
        [SerializeField] int maxSize = 64;

        ObjectPool<Bullet> pool;
        Action<Bullet> release;

        void Awake()
        {
            release = Release;
            pool = new ObjectPool<Bullet>(
                Create,
                bullet => bullet.gameObject.SetActive(true),
                bullet => bullet.gameObject.SetActive(false),
                bullet => Destroy(bullet.gameObject),
                false,
                defaultCapacity,
                maxSize);
        }

        void OnDestroy()
        {
            pool.Dispose();
        }

        public void Spawn(Vector3 position, Vector3 direction)
        {
            pool.Get().Launch(position, direction);
        }

        Bullet Create()
        {
            Bullet bullet = Instantiate(prefab, transform);
            bullet.Init(release);
            return bullet;
        }

        void Release(Bullet bullet)
        {
            pool.Release(bullet);
        }
    }
}
