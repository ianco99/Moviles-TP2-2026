using GunJump.Core;

namespace GunJump.Gameplay
{
    public sealed class FireCommand : ICommand
    {
        readonly Gun gun;

        public FireCommand(Gun gun)
        {
            this.gun = gun;
        }

        public void Execute()
        {
            gun.TryFire();
        }
    }
}
