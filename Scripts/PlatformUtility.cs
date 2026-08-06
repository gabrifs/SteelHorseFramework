using UnityEngine;

namespace SteelHorse.Framework
{
    // Shared platform check for code that behaves differently on touch devices —
    // hiding desktop-only cursor/navigation UI, showing mobile-only controls, etc.
    public static class PlatformUtility
    {
        public static bool IsMobilePlatform()
        {
            // UnityEngine.Device.Application (not UnityEngine.Application) is the
            // Device Simulator-aware variant: in the Editor with the Simulator window
            // active it reports the simulated device's platform; plain
            // UnityEngine.Application.platform always reports the real Editor platform
            // (WindowsEditor/OSXEditor/...) regardless of the Simulator's selection.
            var platform = UnityEngine.Device.Application.platform;
            return platform == RuntimePlatform.Android
                || platform == RuntimePlatform.IPhonePlayer;
        }
    }
}
