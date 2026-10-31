using System.IO;
using GunJump.Core;
using NUnit.Framework;
using UnityEngine;

namespace GunJump.Tests
{
    public class ProgressTests
    {
        string path;

        [SetUp]
        public void SetUp()
        {
            path = Path.Combine(Application.temporaryCachePath, "gunjump-test-" + System.Guid.NewGuid().ToString("N") + ".json");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        [Test]
        public void Spend_FailsWithoutEnoughCoins()
        {
            var progress = new PlayerProgress();
            progress.AddCoins(100);

            Assert.IsFalse(progress.TrySpend(250));
            Assert.AreEqual(100, progress.Coins);
            Assert.IsTrue(progress.TrySpend(60));
            Assert.AreEqual(40, progress.Coins);
        }

        [Test]
        public void RecordLevel_ReportsFirstClearOnce()
        {
            var progress = new PlayerProgress();

            Assert.IsTrue(progress.RecordLevel("Level1", 1, 500));
            Assert.IsFalse(progress.RecordLevel("Level1", 3, 400));
        }

        [Test]
        public void RecordLevel_KeepsTheBestResult()
        {
            var progress = new PlayerProgress();

            progress.RecordLevel("Level1", 3, 400);
            progress.RecordLevel("Level1", 1, 900);

            Assert.AreEqual(3, progress.StarsFor("Level1"));
            Assert.AreEqual(900, progress.BestScoreFor("Level1"));
            Assert.IsFalse(progress.IsCompleted("Level2"));
        }

        [Test]
        public void Snapshot_SurvivesSaveAndLoad()
        {
            var original = new PlayerProgress();
            original.AddCoins(340);
            original.RecordLevel("Level1", 3, 1200);
            original.RecordLevel("Level2", 1, 300);

            var save = new SaveSystem(path);
            save.Save(original.CreateSnapshot());

            var restored = new PlayerProgress();
            restored.Restore(new SaveSystem(path).Load());

            Assert.AreEqual(340, restored.Coins);
            Assert.AreEqual(3, restored.StarsFor("Level1"));
            Assert.AreEqual(1200, restored.BestScoreFor("Level1"));
            Assert.AreEqual(1, restored.StarsFor("Level2"));
        }

        [Test]
        public void Snapshot_IsACopyNotALiveView()
        {
            var progress = new PlayerProgress();
            progress.RecordLevel("Level1", 1, 100);
            ProgressSnapshot snapshot = progress.CreateSnapshot();

            progress.AddCoins(999);
            progress.RecordLevel("Level1", 3, 800);

            var restored = new PlayerProgress();
            restored.Restore(snapshot);
            Assert.AreEqual(0, restored.Coins);
            Assert.AreEqual(1, restored.StarsFor("Level1"));
        }

        [Test]
        public void Save_OverwritesThePreviousFile()
        {
            var save = new SaveSystem(path);
            var progress = new PlayerProgress();

            progress.AddCoins(10);
            save.Save(progress.CreateSnapshot());
            progress.AddCoins(5);
            save.Save(progress.CreateSnapshot());

            var restored = new PlayerProgress();
            restored.Restore(save.Load());
            Assert.AreEqual(15, restored.Coins);
            Assert.IsFalse(File.Exists(path + ".tmp"));
        }

        [Test]
        public void Load_WithoutFile_ReturnsNull()
        {
            Assert.IsNull(new SaveSystem(path).Load());
        }

        [Test]
        public void Load_WithCorruptFile_ReturnsNull()
        {
            File.WriteAllText(path, "{ esto no es json");

            Assert.IsNull(new SaveSystem(path).Load());
        }
    }
}
