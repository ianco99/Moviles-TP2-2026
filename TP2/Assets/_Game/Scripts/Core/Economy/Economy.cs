namespace GunJump.Core
{
    public sealed class Economy
    {
        readonly PlayerProgress progress;

        public Economy(PlayerProgress progress, EconomyConfig config)
        {
            this.progress = progress;
            Config = config;
        }

        public EconomyConfig Config { get; }

        // Registra el resultado del nivel y acredita las monedas. Devuelve cuántas se acreditaron.
        public int GrantLevelReward(string levelId, int levelCoins, int multiplier, int stars, int score)
        {
            bool firstClear = progress.RecordLevel(levelId, stars, score);
            int reward = LevelRewards.Coins(levelCoins, multiplier, firstClear ? Config.FirstClearBonus : 0);
            progress.AddCoins(reward);
            return reward;
        }
    }
}
