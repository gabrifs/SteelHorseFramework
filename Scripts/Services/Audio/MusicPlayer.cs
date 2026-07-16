using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace SteelHorse.Framework.Services.Audio
{
    public class MusicPlayer : MonoBehaviour, IMusicPlayer
    {
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioMixerGroup _channelAGroup;
        [SerializeField] private AudioMixerGroup _channelBGroup;
        [SerializeField] private string _channelAVolumeParameter = "MusicCh1Volume";
        [SerializeField] private string _channelBVolumeParameter = "MusicCh2Volume";

        private MusicChannel _channelA;
        private MusicChannel _channelB;
        private MusicChannel _activeChannel;
        private MusicChannel _inactiveChannel;

        private MusicPlaylist _currentPlaylist;
        private bool _isPlaying;
        private Coroutine _playbackRoutine;

        private void Awake()
        {
            _channelA = new MusicChannel(gameObject.AddComponent<AudioSource>(), _channelAGroup, _mixer, _channelAVolumeParameter);
            _channelB = new MusicChannel(gameObject.AddComponent<AudioSource>(), _channelBGroup, _mixer, _channelBVolumeParameter);
            _activeChannel = _channelA;
            _inactiveChannel = _channelB;
        }

        public void Play(MusicPlaylist playlist)
        {
            if (playlist == null || !playlist.HasSongs)
            {
                Debug.LogWarning("MusicPlayer.Play called with a null or empty playlist.", this);
                return;
            }

            // Already the active playlist: no-op, so repeated/spammy triggers
            // (e.g. a state re-entering the same game mode) don't restart or
            // re-crossfade a song that's already playing.
            if (playlist == _currentPlaylist)
                return;

            CancelPlayback();
            _currentPlaylist = playlist;
            _playbackRoutine = StartCoroutine(PlaybackSession(playlist));
        }

        public void Stop(float fadeDuration = -1f)
        {
            if (!_isPlaying)
                return;

            float duration = fadeDuration < 0f ? _currentPlaylist.FadeOutTime : fadeDuration;

            CancelPlayback();
            _currentPlaylist = null;
            _playbackRoutine = StartCoroutine(FadeOutAndStop(Mathf.Max(0f, duration)));
        }

        // Always called from a normal method (Play/Stop), never from inside
        // the coroutine it stops — safe by construction.
        private void CancelPlayback()
        {
            if (_playbackRoutine == null)
                return;

            StopCoroutine(_playbackRoutine);
            _playbackRoutine = null;
        }

        // Owns one playlist "session" end-to-end: the transition into the
        // first song (instant if nothing was playing yet, crossfaded
        // otherwise), then an indefinite auto-advance loop within that same
        // playlist. Everything after the first yield runs across many frames
        // under this ONE Coroutine handle, so Play()/Stop() only ever need to
        // cancel a single reference — this method never stops itself.
        private IEnumerator PlaybackSession(MusicPlaylist playlist)
        {
            AudioClip clip = playlist.GetNextClip();

            if (_isPlaying)
            {
                // Explicit trigger while something is already playing: crossfade,
                // using the incoming playlist's own FadeOutTime as the transition length.
                yield return Crossfade(clip, playlist.FadeOutTime);
            }
            else
            {
                // Nothing currently playing: start immediately, no fade at all.
                _activeChannel.Play(clip);
                _activeChannel.SetVolume(1f);
                _isPlaying = true;
            }

            while (true)
            {
                float fadeOutTime = Mathf.Max(0f, playlist.FadeOutTime);
                // Never wait a negative amount: if FadeOutTime >= the clip's length,
                // start the crossfade immediately instead of "before the clip started".
                float waitTime = Mathf.Max(0f, clip.length - fadeOutTime);
                yield return new WaitForSeconds(waitTime);

                // Never crossfade for longer than the clip actually has left.
                float fadeDuration = Mathf.Min(fadeOutTime, clip.length);
                clip = playlist.GetNextClip();
                yield return Crossfade(clip, fadeDuration);
            }
        }

        private IEnumerator FadeOutAndStop(float duration)
        {
            MusicChannel channel = _activeChannel;
            float startVolume = channel.GetVolume();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                channel.SetVolume(Mathf.Lerp(startVolume, 0f, elapsed / duration));
                yield return null;
            }

            channel.Stop();
            _isPlaying = false;
            _playbackRoutine = null;
        }

        // Fades the inactive channel up (from 0, with the new clip) while
        // fading the active channel down from ITS CURRENT actual volume (not
        // assumed to be 1) to 0, then swaps which channel is active. Starting
        // "from" at its current volume rather than 1 makes this
        // self-correcting if a previous crossfade was interrupted mid-fade —
        // no special-case detection needed. Shared by both the explicit
        // Play() transition and every auto-advance transition inside
        // PlaybackSession's loop.
        private IEnumerator Crossfade(AudioClip clip, float duration)
        {
            duration = Mathf.Max(0f, duration);

            MusicChannel from = _activeChannel;
            MusicChannel to = _inactiveChannel;

            float fromStartVolume = from.GetVolume();
            to.Play(clip);
            to.SetVolume(0f);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                to.SetVolume(Mathf.Lerp(0f, 1f, t));
                from.SetVolume(Mathf.Lerp(fromStartVolume, 0f, t));
                yield return null;
            }

            to.SetVolume(1f);
            from.Stop();

            _activeChannel = to;
            _inactiveChannel = from;
        }
    }
}
