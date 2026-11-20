namespace GunJump.Core
{
    public sealed class LoadSceneCommand : ICommand
    {
        readonly SceneLoader loader;
        readonly string sceneName;

        public LoadSceneCommand(SceneLoader loader, string sceneName)
        {
            this.loader = loader;
            this.sceneName = sceneName;
        }

        public void Execute()
        {
            loader.Load(sceneName);
        }
    }
}
