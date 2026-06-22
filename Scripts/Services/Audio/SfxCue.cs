using UnityEngine;
using UnityEngine.Audio;

namespace SteelHorse.Framework.Services.Audio
{
    public enum SfxPlaybackMode { World3D, UI2D }
    public enum ClipSelectionMode { Random, Ordered }

    [CreateAssetMenu(menuName = "Steel Horse/Audio/SFX Cue", fileName = "New SFX Cue")]
    public class SfxCue : ScriptableObject
    {
        public AudioMixerGroup OutputGroup { get { return _outputGroup; } }
        public SfxPlaybackMode PlaybackMode { get { return _playbackMode; } }
        public bool Looped { get { return _looped; } }

        [SerializeField] private AudioClip[] _clips;
        [SerializeField] private ClipSelectionMode _selectionMode = ClipSelectionMode.Random;
        [SerializeField] private bool _looped;
        [SerializeField] private AudioMixerGroup _outputGroup;
        [SerializeField] private SfxPlaybackMode _playbackMode = SfxPlaybackMode.World3D;
        [SerializeField] private Vector2 _volumeRange = new Vector2(1f, 1f);
        [SerializeField] private Vector2 _pitchRange = new Vector2(1f, 1f);

        private int _lastPlayedIndex = -1;

        public AudioClip GetNextClip()
        {
            _lastPlayedIndex = GetNextIndex();
            return _clips[_lastPlayedIndex];
        }

        private int GetNextIndex()
        {
            // Single clip: skip the no-repeat loop below, which would never terminate.
            if (_clips.Length == 1)
                return 0;

            if (_selectionMode == ClipSelectionMode.Ordered)
                return (_lastPlayedIndex + 1) % _clips.Length;

            int nextIndex;
            do
            {
                nextIndex = Random.Range(0, _clips.Length);
            } while (nextIndex == _lastPlayedIndex);

            return nextIndex;
        }

        public float GetVolume()
        {
            return Random.Range(_volumeRange.x, _volumeRange.y);
        }

        public float GetPitch()
        {
            return Random.Range(_pitchRange.x, _pitchRange.y);
        }
    }
}
