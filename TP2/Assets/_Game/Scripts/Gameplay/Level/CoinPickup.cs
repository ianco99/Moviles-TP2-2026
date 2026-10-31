using UnityEngine;

namespace GunJump.Gameplay
{
    // Moneda suelta del nivel: un trigger que el arma junta al pasar.
    [RequireComponent(typeof(Collider))]
    public sealed class CoinPickup : MonoBehaviour
    {
        LevelSession session;
        int value;

        public void Bind(LevelSession levelSession, int coinValue)
        {
            session = levelSession;
            value = coinValue;
        }

        void OnTriggerEnter(Collider other)
        {
            if (session == null || other.GetComponentInParent<Gun>() == null)
                return;

            session.AddCoins(value);
            gameObject.SetActive(false);
        }
    }
}
