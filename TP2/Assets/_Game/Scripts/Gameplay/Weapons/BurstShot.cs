using System.Collections;
using UnityEngine;

namespace GunJump.Gameplay
{
    [CreateAssetMenu(menuName = "GunJump/Fire/Burst Shot", fileName = "BurstShot")]
    public sealed class BurstShot : FireStrategy
    {
        [Tooltip("Balas por toque, disparadas una detrás de otra.")]
        [Min(1)]
        [SerializeField] int shotCount = 3;
        [Tooltip("Segundos entre balas de la misma ráfaga. La ráfaga entera tiene que durar menos que el Fire Interval de GunTuning.")]
        [SerializeField] float shotInterval = 0.05f;
        [Tooltip("Velocidad, en unidades por segundo, que cada bala de la ráfaga le da al arma.")]
        [SerializeField] float recoilPerShot = 3.5f;

        public override void Fire(Gun gun, BulletSpawner spawner)
        {
            gun.StartCoroutine(Burst(gun, spawner));
        }

        IEnumerator Burst(Gun gun, BulletSpawner spawner)
        {
            WaitForSeconds wait = new WaitForSeconds(shotInterval);

            for (int i = 0; i < shotCount; i++)
            {
                if (i > 0)
                    yield return wait;

                spawner.Spawn(gun.Muzzle.position, gun.Muzzle.right);
                gun.ApplyRecoil(recoilPerShot);
            }
        }
    }
}
