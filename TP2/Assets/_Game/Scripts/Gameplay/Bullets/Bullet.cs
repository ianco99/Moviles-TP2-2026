using System;
using UnityEngine;

namespace GunJump.Gameplay
{
    // Se mueve por raycast en vez de por física: a esta velocidad un collider atravesaría a los enemigos.
    public sealed class Bullet : MonoBehaviour
    {
        [SerializeField] BulletConfig config;

        Action<Bullet> release;
        Vector3 direction;
        float deathTime;

        public void Init(Action<Bullet> releaseToPool)
        {
            release = releaseToPool;
        }

        public void Launch(Vector3 position, Vector3 launchDirection)
        {
            direction = launchDirection.normalized;
            transform.SetPositionAndRotation(position, Quaternion.FromToRotation(Vector3.right, direction));
            deathTime = Time.time + config.Lifetime;
        }

        void Update()
        {
            float step = config.Speed * Time.deltaTime;

            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, step, config.HitMask, QueryTriggerInteraction.Ignore))
            {
                Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
                if (enemy != null)
                    enemy.TakeHit(new HitInfo(config.Damage, hit.point, direction));

                release(this);
                return;
            }

            transform.position += direction * step;

            if (Time.time >= deathTime)
                release(this);
        }
    }
}
