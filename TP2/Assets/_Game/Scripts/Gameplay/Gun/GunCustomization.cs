using System;
using System.Collections;
using _Game.Scripts.Shop;
using GunJump.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GunJump.Gameplay
{
    public sealed class GunCustomization
    {
        readonly MonoBehaviour runner;
        readonly Transform gun;
        readonly Transform muzzle;
        readonly BulletSpawner bulletSpawner;
        readonly Renderer[] placeholder;

        Quaternion modelRotation;
        float modelLength;
        GameObject model;
        Renderer[] modelRenderers;
        GameObject shotEffectInstance;
        ParticleSystem shotEffect;
        int requestId;
        Action pendingDone;

        public GunCustomization(
            MonoBehaviour coroutineRunner,
            Transform gunTransform,
            Transform muzzleTransform,
            BulletSpawner spawner,
            Renderer[] placeholderRenderers,
            Vector3 modelEulerRotation,
            float fittedModelLength)
        {
            runner = coroutineRunner;
            gun = gunTransform;
            muzzle = muzzleTransform;
            bulletSpawner = spawner;
            placeholder = placeholderRenderers;
            SetModelFit(modelEulerRotation, fittedModelLength);
        }

        public void Apply(GunLoadout loadout, ContentLibrary content, Action onDone = null)
        {
            requestId++;
            FinishPending();

            if (TryGetLoaded(loadout, content, out Parts parts))
            {
                Show(parts);
                onDone?.Invoke();
                return;
            }

            pendingDone = onDone;
            runner.StartCoroutine(LoadRoutine(loadout, content, requestId));
        }

        public void SetModelFit(Vector3 eulerRotation, float length)
        {
            modelRotation = Quaternion.Euler(eulerRotation);
            modelLength = length;

            if (model != null)
                FitModel();
        }

        public void PlayShotEffect()
        {
            if (shotEffect == null)
                return;

            shotEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            shotEffect.Play(true);
        }

        public void Dispose()
        {
            requestId++;
            FinishPending();
            ClearModel();
            ClearShotEffect();
        }

        void FinishPending()
        {
            Action done = pendingDone;
            pendingDone = null;
            done?.Invoke();
        }

        struct Parts
        {
            public GameObject Model;
            public Material Skin;
            public GameObject Bullet;
            public GameObject ShotEffect;
        }

        static bool TryGetLoaded(GunLoadout loadout, ContentLibrary content, out Parts parts)
        {
            parts = default;
            return TryGetLoaded(loadout.Gun, content, out parts.Model)
                   && TryGetLoaded(loadout.Skin, content, out parts.Skin)
                   && TryGetLoaded(loadout.Bullet, content, out parts.Bullet)
                   && TryGetLoaded(loadout.ShotVfx, content, out parts.ShotEffect);
        }

        static bool TryGetLoaded<AssetType>(ShopItem item, ContentLibrary content, out AssetType asset) where AssetType : Object
        {
            asset = null;

            if (!IsCustom(item))
                return true;

            return content.Get(item.BundleName).TryGetLoaded(item.AssetName, out asset);
        }

        IEnumerator LoadRoutine(GunLoadout loadout, ContentLibrary content, int id)
        {
            Parts parts = new Parts();

            yield return Load<GameObject>(loadout.Gun, content, loaded => parts.Model = loaded);
            yield return Load<Material>(loadout.Skin, content, loaded => parts.Skin = loaded);
            yield return Load<GameObject>(loadout.Bullet, content, loaded => parts.Bullet = loaded);
            yield return Load<GameObject>(loadout.ShotVfx, content, loaded => parts.ShotEffect = loaded);

            if (id != requestId)
                yield break;

            Show(parts);
            FinishPending();
        }

        static IEnumerator Load<AssetType>(ShopItem item, ContentLibrary content, Action<AssetType> onLoaded) where AssetType : Object
        {
            if (!IsCustom(item))
                yield break;

            ContentBundle bundle = content.Get(item.BundleName);
            yield return bundle.LoadBundle();
            yield return bundle.LoadAsset(item.AssetName, onLoaded);
        }

        static bool IsCustom(ShopItem item)
        {
            return item != null && !string.IsNullOrEmpty(item.AssetName);
        }

        void Show(Parts parts)
        {
            ShowModel(parts.Model);
            ApplySkin(parts.Skin);
            bulletSpawner.SetModel(parts.Bullet);
            SetShotEffect(parts.ShotEffect);
        }

        void ShowModel(GameObject prefab)
        {
            ClearModel();
            SetPlaceholderVisible(prefab == null);

            if (prefab == null)
                return;

            model = Object.Instantiate(prefab, gun);
            RemoveColliders(model);
            modelRenderers = model.GetComponentsInChildren<Renderer>(true);
            FitModel();
        }

        void FitModel()
        {
            Transform modelTransform = model.transform;
            modelTransform.localPosition = Vector3.zero;
            modelTransform.localRotation = modelRotation;
            modelTransform.localScale = Vector3.one;

            GunMath.FitToLength(BoundsInGunSpace(modelRenderers), modelLength, out float scale, out Vector3 position);
            modelTransform.localScale = Vector3.one * scale;
            modelTransform.localPosition = position;
        }

        void ApplySkin(Material skin)
        {
            GunSkin.Apply(model != null ? modelRenderers : placeholder, skin);
        }

        void SetShotEffect(GameObject prefab)
        {
            ClearShotEffect();

            if (prefab == null)
                return;

            shotEffectInstance = Object.Instantiate(prefab, muzzle);
            shotEffect = shotEffectInstance.GetComponentInChildren<ParticleSystem>(true);

            if (shotEffect == null)
            {
                Debug.LogError($"El efecto de disparo '{prefab.name}' no tiene un ParticleSystem.", prefab);
                ClearShotEffect();
                return;
            }

            shotEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        Bounds BoundsInGunSpace(Renderer[] renderers)
        {
            Matrix4x4 toGun = gun.worldToLocalMatrix;
            Bounds result = default;
            bool hasPoints = false;

            foreach (Renderer target in renderers)
            {
                Matrix4x4 toGunFromRenderer = toGun * target.transform.localToWorldMatrix;
                Bounds local = target.localBounds;
                Vector3 min = local.min;
                Vector3 max = local.max;

                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 point = new Vector3(
                        (corner & 1) == 0 ? min.x : max.x,
                        (corner & 2) == 0 ? min.y : max.y,
                        (corner & 4) == 0 ? min.z : max.z);
                    point = toGunFromRenderer.MultiplyPoint3x4(point);

                    if (hasPoints)
                    {
                        result.Encapsulate(point);
                    }
                    else
                    {
                        result = new Bounds(point, Vector3.zero);
                        hasPoints = true;
                    }
                }
            }

            return result;
        }

        void SetPlaceholderVisible(bool visible)
        {
            foreach (Renderer part in placeholder)
            {
                if (part != null)
                    part.enabled = visible;
            }
        }

        static void RemoveColliders(GameObject instance)
        {
            foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
                Object.Destroy(collider);
        }

        void ClearModel()
        {
            if (model != null)
            {
                model.SetActive(false);
                Object.Destroy(model);
            }

            model = null;
            modelRenderers = null;
        }

        void ClearShotEffect()
        {
            if (shotEffectInstance != null)
            {
                shotEffectInstance.SetActive(false);
                Object.Destroy(shotEffectInstance);
            }

            shotEffectInstance = null;
            shotEffect = null;
        }
    }
}
