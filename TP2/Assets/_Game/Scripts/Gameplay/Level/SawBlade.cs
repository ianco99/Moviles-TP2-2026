using UnityEngine;

namespace GunJump.Gameplay
{
    public sealed class SawBlade : MonoBehaviour
    {
        [Tooltip("La parte que gira. El que hace perder es el HazardZone del mismo prefab.")]
        [SerializeField] Transform blade;
        [Tooltip("Grados por segundo. Negativo gira en sentido horario.")]
        [SerializeField] float degreesPerSecond = -360f;

        void Update()
        {
            blade.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}
