using ianco99.ToolBox.Services;

namespace GunJump.Core
{
    // Atajo para pedir un sonido en una línea. Dando Play directo en una escena de nivel no pasó
    // el Bootstrapper y no hay AudioService: el juego corre igual, en silencio.
    public static class GameAudio
    {
        public static void Play(SoundId id, float volumeScale = 1f)
        {
            ServiceProvider services = ServiceProvider.Instance;
            if (services.ContainsService<AudioService>())
                services.GetService<AudioService>().Play(id, volumeScale);
        }
    }
}
