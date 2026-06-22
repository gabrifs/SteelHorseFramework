using UnityEngine;

namespace SteelHorse.Framework.Services.Audio
{
    public interface ISfxPlayer
    {
        SfxHandle Play(SfxCue cue, Transform parent = null, Vector3? position = null);
        void Stop(SfxHandle handle);
    }
}
