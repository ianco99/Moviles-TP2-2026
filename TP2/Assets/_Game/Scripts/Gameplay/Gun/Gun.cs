using _Game.Scripts.Economy;
using GunJump.Core;
using GunJump.Platform;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using UnityEngine;

namespace GunJump.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Gun : MonoBehaviour
    {
        const float MAX_ANGULAR_VELOCITY = 60f;

        [SerializeField] GunTuning tuning;
        [Tooltip("El cañón apunta hacia el eje X local de este transform.")]
        [SerializeField] Transform muzzle;
        [SerializeField] FireStrategy fireStrategy;
        [Tooltip("Opcional. Elige la estrategia según el arma equipada en la tienda al cargar el nivel. Vacío = siempre Fire Strategy.")]
        [SerializeField] WeaponCatalog weapons;
        [SerializeField] BulletSpawner bulletSpawner;

        [Header("Equipamiento")]
        [Tooltip("Rotación local del modelo equipado para que su cañón mire hacia +X, como la boca.")]
        [SerializeField] Vector3 modelRotation = new Vector3(0f, 90f, 0f);
        [Tooltip("Largo, en unidades, que ocupa el modelo equipado sobre X. Se escala a ese largo y se centra en el arma: " +
                 "el collider y la boca del cañón no dependen del modelo.")]
        [SerializeField] float modelLength = 1f;

        Rigidbody body;
        IFireStrategy strategy;
        float nextFireTime;
        float lastContactTime = float.NegativeInfinity;
        float nextImpactSoundTime;

        GunCustomization customization;
        EventBus eventBus;
        EventBus.EventCallback<LevelSceneLoadedEvent> levelLoadedCallback;

        MobilePlatform MobilePlatform => ServiceProvider.Instance.GetService<MobilePlatform>();

        public Transform Muzzle => muzzle;
        bool IsGrounded => Time.time - lastContactTime <= tuning.GroundedGrace;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezePositionZ
                               | RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationY;
            body.maxAngularVelocity = MAX_ANGULAR_VELOCITY;
            body.angularDamping = 0f;
            body.sleepThreshold = 0f;
            strategy = fireStrategy;

            Renderer[] placeholder = GetComponentsInChildren<Renderer>(true);
            customization = new GunCustomization(this, transform, muzzle, bulletSpawner, placeholder, modelRotation, modelLength);
            SubscribeToLevelLoad();
        }

        void OnDestroy()
        {
            if (eventBus != null)
                eventBus.UnSubscribe(levelLoadedCallback);

            customization.Dispose();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            customization?.SetModelFit(modelRotation, modelLength);
        }
#endif

        void FixedUpdate()
        {
            Vector3 angular = body.angularVelocity;
            angular.z = GunMath.DampSpin(angular.z, tuning.SpinDamping * Mathf.Deg2Rad, Time.fixedDeltaTime);
            body.angularVelocity = angular;
        }

        void OnCollisionEnter(Collision collision)
        {
            float speed = collision.relativeVelocity.magnitude;
            if (speed < tuning.ImpactSoundMinSpeed || Time.time < nextImpactSoundTime)
                return;

            nextImpactSoundTime = Time.time + tuning.ImpactSoundInterval;
            float strength = Mathf.InverseLerp(tuning.ImpactSoundMinSpeed, tuning.ImpactSoundMaxSpeed, speed);
            GameAudio.Play(SoundId.GunImpact, Mathf.Lerp(tuning.ImpactSoundMinVolume, 1f, strength));
        }

        void OnCollisionStay(Collision collision)
        {
            lastContactTime = Time.time;
        }

        public void SetStrategy(IFireStrategy value)
        {
            strategy = value;
        }

        public void ApplyLoadout(GunLoadout loadout, ContentLibrary content, System.Action onDone = null)
        {
            customization.Apply(loadout, content, onDone);
        }

        public bool TryFire()
        {
            if (strategy == null || Time.time < nextFireTime)
                return false;

            nextFireTime = Time.time + tuning.FireInterval;
            bool wasGrounded = IsGrounded;
            strategy.Fire(this, bulletSpawner);
            customization.PlayShotEffect();
            GameAudio.Play(SoundId.Shot);
            MobilePlatform.Vibrate(HapticPulse.Shot);

            if (wasGrounded)
            {
                body.linearVelocity = GunMath.EnsureLift(body.linearVelocity, tuning.GroundLift, tuning.MaxSpeed);
                lastContactTime = float.NegativeInfinity;
            }

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

        void SubscribeToLevelLoad()
        {
            ServiceProvider services = ServiceProvider.Instance;
            if (!services.ContainsService<EventBus>())
                return;

            eventBus = services.GetService<EventBus>();
            levelLoadedCallback = OnLevelLoaded;
            eventBus.Subscribe(levelLoadedCallback);
        }

        void OnLevelLoaded(in LevelSceneLoadedEvent e)
        {
            ServiceProvider services = ServiceProvider.Instance;
            if (!services.ContainsService<PlayerEquipment>()
                || !services.ContainsService<ContentLibrary>()
                || !services.ContainsService<LevelLoader>())
                return;

            GunLoadout loadout = GunLoadout.Read(services.GetService<PlayerEquipment>());
            FireStrategy equippedStrategy = weapons != null ? weapons.StrategyFor(loadout.Gun) : null;
            if (equippedStrategy != null)
                SetStrategy(equippedStrategy);

            Debug.Log($"Gun: {loadout}, fires with {strategy}.");

            LevelLoader loader = services.GetService<LevelLoader>();
            loader.Hold();
            ApplyLoadout(loadout, services.GetService<ContentLibrary>(), loader.Release);
        }
    }
}
