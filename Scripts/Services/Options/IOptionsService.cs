using System;
using UnityEngine;
using SteelHorse.Framework.UI;

namespace SteelHorse.Framework.Services.Options
{
    public interface IOptionsService
    {
        // Unity has no native event for resolution changes, so anything that needs to
        // react to the player picking a new resolution subscribes here instead of polling
        // Screen.width/height. Only fires from SetResolution (a runtime change) - it does
        // not fire during Setup's boot-time apply, since that runs from GameManagers.Awake,
        // before any other object's OnEnable has subscribed yet. Read CurrentResolution for
        // the initial value instead.
        event Action<int, int> ResolutionChanged;

        void Setup();

        float GetVolume(string channelName);
        void SetVolume(string channelName, float linear);

        int CurrentQualityIndex { get; }
        void SetQuality(int index);

        // Curated options from the configured ResolutionOptions asset, filtered to the
        // player's monitor and with its native resolution appended if missing. Indices
        // into this array are what's persisted to PlayerPrefs and what SetResolution takes.
        ResolutionSetting[] Resolutions { get; }
        int CurrentResolutionIndex { get; }
        Vector2Int CurrentResolution { get; }
        void SetResolution(int index);
    }
}
