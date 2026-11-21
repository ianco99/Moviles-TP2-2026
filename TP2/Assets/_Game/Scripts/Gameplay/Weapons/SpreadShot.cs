using UnityEngine;

namespace GunJump.Gameplay
{
    [CreateAssetMenu(menuName = "GunJump/Fire/Spread Shot", fileName = "SpreadShot")]
    public sealed class SpreadShot : FireStrategy
    {
        [Tooltip("Balas por disparo, repartidas en abanico.")]
        [Min(1)]
        [SerializeField] int bulletCount = 3;
        [Tooltip("Apertura total del abanico, en grados.")]
        [SerializeField] float spreadAngle = 30f;
        [Tooltip("Velocidad, en unidades por segundo, que el disparo le da al arma.")]
        [SerializeField] float recoil = 10f;

        public override void Fire(Gun gun, BulletSpawner spawner)
        {
            Transform muzzle = gun.Muzzle;
            float step = bulletCount > 1 ? spreadAngle / (bulletCount - 1) : 0f;
            float firstAngle = bulletCount > 1 ? -spreadAngle * 0.5f : 0f;

            for (int i = 0; i < bulletCount; i++)
            {
                Quaternion turn = Quaternion.AngleAxis(firstAngle + step * i, Vector3.forward);
                spawner.Spawn(muzzle.position, turn * muzzle.right);
            }

            gun.ApplyRecoil(recoil);
        }
    }
}
