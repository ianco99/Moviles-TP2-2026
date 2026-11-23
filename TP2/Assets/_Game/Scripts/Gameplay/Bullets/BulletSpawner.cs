using System;
using System.Collections;
using System.Collections.Generic;
using GunJump.Core;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.Pool;

namespace GunJump.Gameplay
{
    public sealed class BulletSpawner : MonoBehaviour
    {
        [Tooltip("Nombre del prefab de la bala en el AssetBundle 'prefabs' (carpeta Prefabs/Runtime). Se carga al cargar el nivel.")]
        [SerializeField] string bulletAssetName = "Bullet";
        [SerializeField] int defaultCapacity = 16;
        [SerializeField] int maxSize = 64;

        readonly List<Bullet> bullets = new List<Bullet>();
        ObjectPool<Bullet> pool;
        Action<Bullet> release;
        GameObject model;
        Bullet prefab;
        EventBus eventBus;
        EventBus.EventCallback<LevelSceneLoadedEvent> levelLoadedCallback;

        void Awake()
        {
            release = Release;
            pool = new ObjectPool<Bullet>(
                Create,
                bullet => bullet.gameObject.SetActive(true),
                bullet => bullet.gameObject.SetActive(false),
                DestroyBullet,
                false,
                defaultCapacity,
                maxSize);

            SubscribeToLevelLoad();
        }

        void OnDestroy()
        {
            if (eventBus != null)
                eventBus.UnSubscribe(levelLoadedCallback);

            pool.Dispose();
        }

        public void Spawn(Vector3 position, Vector3 direction)
        {
            if (prefab == null)
                return;

            pool.Get().Launch(position, direction);
        }

        public void SetModel(GameObject modelPrefab)
        {
            model = modelPrefab;

            foreach (Bullet bullet in bullets)
                bullet.SetModel(modelPrefab);
        }

        Bullet Create()
        {
            Bullet bullet = Instantiate(prefab, transform);
            bullet.Init(release);

            if (model != null)
                bullet.SetModel(model);

            bullets.Add(bullet);
            return bullet;
        }

        void Release(Bullet bullet)
        {
            pool.Release(bullet);
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

        void OnLevelLoaded(in LevelSceneLoadedEvent levelSceneLoadedEvent)
        {
            ServiceProvider services = ServiceProvider.Instance;
            if (prefab != null || !services.ContainsService<ContentLibrary>() || !services.ContainsService<LevelLoader>())
                return;

            LevelLoader loader = services.GetService<LevelLoader>();
            loader.Hold();
            StartCoroutine(LoadPrefab(services.GetService<ContentLibrary>(), loader));
        }

        IEnumerator LoadPrefab(ContentLibrary content, LevelLoader loader)
        {
            ContentBundle bundle = content.Get(ContentLibrary.PREFABS_BUNDLE);
            yield return bundle.LoadBundle();
            yield return bundle.LoadAsset<GameObject>(bulletAssetName, loaded => prefab = loaded != null ? loaded.GetComponent<Bullet>() : null);

            if (prefab == null)
                Debug.LogError($"BulletSpawner: el AssetBundle '{ContentLibrary.PREFABS_BUNDLE}' no tiene un prefab '{bulletAssetName}' con Bullet; el arma no va a disparar.", this);

            loader.Release();
        }

        void DestroyBullet(Bullet bullet)
        {
            bullets.Remove(bullet);
            Destroy(bullet.gameObject);
        }
    }
}
