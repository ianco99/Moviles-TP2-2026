using System.Collections;
using UnityEngine;

namespace GunJump.Gameplay
{
    public sealed class StaticEnemy : Enemy
    {
        [Tooltip("Opcional. Si está asignado, el cuerpo sale despedido al morir.")]
        [SerializeField] Rigidbody body;

        protected override void OnDeath(in HitInfo hit)
        {
            if (body != null)
            {
                body.isKinematic = false;
                body.AddForce((hit.Direction + Vector3.up) * Config.DeathImpulse, ForceMode.VelocityChange);
            }

            StartCoroutine(Despawn());
        }

        IEnumerator Despawn()
        {
            yield return new WaitForSeconds(Config.DespawnDelay);
            gameObject.SetActive(false);
        }
    }
}
