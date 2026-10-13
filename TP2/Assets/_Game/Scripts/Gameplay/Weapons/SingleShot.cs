using UnityEngine;

namespace GunJump.Gameplay
{
    // Pistola: una bala, un retroceso.
    [CreateAssetMenu(menuName = "GunJump/Fire/Single Shot", fileName = "SingleShot")]
    public sealed class SingleShot : FireStrategy
    {
        [Tooltip("Velocidad, en unidades por segundo, que el disparo le da al arma.")]
        [SerializeField] float recoil = 7f;

        public override void Fire(Gun gun, BulletSpawner spawner)
        {
            spawner.Spawn(gun.Muzzle.position, gun.Muzzle.right);
            gun.ApplyRecoil(recoil);
        }
    }
}
