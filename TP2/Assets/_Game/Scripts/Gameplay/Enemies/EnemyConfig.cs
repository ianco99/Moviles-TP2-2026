using UnityEngine;

namespace GunJump.Gameplay
{
    [CreateAssetMenu(menuName = "GunJump/Enemy Config", fileName = "EnemyConfig")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [SerializeField] int maxHealth = 1;
        [SerializeField] int coinReward = 10;
        [Tooltip("Velocidad con que sale despedido al morir.")]
        [SerializeField] float deathImpulse = 6f;
        [Tooltip("Segundos hasta que el cuerpo desaparece.")]
        [SerializeField] float despawnDelay = 2f;

        public int MaxHealth => maxHealth;
        public int CoinReward => coinReward;
        public float DeathImpulse => deathImpulse;
        public float DespawnDelay => despawnDelay;
    }
}
