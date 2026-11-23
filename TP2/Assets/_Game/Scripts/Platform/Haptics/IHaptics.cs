using System;

namespace GunJump.Platform
{
    public interface IHaptics : IDisposable
    {
        void Vibrate(long milliseconds, int amplitude);
    }
}
