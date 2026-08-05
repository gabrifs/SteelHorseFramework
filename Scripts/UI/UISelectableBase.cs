using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SteelHorse.Framework.Services.Audio;

namespace SteelHorse.Framework.UI
{
    // Shared plumbing for Selectable-derived UI widgets (UIButton, UISlider,
    // UIDropdown, ...): the "select" SFX cue that fires the same way
    // (ISelectHandler.OnSelect) for all of them, and mouse-hover-to-select so
    // hovering behaves like gamepad/keyboard navigation. Each subclass wires
    // its own control-specific interaction event to a cue via the protected
    // PlaySfx helper. For platform-conditional visibility, use DisplayOnPlatform.
    [RequireComponent(typeof(Selectable))]
    public abstract class UISelectableBase : MonoBehaviour, ISelectHandler, IPointerEnterHandler
    {
        [Header("SFX")]
        [SerializeField] private SfxCue _selectSfxCue;

        private Selectable _selectable;
        private Selectable Selectable => _selectable != null ? _selectable : (_selectable = GetComponent<Selectable>());

        protected virtual void Awake()
        {
        }

        // Fired by EventSystem on both pointer hover-to-select (via
        // OnPointerEnter below) and gamepad/keyboard navigation, so this single
        // hook covers every input method.
        public void OnSelect(BaseEventData eventData)
        {
            PlaySfx(_selectSfxCue);
        }

        // Mirrors gamepad/keyboard navigation by moving EventSystem selection
        // to whatever's under the mouse. Skipped on mobile: touch fires
        // PointerEnter alongside the tap itself, which would re-enable the
        // auto-highlight look that MenuPanel.Show() and SelectionGuard
        // deliberately clear on touch.
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (PlatformUtility.IsMobilePlatform())
                return;

            if (EventSystem.current == null || !Selectable.IsInteractable())
                return;

            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        protected void PlaySfx(SfxCue cue)
        {
            if (cue == null)
                return;

            GameManagers.Instance.Services.AudioManagerService.PlaySfx(cue);
        }
    }
}
