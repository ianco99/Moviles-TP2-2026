using UnityEngine;

namespace GunJump.Gameplay
{
    [CreateAssetMenu(menuName = "GunJump/Bullet Config", fileName = "BulletConfig")]
    public sealed class BulletConfig : ScriptableObject
    {
        [SerializeField] float speed = 40f;
        [SerializeField] float lifetime = 1.5f;
        [SerializeField] int damage = 1;
        [SerializeField] LayerMask hitMask = ~0;

        public float Speed => speed;
        public float Lifetime => lifetime;
        public int Damage => damage;
        public LayerMask HitMask => hitMask;
    }
}
