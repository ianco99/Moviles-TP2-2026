using ianco99.ToolBox.Services;

namespace GunJump.Core
{
    public static class GameAudio
    {
        public static bool IsAvailable => ServiceProvider.Instance.ContainsService<AudioService>();

        public static void Play(SoundId id, float volumeScale = 1f)
        {
            if (IsAvailable)
                Service.Play(id, volumeScale);
        }

        public static bool IsEnabled(AudioChannel channel)
        {
            return IsAvailable && Service.IsEnabled(channel);
        }

        public static bool Toggle(AudioChannel channel)
        {
            if (!IsAvailable)
                return false;

            AudioService service = Service;
            bool enabled = !service.IsEnabled(channel);
            service.SetEnabled(channel, enabled);
            return enabled;
        }

        static AudioService Service => ServiceProvider.Instance.GetService<AudioService>();
    }
}
