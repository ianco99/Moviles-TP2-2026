using UnityEngine;

namespace GunJump.Gameplay
{
    public readonly struct HitInfo
    {
        public readonly int Damage;
        public readonly Vector3 Point;
        public readonly Vector3 Direction;

        public HitInfo(int damage, Vector3 point, Vector3 direction)
        {
            Damage = damage;
            Point = point;
            Direction = direction;
        }
    }
}
