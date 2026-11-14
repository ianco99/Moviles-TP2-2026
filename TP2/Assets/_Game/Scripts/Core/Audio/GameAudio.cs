using ianco99.ToolBox.Services;

namespace GunJump.Core
{
    // Atajo para pedir un sonido en una línea. Dando Play directo en una escena de nivel no pasó
    // el Bootstrapper y no hay AudioService: el juego corre igual, en silencio.
    public static class GameAudio
    {
        public static bool IsAvailable => ServiceProvider.Instance.ContainsService<AudioService>();

        public static void Play(SoundId id, float volumeScale = 1f)
        {
            if (IsAvailable)
                Service.Play(id, volumeScale);
        }

        // Sin AudioService no suena nada: el canal figura apagado.
        public static bool IsEnabled(AudioChannel channel)
        {
            return IsAvailable && Service.IsEnabled(channel);
        }

        // Prende el canal si estaba apagado y al revés. Devuelve cómo quedó.
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
