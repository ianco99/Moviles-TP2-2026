using System;
using System.Collections.Generic;

namespace GunJump.Core
{
    // Originador del Memento: es el único que sabe armar y restaurar un ProgressSnapshot.
    public sealed class PlayerProgress
    {
        readonly Dictionary<string, LevelRecord> levels = new Dictionary<string, LevelRecord>();

        public int Coins { get; private set; }

        public void AddCoins(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            Coins += amount;
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            if (amount > Coins)
                return false;

            Coins -= amount;
            return true;
        }

        public bool IsCompleted(string levelId)
        {
            return StarsFor(levelId) > 0;
        }

        public int StarsFor(string levelId)
        {
            return levels.TryGetValue(levelId, out LevelRecord record) ? record.stars : 0;
        }

        public int BestScoreFor(string levelId)
        {
            return levels.TryGetValue(levelId, out LevelRecord record) ? record.bestScore : 0;
        }

        // Guarda el mejor resultado del nivel. Devuelve true si es la primera vez que se completa.
        public bool RecordLevel(string levelId, int stars, int score)
        {
            if (!levels.TryGetValue(levelId, out LevelRecord record))
            {
                record = new LevelRecord { levelId = levelId };
                levels.Add(levelId, record);
            }

            bool firstClear = record.stars == 0 && stars > 0;
            record.stars = Math.Max(record.stars, stars);
            record.bestScore = Math.Max(record.bestScore, score);
            return firstClear;
        }

        public ProgressSnapshot CreateSnapshot()
        {
            var snapshot = new ProgressSnapshot { coins = Coins };
            foreach (LevelRecord record in levels.Values)
            {
                snapshot.levels.Add(new LevelRecord
                {
                    levelId = record.levelId,
                    stars = record.stars,
                    bestScore = record.bestScore
                });
            }

            return snapshot;
        }

        public void Restore(ProgressSnapshot snapshot)
        {
            levels.Clear();
            Coins = Math.Max(0, snapshot.coins);

            foreach (LevelRecord record in snapshot.levels)
            {
                if (string.IsNullOrEmpty(record.levelId))
                    continue;

                levels[record.levelId] = new LevelRecord
                {
                    levelId = record.levelId,
                    stars = record.stars,
                    bestScore = record.bestScore
                };
            }
        }
    }
}
