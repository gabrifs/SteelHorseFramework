using System;
using UnityEngine;

namespace SteelHorse.Framework.Services.Audio
{
    // Mirrors AudioSource's "3D Sound Settings" inspector group, applied per-play
    // by PooledSfxPlayer so a World3D SfxCue can control distance/rolloff/spread/
    // doppler instead of every pooled voice sharing one fixed configuration.
    // Ignored for UI2D cues (UiSfxPlayer's source is never spatialized).
    [Serializable]
    public class SfxSpatialSettings
    {
        public float MinDistance { get { return _minDistance; } }
        public float MaxDistance { get { return _maxDistance; } }
        public AudioRolloffMode RolloffMode { get { return _rolloffMode; } }
        public AnimationCurve CustomRolloffCurve { get { return _customRolloffCurve; } }
        public float Spread { get { return _spread; } }
        public float DopplerLevel { get { return _dopplerLevel; } }

        [SerializeField, Min(0f)] private float _minDistance = 1f;
        [SerializeField, Min(0f)] private float _maxDistance = 500f;
        [SerializeField] private AudioRolloffMode _rolloffMode = AudioRolloffMode.Logarithmic;
        [SerializeField] private AnimationCurve _customRolloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        [SerializeField, Range(0f, 360f)] private float _spread = 0f;
        [SerializeField, Range(0f, 5f)] private float _dopplerLevel = 1f;

        public void Apply(AudioSource source)
        {
            source.minDistance = _minDistance;
            source.maxDistance = _maxDistance;
            source.rolloffMode = _rolloffMode;
            source.spread = _spread;
            source.dopplerLevel = _dopplerLevel;

            if (_rolloffMode == AudioRolloffMode.Custom)
                source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, _customRolloffCurve);
        }
    }
}
