using UnityEngine;

namespace SteelHorse.Framework.Services.Audio
{
    public interface IAudioManager
    {
        public void Setup();
        public void RegisterWorldSfxPlayer(ISfxPlayer player);
        public void UnregisterWorldSfxPlayer(ISfxPlayer player);
        public SfxHandle PlaySfx(SfxCue cue, Transform parent = null, Vector3? position = null);
        public void StopSfx(SfxHandle handle);
    }
}