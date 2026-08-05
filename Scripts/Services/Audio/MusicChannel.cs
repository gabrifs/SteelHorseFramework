using UnityEngine;
using UnityEngine.Audio;

namespace SteelHorse.Framework.Services.Audio
{
    // Plain wrapper around one AudioSource "channel" of the two-channel music
    // crossfade — not a MonoBehaviour. MusicPlayer owns every coroutine that
    // drives playback (see MusicPlayer.cs); a channel only carries its
    // AudioSource plus the mixer parameter that controls its audible level.
    //
    // Two independent volume knobs multiply together for the final audible level:
    // - SetVolume(linear)/GetVolume() drive the mixer's exposed per-channel
    //   parameter — the crossfade fader (0..1) and, through it, the overall
    //   MusicVolume/MasterVolume graph, matching how PlayerOptionsController
    //   already drives MasterVolume/MusicVolume/SfxVolume.
    // - Play(clip, baseVolume) sets AudioSource.volume directly — a per-song
    //   loudness ceiling (SoundConfig.BaseVolume) that never touches the mixer,
    //   so a quieter song doesn't get compensated back up by the mixer graph.
    public class MusicChannel
    {
        public AudioSource Source { get; }

        private readonly AudioMixer _mixer;
        private readonly string _volumeParameter;

        public MusicChannel(AudioSource source, AudioMixerGroup outputGroup, AudioMixer mixer, string volumeParameter)
        {
            Source = source;
            Source.outputAudioMixerGroup = outputGroup;
            Source.playOnAwake = false;
            Source.loop = false; // auto-advance is driven manually by MusicPlayer, never by the source itself
            Source.spatialBlend = 0f;
            Source.volume = 1f; // set per-song by Play(clip, baseVolume)
            // Music should keep playing through PauseGame's AudioListener.pause.
            Source.ignoreListenerPause = true;

            _mixer = mixer;
            _volumeParameter = volumeParameter;
            SetVolume(0f); // start silent; also primes AudioMixer.GetFloat for this session
        }

        public void Play(AudioClip clip, float baseVolume = 1f)
        {
            Source.volume = baseVolume;
            Source.clip = clip;
            Source.Play();
        }

        public void Stop()
        {
            Source.Stop();
            Source.clip = null;
        }

        // Crossfade fader (mixer parameter) — independent of Source.volume/baseVolume.
        public void SetVolume(float linear)
        {
            _mixer.SetFloat(_volumeParameter, linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f);
        }

        public float GetVolume()
        {
            _mixer.GetFloat(_volumeParameter, out float decibels);
            return decibels <= -80f ? 0f : Mathf.Pow(10f, decibels / 20f);
        }
    }
}
