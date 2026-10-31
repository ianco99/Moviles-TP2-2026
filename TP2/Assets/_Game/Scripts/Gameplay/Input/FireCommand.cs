using GunJump.Core;

namespace GunJump.Gameplay
{
    public sealed class FireCommand : ICommand
    {
        readonly Gun gun;
        readonly LevelSession session;

        public FireCommand(Gun gun, LevelSession session)
        {
            this.gun = gun;
            this.session = session;
        }

        public void Execute()
        {
            if (session.CanFire && gun.TryFire())
                session.ConsumeAmmo();
        }
    }
}
