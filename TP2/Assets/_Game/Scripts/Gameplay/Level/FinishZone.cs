using UnityEngine;

namespace GunJump.Gameplay
{
    // La meta: un trigger. Cuando el arma entra, el nivel está ganado.
    [RequireComponent(typeof(Collider))]
    public sealed class FinishZone : MonoBehaviour
    {
        LevelSession session;

        public void Bind(LevelSession levelSession)
        {
            session = levelSession;
        }

        void OnTriggerEnter(Collider other)
        {
            if (session != null && other.GetComponentInParent<Gun>() != null)
                session.Win();
        }
    }
}
