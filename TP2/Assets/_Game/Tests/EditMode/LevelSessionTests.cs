using GunJump.Gameplay;
using NUnit.Framework;

namespace GunJump.Tests
{
    public class LevelSessionTests
    {
        [Test]
        public void ZeroAmmo_MeansInfinite()
        {
            var session = new LevelSession(0, 3);

            session.ConsumeAmmo();

            Assert.IsTrue(session.HasInfiniteAmmo);
            Assert.IsTrue(session.CanFire);
            Assert.IsFalse(session.IsOutOfAmmo);
        }

        [Test]
        public void FiniteAmmo_RunsOut()
        {
            var session = new LevelSession(2, 0);

            session.ConsumeAmmo();
            session.ConsumeAmmo();
            session.ConsumeAmmo();

            Assert.AreEqual(0, session.Ammo);
            Assert.IsFalse(session.CanFire);
            Assert.IsTrue(session.IsOutOfAmmo);
        }

        [Test]
        public void Kills_CountEnemiesAndCoins()
        {
            var session = new LevelSession(0, 2);

            session.RegisterKill(10);
            Assert.IsFalse(session.AllEnemiesKilled);
            session.RegisterKill(25);

            Assert.AreEqual(35, session.Coins);
            Assert.AreEqual(0, session.EnemiesRemaining);
            Assert.IsTrue(session.AllEnemiesKilled);
        }

        [Test]
        public void Multiplier_KeepsTheHighestHit()
        {
            var session = new LevelSession(0, 0);

            Assert.AreEqual(1, session.Multiplier);
            session.RegisterMultiplier(4);
            session.RegisterMultiplier(2);

            Assert.AreEqual(4, session.Multiplier);
        }

        [Test]
        public void AfterWinning_NothingElseChanges()
        {
            var session = new LevelSession(5, 1);

            session.Tick(1.5f);
            session.Win();
            session.Lose();
            session.Tick(10f);
            session.RegisterKill(10);
            session.AddCoins(5);
            session.RegisterMultiplier(5);

            Assert.AreEqual(LevelState.Won, session.State);
            Assert.AreEqual(1.5f, session.Elapsed, 0.0001f);
            Assert.AreEqual(0, session.Coins);
            Assert.AreEqual(1, session.Multiplier);
            Assert.IsFalse(session.CanFire);
        }

        [Test]
        public void AfterLosing_CannotWin()
        {
            var session = new LevelSession(0, 0);

            session.Lose();
            session.Win();

            Assert.AreEqual(LevelState.Lost, session.State);
        }
    }
}
