using UnityEngine;
using UnityEngine.EventSystems;
using SteelHorse.Framework.Services.Audio;

namespace SteelHorse.Framework.UI
{
    // Shared plumbing for Selectable-derived UI widgets (UIButton, UISlider,
    // UIDropdown, ...): the "select" SFX cue that fires the same way
    // (ISelectHandler.OnSelect) for all of them. Each subclass wires its own
    // control-specific interaction event to a cue via the protected PlaySfx
    // helper. For platform-conditional visibility, use DisplayOnPlatform.
    public abstract class UISelectableBase : MonoBehaviour, ISelectHandler
    {
        [Header("SFX")]
        [SerializeField] private SfxCue _selectSfxCue;

        protected virtual void Awake()
        {
        }

        // Fired by EventSystem on both pointer hover-to-select and gamepad/keyboard
        // navigation, so this single hook covers every input method.
        public void OnSelect(BaseEventData eventData)
        {
            PlaySfx(_selectSfxCue);
        }

        protected void PlaySfx(SfxCue cue)
        {
            if (cue == null)
                return;

            GameManagers.Instance.Services.AudioManagerService.PlaySfx(cue);
        }
    }
}
