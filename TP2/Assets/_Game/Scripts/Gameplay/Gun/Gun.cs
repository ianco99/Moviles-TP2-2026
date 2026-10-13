using UnityEngine;

namespace GunJump.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Gun : MonoBehaviour
    {
        // El tope por defecto de Unity (7 rad/s) queda por debajo del giro del arma.
        const float MaxAngularVelocity = 60f;

        [SerializeField] GunTuning tuning;
        [Tooltip("El cañón apunta hacia el eje X local de este transform.")]
        [SerializeField] Transform muzzle;
        [SerializeField] FireStrategy fireStrategy;
        [SerializeField] BulletSpawner bulletSpawner;

        Rigidbody body;
        IFireStrategy strategy;
        float nextFireTime;
        float lastContactTime = float.NegativeInfinity;

        public Transform Muzzle => muzzle;
        bool IsGrounded => Time.time - lastContactTime <= tuning.GroundedGrace;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezePositionZ
                               | RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationY;
            body.maxAngularVelocity = MaxAngularVelocity;
            // El freno del giro lo maneja GunTuning.SpinDamping.
            body.angularDamping = 0f;
            // Un cuerpo dormido deja de recibir OnCollisionStay y el arma quieta no contaría como apoyada.
            body.sleepThreshold = 0f;
            strategy = fireStrategy;
        }

        void FixedUpdate()
        {
            Vector3 angular = body.angularVelocity;
            angular.z = GunMath.DampSpin(angular.z, tuning.SpinDamping * Mathf.Deg2Rad, Time.fixedDeltaTime);
            body.angularVelocity = angular;
        }

        void OnCollisionStay(Collision collision)
        {
            lastContactTime = Time.time;
        }

        public void SetStrategy(IFireStrategy value)
        {
            strategy = value;
        }

        public bool TryFire()
        {
            if (strategy == null || Time.time < nextFireTime)
                return false;

            nextFireTime = Time.time + tuning.FireInterval;
            bool wasGrounded = IsGrounded;
            strategy.Fire(this, bulletSpawner);

            if (wasGrounded)
            {
                body.linearVelocity = GunMath.EnsureLift(body.linearVelocity, tuning.GroundLift, tuning.MaxSpeed);
                lastContactTime = float.NegativeInfinity;
            }

            // El disparo fija el giro en vez de sumarlo: así un tiro con el arma dada vuelta
            // siempre la endereza, venga girando como venga.
            Vector3 angular = body.angularVelocity;
            float direction = GunMath.RecoilSpinDirection(muzzle.right.x, angular.z);
            angular.z = direction * tuning.RecoilSpin * Mathf.Deg2Rad;
            body.angularVelocity = angular;
            return true;
        }

        public void ApplyRecoil(float recoil)
        {
            body.linearVelocity = GunMath.ApplyRecoil(
                body.linearVelocity,
                muzzle.right,
                recoil,
                tuning.RecoilCarryOver,
                tuning.MaxSpeed);
        }
    }
}
