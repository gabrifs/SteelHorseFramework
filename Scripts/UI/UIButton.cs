using UnityEngine;
using UnityEngine.UI;
using SteelHorse.Framework.Services.Audio;

namespace SteelHorse.Framework.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButton : UISelectableBase
    {
        [SerializeField] private SfxCue _clickSfxCue;

        private Button _button;
        private Button Button => _button != null ? _button : (_button = GetComponent<Button>());

        protected override void Awake()
        {
            base.Awake();
            Button.onClick.AddListener(PlayClickSfx);
        }

        private void OnDestroy()
        {
            Button.onClick.RemoveListener(PlayClickSfx);
        }

        // Button.onClick already fires for pointer clicks and gamepad/keyboard Submit,
        // so routing through it avoids reimplementing IPointerClickHandler/ISubmitHandler.
        private void PlayClickSfx() => PlaySfx(_clickSfxCue);
    }
}
