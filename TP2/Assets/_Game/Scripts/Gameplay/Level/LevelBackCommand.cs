using GunJump.Core;

namespace GunJump.Gameplay
{
    public sealed class LevelBackCommand : ICommand
    {
        readonly LevelCore level;

        public LevelBackCommand(LevelCore level)
        {
            this.level = level;
        }

        public void Execute()
        {
            level.HandleBack();
        }
    }
}
