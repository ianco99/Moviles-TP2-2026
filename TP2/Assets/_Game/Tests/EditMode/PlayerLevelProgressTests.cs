using _Game.Scripts.Economy;
using NUnit.Framework;
using UnityEngine;

namespace GunJump.Tests
{
    public class PlayerLevelProgressTests
    {
        const string LEVEL = "Level1";

        [Test]
        public void Record_ReportsFirstClearOnce()
        {
            PlayerLevelProgress progress = new PlayerLevelProgress();

            Assert.IsTrue(progress.Record(LEVEL, 1, 100));
            Assert.IsFalse(progress.Record(LEVEL, 3, 500));
            Assert.IsTrue(progress.IsCompleted(LEVEL));
            Assert.IsFalse(progress.IsCompleted("Level2"));
        }

        [Test]
        public void Record_KeepsTheBestResult()
        {
            PlayerLevelProgress progress = new PlayerLevelProgress();
            progress.Record(LEVEL, 3, 200);
            progress.Record(LEVEL, 1, 900);

            Assert.AreEqual(3, progress.StarsFor(LEVEL));
            Assert.AreEqual(900, progress.BestScoreFor(LEVEL));
        }

        [Test]
        public void Record_RaisesChangedOnlyOnImprovement()
        {
            PlayerLevelProgress progress = new PlayerLevelProgress();
            int changes = 0;
            progress.Changed += () => changes++;

            progress.Record(LEVEL, 2, 300);
            Assert.AreEqual(1, changes);

            progress.Record(LEVEL, 1, 100);
            Assert.AreEqual(1, changes);

            progress.Record(LEVEL, 2, 301);
            Assert.AreEqual(2, changes);
        }

        [Test]
        public void Record_WithoutStarsDoesNotComplete()
        {
            PlayerLevelProgress progress = new PlayerLevelProgress();

            Assert.IsFalse(progress.Record(LEVEL, 0, 0));
            Assert.IsFalse(progress.IsCompleted(LEVEL));
        }

        [Test]
        public void Snapshot_SurvivesRoundTrip()
        {
            PlayerLevelProgress original = new PlayerLevelProgress();
            original.Record(LEVEL, 2, 450);
            original.Record("Level2", 1, 120);

            LevelProgressSnapshot snapshot = original.CreateSnapshot();
            PlayerLevelProgress restored = new PlayerLevelProgress();
            restored.Restore(JsonUtility.FromJson<LevelProgressSnapshot>(JsonUtility.ToJson(snapshot)));

            Assert.AreEqual(2, restored.StarsFor(LEVEL));
            Assert.AreEqual(450, restored.BestScoreFor(LEVEL));
            Assert.AreEqual(1, restored.StarsFor("Level2"));
            Assert.AreEqual(120, restored.BestScoreFor("Level2"));
        }

        [Test]
        public void Totals_AddEveryLevelBestResult()
        {
            PlayerLevelProgress progress = new PlayerLevelProgress();
            progress.Record(LEVEL, 2, 300);
            progress.Record(LEVEL, 3, 100);
            progress.Record("Level2", 1, 250);

            Assert.AreEqual(4, progress.TotalStars);
            Assert.AreEqual(550, progress.TotalBestScore);
        }

        [Test]
        public void Restore_DoesNotRaiseChanged()
        {
            PlayerLevelProgress source = new PlayerLevelProgress();
            source.Record(LEVEL, 3, 700);

            PlayerLevelProgress progress = new PlayerLevelProgress();
            int changes = 0;
            progress.Changed += () => changes++;
            progress.Restore(source.CreateSnapshot());

            Assert.AreEqual(0, changes);
            Assert.AreEqual(3, progress.StarsFor(LEVEL));
        }
    }
}
