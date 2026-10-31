using UnityEngine;

namespace GunJump.Gameplay
{
    // Blanco del multiplicador final: hay que pegarle con una bala antes de cruzar la meta.
    public sealed class MultiplierTarget : MonoBehaviour, IBulletTarget
    {
        [Min(1)]
        [SerializeField] int multiplier = 1;
        [Tooltip("Lo que se oculta al acertar.")]
        [SerializeField] GameObject visual;

        LevelSession session;
        bool hitOnce;

        public int Multiplier => multiplier;

        public void Bind(LevelSession levelSession)
        {
            session = levelSession;
        }

        public void TakeHit(in HitInfo hit)
        {
            if (hitOnce || session == null)
                return;

            hitOnce = true;
            session.RegisterMultiplier(multiplier);
            visual.SetActive(false);
        }
    }
}
