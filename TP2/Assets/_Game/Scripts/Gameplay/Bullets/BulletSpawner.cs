using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace GunJump.Gameplay
{
    public sealed class BulletSpawner : MonoBehaviour
    {
        [SerializeField] Bullet prefab;
        [SerializeField] int defaultCapacity = 16;
        [SerializeField] int maxSize = 64;

        // Todas las balas creadas, estén en uso o en el pool, para poder cambiarles el modelo.
        readonly List<Bullet> bullets = new List<Bullet>();
        ObjectPool<Bullet> pool;
        Action<Bullet> release;
        GameObject model;

        void Awake()
        {
            release = Release;
            pool = new ObjectPool<Bullet>(
                Create,
                bullet => bullet.gameObject.SetActive(true),
                bullet => bullet.gameObject.SetActive(false),
                DestroyBullet,
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

        // Modelo de la bala equipada; null vuelve al dibujo del prefab.
        public void SetModel(GameObject modelPrefab)
        {
            model = modelPrefab;

            foreach (Bullet bullet in bullets)
                bullet.SetModel(modelPrefab);
        }

        Bullet Create()
        {
            Bullet bullet = Instantiate(prefab, transform);
            bullet.Init(release);

            if (model != null)
                bullet.SetModel(model);

            bullets.Add(bullet);
            return bullet;
        }

        void Release(Bullet bullet)
        {
            pool.Release(bullet);
        }

        void DestroyBullet(Bullet bullet)
        {
            bullets.Remove(bullet);
            Destroy(bullet.gameObject);
        }
    }
}
