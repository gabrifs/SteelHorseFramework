using UnityEngine;
using UnityEngine.Audio;

namespace SteelHorse.Framework.Services.Audio
{
    public class AudioManager : MonoBehaviour, IAudioManager
    {
        public AudioMixer Mixer { get { return _audioMixer; } }

        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private UiSfxPlayer _uiSfxPlayer;

        private ISfxPlayer _worldSfxPlayer;

        public void Setup() { }

        public void RegisterWorldSfxPlayer(ISfxPlayer player)
        {
            _worldSfxPlayer = player;
        }

        public void UnregisterWorldSfxPlayer(ISfxPlayer player)
        {
            if (_worldSfxPlayer == player)
                _worldSfxPlayer = null;
        }

        public SfxHandle PlaySfx(SfxCue cue, Transform parent = null, Vector3? position = null)
        {
            if (cue == null)
                return default;

            switch (cue.PlaybackMode)
            {
                case SfxPlaybackMode.World3D:
                    _worldSfxPlayer ??= FindFirstObjectByType<PooledSfxPlayer>();
                    return (_worldSfxPlayer ?? _uiSfxPlayer).Play(cue, parent, position);

                default:
                    return _uiSfxPlayer.Play(cue, parent, position);
            }
        }

        public void StopSfx(SfxHandle handle)
        {
            if (handle.Player is Object obj && obj == null)
                return;

            handle.Player?.Stop(handle);
        }
    }
}
