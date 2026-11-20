namespace GunJump.Core
{
    public sealed class Economy
    {
        readonly PlayerProgress progress;

        readonly EconomyConfig config;
        public EconomyConfig Config => config;

        public Economy(PlayerProgress progress, EconomyConfig config)
        {
            this.progress = progress;
            this.config = config;
        }

        public int GrantLevelReward(string levelId, int levelCoins, int multiplier, int stars, int score)
        {
            bool firstClear = progress.RecordLevel(levelId, stars, score);
            int reward = LevelRewards.Coins(levelCoins, multiplier, firstClear ? Config.FirstClearBonus : 0);
            progress.AddCoins(reward);
            return reward;
        }
    }
}
