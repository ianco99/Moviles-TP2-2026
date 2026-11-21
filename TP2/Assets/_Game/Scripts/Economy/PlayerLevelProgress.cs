using System;
using System.Collections.Generic;
using ianco99.ToolBox.Services;

namespace _Game.Scripts.Economy
{
    public class PlayerLevelProgress : IService
    {
        public bool IsPersistance => true;

        public event Action Changed;

        private readonly Dictionary<string, (int stars, int bestScore)> recordsByLevel = new Dictionary<string, (int stars, int bestScore)>();

        public int StarsFor(string levelID)
        {
            return recordsByLevel.TryGetValue(levelID, out (int stars, int bestScore) record) ? record.stars : 0;
        }

        public int BestScoreFor(string levelID)
        {
            return recordsByLevel.TryGetValue(levelID, out (int stars, int bestScore) record) ? record.bestScore : 0;
        }

        public bool IsCompleted(string levelID)
        {
            return StarsFor(levelID) > 0;
        }

        public bool Record(string levelID, int stars, int score)
        {
            recordsByLevel.TryGetValue(levelID, out (int stars, int bestScore) previous);

            bool isFirstClear = previous.stars == 0 && stars > 0;
            (int stars, int bestScore) best = (Math.Max(previous.stars, stars), Math.Max(previous.bestScore, score));

            if (best == previous)
            {
                return false;
            }

            recordsByLevel[levelID] = best;
            Changed?.Invoke();
            return isFirstClear;
        }

        public LevelProgressSnapshot CreateSnapshot()
        {
            LevelProgressSnapshot snapshot = new LevelProgressSnapshot();

            foreach (KeyValuePair<string, (int stars, int bestScore)> record in recordsByLevel)
            {
                snapshot.levels.Add(new LevelRecord { levelID = record.Key, stars = record.Value.stars, bestScore = record.Value.bestScore });
            }

            return snapshot;
        }

        public void Restore(LevelProgressSnapshot snapshot)
        {
            recordsByLevel.Clear();

            if (snapshot.levels == null)
            {
                return;
            }

            foreach (LevelRecord saved in snapshot.levels)
            {
                if (saved == null || string.IsNullOrEmpty(saved.levelID))
                {
                    continue;
                }

                recordsByLevel[saved.levelID] = (Math.Max(0, saved.stars), Math.Max(0, saved.bestScore));
            }
        }
    }
}
