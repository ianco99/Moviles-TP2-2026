using GunJump.Core;
using ianco99.ToolBox.Services;
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

        bool restarting;

        void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;
        }

        void Update()
        {
            if (restarting || gun.position.y >= fallLimitY)
                return;

            restarting = true;

            // Entrando desde el menú, LevelLoader recarga sin pantalla de carga y muestra el nivel con el arma ya equipada.
            // Dando Play directo en Prototype no hay servicios: se recarga a secas y el arma queda por defecto.
            ServiceProvider services = ServiceProvider.Instance;
            if (services.ContainsService<LevelLoader>())
                services.GetService<LevelLoader>().Restart();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
