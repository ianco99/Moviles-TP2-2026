namespace GunJump.Core
{
    public sealed class GameContext
    {
        readonly SaveSystem save;

        readonly SceneLoader loader;
        public SceneLoader Loader => loader;

        readonly CommandInvoker commands;
        public CommandInvoker Commands => commands;

        readonly PlayerProgress progress;
        public PlayerProgress Progress => progress;

        readonly Economy economy;
        public Economy Economy => economy;

        readonly LevelCatalog levels;
        public LevelCatalog Levels => levels;

        public GameContext(
            SceneLoader loader,
            CommandInvoker commands,
            PlayerProgress progress,
            SaveSystem save,
            Economy economy,
            LevelCatalog levels)
        {
            this.loader = loader;
            this.commands = commands;
            this.progress = progress;
            this.save = save;
            this.economy = economy;
            this.levels = levels;
        }

        public void SaveProgress()
        {
            save.Save(Progress.CreateSnapshot());
        }
    }
}
