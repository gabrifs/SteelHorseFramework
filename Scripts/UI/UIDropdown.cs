using UnityEngine;
using TMPro;
using SteelHorse.Framework.Services.Audio;

namespace SteelHorse.Framework.UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class UIDropdown : UISelectableBase
    {
        [SerializeField] private SfxCue _valueChangedSfxCue;

        private TMP_Dropdown _dropdown;
        private TMP_Dropdown Dropdown => _dropdown != null ? _dropdown : (_dropdown = GetComponent<TMP_Dropdown>());

        protected override void Awake()
        {
            base.Awake();
            Dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            Dropdown.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(int _) => PlaySfx(_valueChangedSfxCue);
    }
}
