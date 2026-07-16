namespace SteelHorse.Framework.Services.Audio
{
    public interface IMusicPlayer
    {
        void Play(MusicPlaylist playlist);

        // fadeDuration < 0 (default): fade out over the currently playing
        // playlist's FadeOutTime. Pass 0 for an instant stop, or any other
        // value for an explicit fade duration.
        void Stop(float fadeDuration = -1f);
    }
}
