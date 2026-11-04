using _Game.Scripts.Economy;
using GunJump.Core;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
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

        GunCustomization customization;
        EventBus eventBus;
        EventBus.EventCallback<LevelSceneLoadedEvent> levelLoadedCallback;

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

            // Lo que el arma trae dibujado (los cubos del prototipo) se oculta cuando llega el modelo equipado.
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
        // Para ajustar el modelo desde el Inspector en Play y ver el resultado en el momento.
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

        void OnCollisionStay(Collision collision)
        {
            lastContactTime = Time.time;
        }

        public void SetStrategy(IFireStrategy value)
        {
            strategy = value;
        }

        // Se viste con lo equipado. Lo llama solo al terminar de cargarse el nivel.
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

        // Dando Play directo en una escena de nivel no pasó el Bootstrapper: no hay EventBus
        // y el arma queda como viene en la escena.
        void SubscribeToLevelLoad()
        {
            ServiceProvider services = ServiceProvider.Instance;
            if (!services.ContainsService<EventBus>())
                return;

            eventBus = services.GetService<EventBus>();
            levelLoadedCallback = OnLevelLoaded;
            eventBus.Subscribe(levelLoadedCallback);
        }

        // El nivel todavía está tapado por la pantalla de carga: la sostiene hasta que el arma está vestida.
        void OnLevelLoaded(in LevelSceneLoadedEvent e)
        {
            ServiceProvider services = ServiceProvider.Instance;
            if (!services.ContainsService<PlayerEquipment>()
                || !services.ContainsService<ContentLibrary>()
                || !services.ContainsService<LevelLoader>())
                return;

            LevelLoader loader = services.GetService<LevelLoader>();
            loader.Hold();
            ApplyLoadout(
                GunLoadout.Read(services.GetService<PlayerEquipment>()),
                services.GetService<ContentLibrary>(),
                loader.Release);
        }
    }
}
