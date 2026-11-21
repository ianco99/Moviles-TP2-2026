namespace GunJump.Core
{
    public sealed class LoadSceneCommand : ICommand
    {
        readonly LevelLoader loader;
        readonly int buildIndex;

        public LoadSceneCommand(LevelLoader loader, int buildIndex)
        {
            this.loader = loader;
            this.buildIndex = buildIndex;
        }

        public void Execute()
        {
            loader.Load(buildIndex);
        }
    }
}
