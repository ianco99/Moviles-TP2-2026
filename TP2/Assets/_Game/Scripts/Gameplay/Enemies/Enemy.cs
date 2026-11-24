using GunJump.Core;
using GunJump.Platform;
using ianco99.ToolBox.Services;
using UnityEngine;

namespace GunJump.Gameplay
{
    public abstract class Enemy : MonoBehaviour, IBulletTarget
    {
        [SerializeField] EnemyConfig config;

        LevelSession session;
        int health;

        bool isDead;
        public bool IsDead => isDead;

        protected EnemyConfig Config => config;

        MobilePlatform MobilePlatform => ServiceProvider.Instance.GetService<MobilePlatform>();

        protected virtual void Awake()
        {
            health = config.MaxHealth;
        }

        public void Bind(LevelSession levelSession)
        {
            session = levelSession;
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
            {
                GameAudio.Play(SoundId.EnemyHit);
                return;
            }

            isDead = true;
            GameAudio.Play(SoundId.EnemyDeath);
            MobilePlatform.Vibrate(HapticPulse.Kill);
            session?.RegisterKill(config.CoinReward);
            Debug.Log($"Enemy: {name} killed, +{config.CoinReward} coins.");
            OnDeath(hit);
        }

        protected virtual void OnHit(in HitInfo hit)
        {
        }

        protected abstract void OnDeath(in HitInfo hit);
    }
}
