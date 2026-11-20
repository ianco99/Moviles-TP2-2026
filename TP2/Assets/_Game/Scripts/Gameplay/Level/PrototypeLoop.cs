using GunJump.Core;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GunJump.Gameplay
{
    public sealed class PrototypeLoop : MonoBehaviour
    {
        const int TARGET_FRAME_RATE = 60;

        [SerializeField] Transform gun;
        [SerializeField] float fallLimitY = -12f;

        bool restarting;

        void Awake()
        {
            Application.targetFrameRate = TARGET_FRAME_RATE;
        }

        void Update()
        {
            if (restarting || gun.position.y >= fallLimitY)
                return;

            restarting = true;
            GameAudio.Play(SoundId.LevelLose);

            ServiceProvider services = ServiceProvider.Instance;
            if (services.ContainsService<LevelLoader>())
                services.GetService<LevelLoader>().Restart();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
