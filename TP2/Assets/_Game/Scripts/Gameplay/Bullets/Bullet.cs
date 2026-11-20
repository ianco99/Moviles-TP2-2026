using System;
using GunJump.Core;
using UnityEngine;

namespace GunJump.Gameplay
{
    public sealed class Bullet : MonoBehaviour
    {
        [SerializeField] BulletConfig config;

        Action<Bullet> release;
        Vector3 direction;
        float deathTime;
        GameObject model;

        public void Init(Action<Bullet> releaseToPool)
        {
            release = releaseToPool;
        }

        public void SetModel(GameObject modelPrefab)
        {
            if (model != null)
            {
                model.SetActive(false);
                Destroy(model);
                model = null;
            }

            if (TryGetComponent(out Renderer ownRenderer))
                ownRenderer.enabled = modelPrefab == null;

            if (modelPrefab == null)
                return;

            model = Instantiate(modelPrefab, transform);
            model.transform.localScale = GunMath.UndoParentScale(modelPrefab.transform.localScale, transform.localScale);

            foreach (Collider collider in model.GetComponentsInChildren<Collider>(true))
                Destroy(collider);
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
                IBulletTarget target = hit.collider.GetComponentInParent<IBulletTarget>();
                if (target != null)
                    target.TakeHit(new HitInfo(config.Damage, hit.point, direction));
                else
                    GameAudio.Play(SoundId.BulletImpact);

                release(this);
                return;
            }

            transform.position += direction * step;

            if (Time.time >= deathTime)
                release(this);
        }
    }
}
