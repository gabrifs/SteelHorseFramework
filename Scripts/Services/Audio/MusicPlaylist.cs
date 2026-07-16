using UnityEngine;

namespace SteelHorse.Framework.Services.Audio
{
    public enum PlaylistSequenceMode { Sequential, Random }

    [CreateAssetMenu(menuName = "Steel Horse/Audio/Music Playlist", fileName = "New Music Playlist")]
    public class MusicPlaylist : ScriptableObject
    {
        public float FadeOutTime { get { return _fadeOutTime; } }
        public bool HasSongs { get { return _songs != null && _songs.Length > 0; } }

        [SerializeField] private AudioClip[] _songs;
        [SerializeField] private PlaylistSequenceMode _sequenceMode = PlaylistSequenceMode.Sequential;

        [Tooltip("Seconds before a song ends when the next song should start crossfading in. " +
                 "Also used as the crossfade duration when this playlist is explicitly triggered via Play().")]
        [SerializeField, Min(0f)] private float _fadeOutTime = 3f;

        private int _lastPlayedIndex = -1;

        public AudioClip GetNextClip()
        {
            _lastPlayedIndex = GetNextIndex();
            return _songs[_lastPlayedIndex];
        }

        private int GetNextIndex()
        {
            // Single song: skip the no-repeat loop below, which would never terminate.
            if (_songs.Length == 1)
                return 0;

            if (_sequenceMode == PlaylistSequenceMode.Sequential)
                return (_lastPlayedIndex + 1) % _songs.Length;

            int nextIndex;
            do
            {
                nextIndex = Random.Range(0, _songs.Length);
            } while (nextIndex == _lastPlayedIndex);

            return nextIndex;
        }
    }
}
