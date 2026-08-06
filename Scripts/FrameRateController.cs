using UnityEngine;

namespace SteelHorse.Framework
{
    // Drop as a sibling on the Game Managers prefab root. QualitySettings.vSyncCount is
    // ignored on Android/iOS - the OS controls presentation timing there - so without an
    // explicit Application.targetFrameRate, mobile renders uncapped and unevenly (reads as
    // stutter/lag) regardless of the vSync toggle in Quality Settings. Desktop is left alone
    // since QualitySettings.vSyncCount already works correctly there.
    public class FrameRateController : MonoBehaviour
    {
        private void Awake()
        {
            if (!PlatformUtility.IsMobilePlatform())
                return;

            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        }
    }
}
