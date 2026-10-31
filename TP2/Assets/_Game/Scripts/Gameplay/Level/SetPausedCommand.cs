using GunJump.Core;

namespace GunJump.Gameplay
{
    public sealed class SetPausedCommand : ICommand
    {
        readonly LevelCore level;
        readonly bool paused;

        public SetPausedCommand(LevelCore level, bool paused)
        {
            this.level = level;
            this.paused = paused;
        }

        public void Execute()
        {
            level.SetPaused(paused);
        }
    }
}
