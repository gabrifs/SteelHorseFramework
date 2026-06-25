using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SteelHorse.Framework.UI
{
    // Prevents the EventSystem from silently losing its selection, which would break
    // gamepad and keyboard UI navigation. Drop on any GameObject that stays active
    // throughout the menu lifetime.
    public class SelectionGuard : MonoBehaviour
    {
        private GameObject _lastSelected;

        private void Update()
        {
            if (EventSystem.current == null) return;

            var current = EventSystem.current.currentSelectedGameObject;
            if (current != null && IsSelectableActive(current))
                _lastSelected = current;
            else if (_lastSelected != null && IsSelectableActive(_lastSelected))
                EventSystem.current.SetSelectedGameObject(_lastSelected);
        }

        private static bool IsSelectableActive(GameObject go)
        {
            if (!go.activeInHierarchy) return false;
            var selectable = go.GetComponent<Selectable>();
            return selectable != null && selectable.IsInteractable();
        }
    }
}
