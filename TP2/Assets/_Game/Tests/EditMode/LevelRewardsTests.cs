using GunJump.Core;
using NUnit.Framework;

namespace GunJump.Tests
{
    public class LevelRewardsTests
    {
        [Test]
        public void Stars_OneForFinishing()
        {
            Assert.AreEqual(1, LevelRewards.Stars(false, 10f, 35f));
        }

        [Test]
        public void Stars_TwoForKillingEveryone()
        {
            Assert.AreEqual(2, LevelRewards.Stars(true, 40f, 35f));
        }

        [Test]
        public void Stars_ThreeForKillingEveryoneUnderTheTargetTime()
        {
            Assert.AreEqual(3, LevelRewards.Stars(true, 35f, 35f));
        }

        [Test]
        public void Coins_MultiplierAppliesToLevelCoinsOnly()
        {
            Assert.AreEqual(90 * 2 + 50, LevelRewards.Coins(90, 2, 50));
            Assert.AreEqual(90, LevelRewards.Coins(90, 1, 0));
        }

        [Test]
        public void Score_AddsPointsForSecondsSaved()
        {
            Assert.AreEqual(90 * 2 * 10 + 50, LevelRewards.Score(90, 2, 30f, 35f, 10, 10));
        }

        [Test]
        public void Score_NoPenaltyForBeingSlow()
        {
            Assert.AreEqual(900, LevelRewards.Score(90, 1, 60f, 35f, 10, 10));
        }

        [Test]
        public void Catalog_NextAfter()
        {
            string[] names = { "Level1", "Level2", "Level3" };

            Assert.AreEqual("Level2", LevelCatalog.NextAfter(names, "Level1"));
            Assert.IsNull(LevelCatalog.NextAfter(names, "Level3"));
            Assert.IsNull(LevelCatalog.NextAfter(names, "Menu"));
        }
    }
}
