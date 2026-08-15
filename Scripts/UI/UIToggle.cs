using UnityEngine;
using UnityEngine.UI;
using SteelHorse.Framework.Services.Audio;

namespace SteelHorse.Framework.UI
{
    [RequireComponent(typeof(Toggle))]
    public class UIToggle : UISelectableBase
    {
        [SerializeField] private SfxCue _valueChangedSfxCue;

        private Toggle _toggle;
        private Toggle Toggle => _toggle != null ? _toggle : (_toggle = GetComponent<Toggle>());

        protected override void Awake()
        {
            base.Awake();
            Toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            Toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(bool _) => PlaySfx(_valueChangedSfxCue);
    }
}
