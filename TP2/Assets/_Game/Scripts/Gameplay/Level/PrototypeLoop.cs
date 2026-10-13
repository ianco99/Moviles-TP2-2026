using UnityEngine;
using UnityEngine.SceneManagement;

namespace GunJump.Gameplay
{
    // Solo para el prototipo de la semana 1: reinicia la escena cuando el arma se cae.
    // La semana 2 lo reemplaza por el flujo real de nivel (meta, resultado, reintentar).
    public sealed class PrototypeLoop : MonoBehaviour
    {
        const int TargetFrameRate = 60;

        [SerializeField] Transform gun;
        [SerializeField] float fallLimitY = -12f;

        void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;
        }

        void Update()
        {
            if (gun.position.y < fallLimitY)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
