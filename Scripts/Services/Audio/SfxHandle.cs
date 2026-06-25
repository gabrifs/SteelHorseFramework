namespace SteelHorse.Framework.Services.Audio
{
    // Lightweight token returned by ISfxPlayer.Play. The generation counter lets
    // Stop() verify the handle still refers to the same playback rather than a
    // voice slot that has since been reused for a different sound.
    public readonly struct SfxHandle
    {
        public ISfxPlayer Player { get { return _player; } }
        public int VoiceIndex { get { return _voiceIndex; } }
        public int Generation { get { return _generation; } }

        private readonly ISfxPlayer _player;
        private readonly int _voiceIndex;
        private readonly int _generation;

        public SfxHandle(ISfxPlayer player, int voiceIndex, int generation)
        {
            _player = player;
            _voiceIndex = voiceIndex;
            _generation = generation;
        }
    }
}
