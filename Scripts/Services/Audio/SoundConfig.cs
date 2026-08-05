using System;
using UnityEngine;

namespace SteelHorse.Framework.Services.Audio
{
    // Pairs an AudioClip with the volume it should play back at, so SfxCue/MusicPlaylist
    // don't need a parallel volume array (or a single fixed volume) to vary loudness per clip.
    [Serializable]
    public class SoundConfig
    {
        public AudioClip Clip { get { return _clip; } }
        public float BaseVolume { get { return _baseVolume; } }

        [SerializeField] private AudioClip _clip;
        [SerializeField, Min(0f)] private float _baseVolume = 1f;
    }
}
