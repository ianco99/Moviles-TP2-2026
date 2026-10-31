namespace GunJump.Core
{
    // Los sistemas que viven toda la sesión. Lo arma GameBootstrap y SceneLoader se lo entrega a cada escena.
    public sealed class GameContext
    {
        readonly SaveSystem save;

        public GameContext(
            SceneLoader loader,
            CommandInvoker commands,
            PlayerProgress progress,
            SaveSystem save,
            Economy economy,
            LevelCatalog levels)
        {
            Loader = loader;
            Commands = commands;
            Progress = progress;
            this.save = save;
            Economy = economy;
            Levels = levels;
        }

        public SceneLoader Loader { get; }
        public CommandInvoker Commands { get; }
        public PlayerProgress Progress { get; }
        public Economy Economy { get; }
        public LevelCatalog Levels { get; }

        public void SaveProgress()
        {
            save.Save(Progress.CreateSnapshot());
        }
    }
}
