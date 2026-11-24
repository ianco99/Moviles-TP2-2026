using GunJump.Core;
using GunJump.Platform;
using ianco99.ToolBox.Services;
using UnityEngine;

namespace GunJump.Gameplay
{
    public sealed class MultiplierTarget : MonoBehaviour, IBulletTarget
    {
        [Min(1)]
        [SerializeField] int multiplier = 1;
        [Tooltip("Lo que se oculta al acertar.")]
        [SerializeField] GameObject visual;

        LevelSession session;
        bool hitOnce;

        public int Multiplier => multiplier;

        MobilePlatform MobilePlatform => ServiceProvider.Instance.GetService<MobilePlatform>();

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
            Debug.Log($"MultiplierTarget: x{multiplier} hit.");
            GameAudio.Play(SoundId.MultiplierHit);
            MobilePlatform.Vibrate(HapticPulse.Multiplier);
            visual.SetActive(false);
        }
    }
}
