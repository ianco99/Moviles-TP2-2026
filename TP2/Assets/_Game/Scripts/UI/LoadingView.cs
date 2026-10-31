using GunJump.Core;
using UnityEngine;

namespace GunJump.UI
{
    public sealed class LoadingView : MonoBehaviour, ILoadingView
    {
        [Tooltip("El relleno de la barra. Tiene que estar anclado a la izquierda de su fondo.")]
        [SerializeField] RectTransform fill;

        public void SetProgress(float progress)
        {
            fill.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);
        }
    }
}
