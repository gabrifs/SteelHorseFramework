using UnityEngine;
using UnityEngine.Audio;

namespace SteelHorse.Framework.Services.Audio
{
    // Plain wrapper around one AudioSource "channel" of the two-channel music
    // crossfade — not a MonoBehaviour. MusicPlayer owns every coroutine that
    // drives playback (see MusicPlayer.cs); a channel only carries its
    // AudioSource plus the mixer parameter that controls its audible level.
    //
    // Volume is driven entirely through the mixer's exposed per-channel
    // parameter (not AudioSource.volume, which stays fixed at 1) so that all
    // audible-level control — overall music volume and per-channel crossfade
    // alike — lives in the mixer graph, matching how PlayerOptionsController
    // already drives MasterVolume/MusicVolume/SfxVolume.
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
            Source.volume = 1f; // audible level is controlled via the mixer parameter below, not this

            _mixer = mixer;
            _volumeParameter = volumeParameter;
            SetVolume(0f); // start silent; also primes AudioMixer.GetFloat for this session
        }

        public void Play(AudioClip clip)
        {
            Source.clip = clip;
            Source.Play();
        }

        public void Stop()
        {
            Source.Stop();
            Source.clip = null;
        }

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
