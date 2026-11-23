using GunJump.UI;
using UnityEngine;

namespace GunJump.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public sealed class TutorialZone : MonoBehaviour
    {
        [Tooltip("Texto que aparece mientras el arma está dentro de la zona.")]
        [TextArea]
        [SerializeField] string hint;

        HudView hud;

        public void Bind(HudView levelHud)
        {
            hud = levelHud;
        }

        void OnTriggerEnter(Collider other)
        {
            if (hud != null && other.GetComponentInParent<Gun>() != null)
                hud.ShowHint(hint, false);
        }

        void OnTriggerExit(Collider other)
        {
            if (hud != null && other.GetComponentInParent<Gun>() != null)
                hud.HideHint();
        }
    }
}
