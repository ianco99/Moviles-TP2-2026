using System;

namespace GunJump.Core
{
    public static class LevelRewards
    {
        // Una estrella por llegar a la meta, dos por eliminar a todos, tres por hacerlo bajo el tiempo objetivo.
        public static int Stars(bool allEnemiesKilled, float elapsed, float targetTime)
        {
            if (!allEnemiesKilled)
                return 1;

            return elapsed <= targetTime ? 3 : 2;
        }

        public static int Coins(int levelCoins, int multiplier, int bonus)
        {
            return levelCoins * multiplier + bonus;
        }

        public static int Score(int levelCoins, int multiplier, float elapsed, float targetTime, int pointsPerCoin, int pointsPerSecondSaved)
        {
            float secondsSaved = Math.Max(0f, targetTime - elapsed);
            return levelCoins * multiplier * pointsPerCoin + (int)(secondsSaved * pointsPerSecondSaved);
        }
    }
}
