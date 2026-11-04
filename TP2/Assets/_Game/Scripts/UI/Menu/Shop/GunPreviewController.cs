using System.Collections;
using _Game.Scripts.Economy;
using GunJump.Core;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Shop
{
    // Muestra el arma equipada, con su skin, en el menú principal y en la tienda, dando vueltas sobre su eje X (de punta,
    // hacia la cámara) mientras flota subiendo y bajando. Una cámara propia la renderiza a una
    // RenderTexture que se ve en un RawImage. Se prende y se apaga con ShowGunPreviewEvent y
    // HideGunPreviewEvent (los levanta MainMenuController); en la selección de nivel está apagada.
    // Tiene que estar fuera de los paneles del menú para seguir escuchando eventos aunque se desactiven.
    public class GunPreviewController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RawImage target;
        [SerializeField] private Camera previewCamera;
        [Tooltip("Pivote que gira sobre su eje X y flota; el modelo se instancia como hijo, centrado en él.")]
        [SerializeField] private Transform spinner;

        [Header("Presentation")]
        [Tooltip("Grados por segundo alrededor del eje X del pivote. Negativo invierte el sentido.")]
        [SerializeField] private float spinSpeed = 180f;
        [Tooltip("Grados que se gira el eje de giro alrededor del vertical. 0 = vuelta pura hacia la cámara; 90 = vuelta en el plano de la pantalla, como en el juego.")]
        [SerializeField] private float spinAxisYaw = 30f;
        [Tooltip("Unidades que el arma sube y baja mientras flota.")]
        [SerializeField] private float floatHeight = 0.05f;
        [Tooltip("Segundos de cada ciclo de subida y bajada.")]
        [SerializeField] private float floatPeriod = 2.5f;
        [Tooltip("Diámetro en unidades del círculo que barre el arma al girar, para que todas ocupen lo mismo.")]
        [SerializeField] private float spinSize = 0.9f;
        [Tooltip("Rotación local del modelo dentro del pivote, por si los modelos no miran hacia donde queremos.")]
        [SerializeField] private Vector3 modelRotation;
        [SerializeField] private int antiAliasing = 4;

        private ContentBundle bundle;
        private ContentBundle skinBundle;

        private EventBus eventBus;
        private PlayerEquipment equipment;
        private EventBus.EventCallback<ShowGunPreviewEvent> showCallback;
        private EventBus.EventCallback<HideGunPreviewEvent> hideCallback;
        private EventBus.EventCallback<EquippedItemChangedEvent> changedCallback;

        private RenderTexture renderTexture;
        private GameObject instance;
        private string shownModel;
        private string shownSkin;
        private int requestId;
        private bool isOpen;

        private Vector3 restPosition;
        private Quaternion restRotation;
        private float spinAngle;
        private float floatTime;

        private EventBus EventBus => eventBus ??= ServiceProvider.Instance.GetService<EventBus>();
        private PlayerEquipment Equipment => equipment ??= ServiceProvider.Instance.GetService<PlayerEquipment>();

        // Los mismos bundles que usa el arma en los niveles: Unity no deja abrir dos veces el mismo.
        private ContentBundle Bundle => bundle ??= ServiceProvider.Instance.GetService<ContentLibrary>().Get(ShopBundles.Guns);
        private ContentBundle SkinBundle => skinBundle ??= ServiceProvider.Instance.GetService<ContentLibrary>().Get(ShopBundles.Skins);

        private void Awake()
        {
            showCallback = OnShowRequested;
            hideCallback = OnHideRequested;
            changedCallback = OnEquippedChanged;

            EventBus.Subscribe(showCallback);
            EventBus.Subscribe(hideCallback);
            EventBus.Subscribe(changedCallback);

            restPosition = spinner.localPosition;
            restRotation = spinner.localRotation;

            previewCamera.enabled = false;
            target.enabled = false;
        }

        private void OnDestroy()
        {
            Close();

            if (eventBus == null)
            {
                return;
            }

            eventBus.UnSubscribe(showCallback);
            eventBus.UnSubscribe(hideCallback);
            eventBus.UnSubscribe(changedCallback);
        }

        private void Update()
        {
            if (instance == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            spinAngle = Mathf.Repeat(spinAngle + spinSpeed * deltaTime, 360f);

            float bob = 0f;

            if (floatPeriod > 0f)
            {
                floatTime = Mathf.Repeat(floatTime + deltaTime, floatPeriod);
                bob = Mathf.Sin(floatTime / floatPeriod * 2f * Mathf.PI) * floatHeight;
            }

            // Primero gira sobre su propio eje X y después el yaw inclina ese eje respecto de la cámara.
            spinner.localPosition = restPosition + Vector3.up * bob;
            spinner.localRotation = restRotation
                                    * Quaternion.Euler(0f, spinAxisYaw, 0f)
                                    * Quaternion.AngleAxis(spinAngle, Vector3.right);
        }

        private void ResetPose()
        {
            spinAngle = 0f;
            floatTime = 0f;
            spinner.localPosition = restPosition;
            spinner.localRotation = restRotation;
        }

        private void OnShowRequested(in ShowGunPreviewEvent e)
        {
            if (isOpen)
            {
                return;
            }

            isOpen = true;
            ResetPose();
            CreateRenderTexture();
            previewCamera.enabled = true;
            StartCoroutine(OpenRoutine());
        }

        private void OnHideRequested(in HideGunPreviewEvent e)
        {
            Close();
        }

        private void OnEquippedChanged(in EquippedItemChangedEvent e)
        {
            if (isOpen && Bundle.IsLoaded)
            {
                ShowEquipped();
            }
        }

        private IEnumerator OpenRoutine()
        {
            yield return Bundle.LoadBundle();

            if (isOpen && Bundle.IsLoaded)
            {
                ShowEquipped();
            }
        }

        private void ShowEquipped()
        {
            ShopItem gun = Equipment.GetEquipped(ShopCategory.Guns);
            string modelName = gun != null ? gun.AssetName : null;

            if (string.IsNullOrEmpty(modelName))
            {
                Debug.LogWarning($"El arma equipada ({(gun != null ? gun.name : "ninguna")}) no tiene Asset Name; no hay nada que mostrar.");
                ClearInstance();
                return;
            }

            // Sin skin equipada o sin Asset Name, el arma se muestra con sus propios materiales.
            ShopItem skin = Equipment.GetEquipped(ShopCategory.GunSkins);
            string skinName = skin != null && !string.IsNullOrEmpty(skin.AssetName) ? skin.AssetName : null;

            if (modelName == shownModel && skinName == shownSkin)
            {
                return;
            }

            requestId++;
            StartCoroutine(SwapRoutine(modelName, skinName, requestId));
        }

        // Ante cualquier cambio se vuelve a instanciar el modelo: así, sacar la skin le devuelve sus materiales.
        // El pivote sigue girando con su pose, así que el cambio no se nota como un salto.
        private IEnumerator SwapRoutine(string modelName, string skinName, int id)
        {
            GameObject prefab = null;
            yield return Bundle.LoadAsset<GameObject>(modelName, loaded => prefab = loaded);

            Material skin = null;

            if (skinName != null)
            {
                yield return SkinBundle.LoadBundle();
                yield return SkinBundle.LoadAsset<Material>(skinName, loaded => skin = loaded);
            }

            // Si mientras cargaba se pidió otra arma u otra skin, o se ocultó la vista previa, este resultado ya no sirve.
            if (id != requestId || !isOpen)
            {
                yield break;
            }

            ClearInstance();

            if (prefab == null)
            {
                yield break;
            }

            instance = Instantiate(prefab, spinner);
            shownModel = modelName;
            shownSkin = skinName;

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            GunSkin.Apply(renderers, skin);
            FitToSpinner(instance.transform, renderers);
            target.enabled = true;
        }

        // Centra el modelo en el pivote y lo escala para que el círculo que barre al girar sobre X mida spinSize.
        private void FitToSpinner(Transform model, Renderer[] renderers)
        {
            Quaternion spin = spinner.rotation;
            spinner.rotation = Quaternion.identity;

            model.localPosition = Vector3.zero;
            model.localRotation = Quaternion.Euler(modelRotation);
            model.localScale = Vector3.one;

            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;

                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }

                // Al girar sobre X, el punto más lejano del eje está como mucho en la diagonal de Y y Z.
                // El largo sobre X cuenta por si modelRotation deja el lado largo sobre el eje.
                Vector3 size = bounds.size;
                float sweep = Mathf.Sqrt(size.y * size.y + size.z * size.z);
                float span = Mathf.Max(size.x, sweep);
                float scale = span > 0f ? spinSize / span : 1f;
                Vector3 localCenter = spinner.InverseTransformPoint(bounds.center);

                model.localScale = Vector3.one * scale;
                model.localPosition = -localCenter * scale;
            }

            spinner.rotation = spin;
        }

        private void CreateRenderTexture()
        {
            Rect rect = target.rectTransform.rect;
            float canvasScale = target.canvas != null ? target.canvas.scaleFactor : 1f;
            int width = Mathf.Max(64, Mathf.RoundToInt(rect.width * canvasScale));
            int height = Mathf.Max(64, Mathf.RoundToInt(rect.height * canvasScale));

            renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                name = "GunPreview",
                antiAliasing = Mathf.Max(1, antiAliasing)
            };

            previewCamera.targetTexture = renderTexture;
            target.texture = renderTexture;
        }

        private void ClearInstance()
        {
            if (instance != null)
            {
                // Destroy recién actúa al final del frame; la apagamos ya porque el bundle se puede descargar antes.
                instance.SetActive(false);
                Destroy(instance);
                instance = null;
            }

            shownModel = null;
            shownSkin = null;

            if (target != null)
            {
                target.enabled = false;
            }
        }

        private void Close()
        {
            if (!isOpen)
            {
                return;
            }

            isOpen = false;
            requestId++;
            StopAllCoroutines();

            // Las instancias se destruyen antes de descargar los bundles de los que salieron. Se oculta al pasar a la
            // selección de nivel, así que el menú ya no los usa; el arma los vuelve a cargar una sola vez al entrar al nivel.
            ClearInstance();
            bundle?.Unload();
            skinBundle?.Unload();

            // Al descargar la escena, la cámara o el RawImage pueden haberse destruido antes que este componente.
            if (previewCamera != null)
            {
                previewCamera.enabled = false;
                previewCamera.targetTexture = null;
            }

            if (target != null)
            {
                target.texture = null;
            }

            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
                renderTexture = null;
            }
        }
    }
}
