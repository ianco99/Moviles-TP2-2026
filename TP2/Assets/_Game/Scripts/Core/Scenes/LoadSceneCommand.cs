namespace GunJump.Core
{
    // Reintentar, siguiente nivel y volver al menú son el mismo comando con otra escena.
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
