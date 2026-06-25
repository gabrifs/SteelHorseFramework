using UnityEngine;

namespace SteelHorse.Framework.UI
{
    // Drop on a root GameObject in any scene that should hide and lock the OS cursor.
    public class SystemCursorLocker : MonoBehaviour
    {
        private void Awake() => Cursor.lockState = CursorLockMode.Locked;

        private void OnApplicationFocus(bool hasFocus)
        {
            // Unity releases CursorLockMode.Locked automatically when the application
            // loses focus; re-apply on refocus so the cursor doesn't stay unlocked after alt-tab.
            if (hasFocus)
                Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
