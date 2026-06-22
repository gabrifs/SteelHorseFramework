namespace SteelHorse.Framework.Services.Audio
{
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
