using UnityEngine;

namespace GunJump.Core
{
    [CreateAssetMenu(menuName = "GunJump/Economy Config", fileName = "EconomyConfig")]
    public sealed class EconomyConfig : ScriptableObject
    {
        [Header("Monedas")]
        [Tooltip("Monedas que da cada moneda suelta del nivel.")]
        [SerializeField] int coinPickupValue = 5;
        [Tooltip("Monedas extra la primera vez que se completa un nivel.")]
        [SerializeField] int firstClearBonus = 50;

        [Header("Puntaje")]
        [SerializeField] int pointsPerCoin = 10;
        [Tooltip("Puntos por cada segundo por debajo del tiempo objetivo.")]
        [SerializeField] int pointsPerSecondSaved = 10;

        public int CoinPickupValue => coinPickupValue;
        public int FirstClearBonus => firstClearBonus;
        public int PointsPerCoin => pointsPerCoin;
        public int PointsPerSecondSaved => pointsPerSecondSaved;
    }
}
