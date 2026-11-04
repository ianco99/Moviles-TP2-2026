using System.Collections;
using _Game.Scripts.Economy;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Shop
{
    // Muestra el arma equipada girando en el centro de la tienda. Una cámara propia la renderiza a una
    // RenderTexture que se ve en un RawImage. Tiene que estar fuera del panel de la tienda para seguir
    // escuchando eventos aunque el panel se desactive.
    public class GunPreviewController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RawImage target;
        [SerializeField] private Camera previewCamera;
        [Tooltip("Pivote que gira; el modelo se instancia como hijo, centrado en él.")]
        [SerializeField] private Transform spinner;

        [Header("Presentation")]
        [Tooltip("Grados por segundo alrededor del eje vertical.")]
        [SerializeField] private float rotationSpeed = 60f;
        [Tooltip("Tamaño en unidades del lado más largo del modelo, para que todas las armas ocupen lo mismo.")]
        [SerializeField] private float fitSize = 1f;
        [Tooltip("Rotación local del modelo dentro del pivote, por si los modelos no miran hacia donde queremos.")]
        [SerializeField] private Vector3 modelRotation;
        [SerializeField] private int antiAliasing = 4;

        private readonly GunModelBundle bundle = new GunModelBundle();

        private EventBus eventBus;
        private PlayerEquipment equipment;
        private EventBus.EventCallback<ShopOpenedEvent> openedCallback;
        private EventBus.EventCallback<ShopClosedEvent> closedCallback;
        private EventBus.EventCallback<EquippedItemChangedEvent> changedCallback;

        private RenderTexture renderTexture;
        private GameObject instance;
        private string shownModel;
        private int requestId;
        private bool isOpen;

        private EventBus EventBus => eventBus ??= ServiceProvider.Instance.GetService<EventBus>();
        private PlayerEquipment Equipment => equipment ??= ServiceProvider.Instance.GetService<PlayerEquipment>();

        private void Awake()
        {
            openedCallback = OnShopOpened;
            closedCallback = OnShopClosed;
            changedCallback = OnEquippedChanged;

            EventBus.Subscribe(openedCallback);
            EventBus.Subscribe(closedCallback);
            EventBus.Subscribe(changedCallback);

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

            eventBus.UnSubscribe(openedCallback);
            eventBus.UnSubscribe(closedCallback);
            eventBus.UnSubscribe(changedCallback);
        }

        private void Update()
        {
            if (instance != null)
            {
                spinner.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
            }
        }

        private void OnShopOpened(in ShopOpenedEvent e)
        {
            if (isOpen)
            {
                return;
            }

            isOpen = true;
            CreateRenderTexture();
            previewCamera.enabled = true;
            StartCoroutine(OpenRoutine());
        }

        private void OnShopClosed(in ShopClosedEvent e)
        {
            Close();
        }

        private void OnEquippedChanged(in EquippedItemChangedEvent e)
        {
            if (isOpen && bundle.IsLoaded)
            {
                ShowEquipped();
            }
        }

        private IEnumerator OpenRoutine()
        {
            yield return bundle.LoadBundle();

            if (isOpen && bundle.IsLoaded)
            {
                ShowEquipped();
            }
        }

        private void ShowEquipped()
        {
            ShopItem gun = Equipment.GetEquipped(ShopCategory.Guns);
            string modelName = gun != null ? gun.ModelName : null;

            if (string.IsNullOrEmpty(modelName))
            {
                Debug.LogWarning($"El arma equipada ({(gun != null ? gun.name : "ninguna")}) no tiene Model Name; no hay nada que mostrar.");
                ClearInstance();
                return;
            }

            if (modelName == shownModel)
            {
                return;
            }

            requestId++;
            StartCoroutine(SwapRoutine(modelName, requestId));
        }

        private IEnumerator SwapRoutine(string modelName, int id)
        {
            GameObject prefab = null;
            yield return bundle.LoadModel(modelName, loaded => prefab = loaded);

            // Si mientras cargaba se pidió otra arma o se cerró la tienda, este resultado ya no sirve.
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
            FitToSpinner(instance.transform);
            target.enabled = true;
        }

        // Centra el modelo en el pivote y lo escala para que su lado más largo mida fitSize.
        private void FitToSpinner(Transform model)
        {
            Quaternion spin = spinner.rotation;
            spinner.rotation = Quaternion.identity;

            model.localPosition = Vector3.zero;
            model.localRotation = Quaternion.Euler(modelRotation);
            model.localScale = Vector3.one;

            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;

                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }

                float largest = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
                float scale = largest > 0f ? fitSize / largest : 1f;
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

            // Las instancias se destruyen antes de descargar el bundle del que salieron.
            ClearInstance();
            bundle.Unload();

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
