using UnityEngine;

namespace SteelHorse.Framework.UI
{
    // Hides this GameObject on platforms disabled below, via SetActive — works on any
    // GameObject (buttons, prompts, whole panels), not just Selectable-derived widgets.
    public class DisplayOnPlatform : MonoBehaviour
    {
        [SerializeField] private bool _desktop = true;
        [SerializeField] private bool _mobile = true;

        private void Awake()
        {
            bool visible = PlatformUtility.IsMobilePlatform() ? _mobile : _desktop;
            gameObject.SetActive(visible);
        }
    }
}
