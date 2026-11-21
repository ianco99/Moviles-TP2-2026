namespace GunJump.Core
{
    public sealed class LoadMenuCommand : ICommand
    {
        readonly LevelLoader loader;
        readonly int menuBuildIndex;
        readonly MenuScreen screen;

        public LoadMenuCommand(LevelLoader loader, int menuBuildIndex, MenuScreen screen)
        {
            this.loader = loader;
            this.menuBuildIndex = menuBuildIndex;
            this.screen = screen;
        }

        public void Execute()
        {
            loader.LoadMenu(menuBuildIndex, screen);
        }
    }
}
