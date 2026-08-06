using System;
using UnityEngine;

namespace SteelHorse.Framework.UI
{
    [Serializable]
    public class ResolutionSetting
    {
        public Vector2 Resolution { get { return _resolution; } }

        // Inspector-only label so entries are recognizable at a glance when editing the
        // ResolutionOptions asset; the dropdown shown to players is built from Resolution.
        [SerializeField] private string _resolutionName;
        [SerializeField] private Vector2 _resolution;

        // Used to synthesize a dropdown entry for the player's monitor resolution when
        // it isn't already one of the curated options on the ResolutionOptions asset.
        public ResolutionSetting(Vector2 resolution)
        {
            _resolution = resolution;
        }
    }
}
