using UnityEngine;

namespace SteelHorse.Framework.UI
{
    // Drop on a root GameObject in any scene that should hide and lock the OS cursor.
    // Skips entirely on mobile — there's no OS cursor to lock, and leaving it enabled
    // would make testing the game in the Editor (e.g. Device Simulator) awkward.
    public class SystemCursorLocker : MonoBehaviour
    {
        private void Awake()
        {
            if (PlatformUtility.IsMobilePlatform())
                return;

            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (PlatformUtility.IsMobilePlatform())
                return;

            // Unity releases CursorLockMode.Locked automatically when the application
            // loses focus; re-apply on refocus so the cursor doesn't stay unlocked after alt-tab.
            if (hasFocus)
                Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
