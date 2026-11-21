namespace GunJump.Core
{
    public sealed class RestartSceneCommand : ICommand
    {
        readonly LevelLoader loader;

        public RestartSceneCommand(LevelLoader loader)
        {
            this.loader = loader;
        }

        public void Execute()
        {
            loader.Restart();
        }
    }
}
