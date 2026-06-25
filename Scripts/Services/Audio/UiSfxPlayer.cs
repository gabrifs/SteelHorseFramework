using System.Collections;
using UnityEngine;

namespace SteelHorse.Framework.Services.Audio
{
    public class UiSfxPlayer : MonoBehaviour, ISfxPlayer
    {
        private AudioSource _source;
        private Coroutine _loopRoutine;
        private int _generation;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
        }

        public SfxHandle Play(SfxCue cue, Transform parent = null, Vector3? position = null)
        {
            StopLoop();
            // Increment generation so any handle pointing to the previous sound is
            // treated as stale and rejected by Stop().
            _generation++;

            _source.outputAudioMixerGroup = cue.OutputGroup;

            if (cue.Looped)
            {
                _loopRoutine = StartCoroutine(PlayLooped(cue));
            }
            else
            {
                _source.pitch = cue.GetPitch();
                _source.PlayOneShot(cue.GetNextClip(), cue.GetVolume());
            }

            return new SfxHandle(this, 0, _generation);
        }

        public void Stop(SfxHandle handle)
        {
            if (_generation != handle.Generation)
                return;

            StopLoop();
            _source.Stop();
        }

        private void StopLoop()
        {
            if (_loopRoutine == null)
                return;

            StopCoroutine(_loopRoutine);
            _loopRoutine = null;
        }

        private IEnumerator PlayLooped(SfxCue cue)
        {
            while (true)
            {
                _source.pitch = cue.GetPitch();
                _source.volume = cue.GetVolume();
                _source.clip = cue.GetNextClip();
                _source.Play();
                // Divide by pitch: pitch > 1 speeds playback up, reducing effective duration.
                yield return new WaitForSeconds(_source.clip.length / _source.pitch);
            }
        }
    }
}
