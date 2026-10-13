using UnityEngine;

namespace GunJump.Gameplay
{
    public abstract class Enemy : MonoBehaviour
    {
        [SerializeField] EnemyConfig config;

        int health;

        public bool IsDead { get; private set; }
        protected EnemyConfig Config => config;

        protected virtual void Awake()
        {
            health = config.MaxHealth;
        }

        // Template Method: la secuencia de recibir un impacto es fija;
        // cada enemigo redefine OnHit y OnDeath.
        public void TakeHit(in HitInfo hit)
        {
            if (IsDead)
                return;

            health -= hit.Damage;
            OnHit(hit);

            if (health > 0)
                return;

            IsDead = true;
            OnDeath(hit);
        }

        protected virtual void OnHit(in HitInfo hit)
        {
        }

        protected abstract void OnDeath(in HitInfo hit);
    }
}
