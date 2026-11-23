using UnityEngine;

namespace GunJump.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public sealed class HazardZone : MonoBehaviour
    {
        LevelSession session;

        public void Bind(LevelSession levelSession)
        {
            session = levelSession;
        }

        void OnTriggerEnter(Collider other)
        {
            if (session != null && other.GetComponentInParent<Gun>() != null)
                session.Lose();
        }
    }
}
