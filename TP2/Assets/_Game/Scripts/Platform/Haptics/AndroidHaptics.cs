using UnityEngine;
using UnityEngine.Android;

namespace GunJump.Platform
{
    public sealed class AndroidHaptics : IHaptics
    {
        private const string VIBRATOR_CLASS_NAME = "com.example.kuznickilogger.KuznickiVibrator";
        private const string VIBRATE_METHOD = "vibrate";
        private const string CANCEL_METHOD = "cancel";

        private readonly AndroidJavaObject vibrator;

        public AndroidHaptics()
        {
            vibrator = new AndroidJavaObject(VIBRATOR_CLASS_NAME, AndroidApplication.currentActivity);
        }

        public void Vibrate(long milliseconds, int amplitude)
        {
            vibrator.Call(VIBRATE_METHOD, milliseconds, amplitude);
        }

        public void Dispose()
        {
            vibrator.Call(CANCEL_METHOD);
            vibrator.Dispose();
        }
    }
}
