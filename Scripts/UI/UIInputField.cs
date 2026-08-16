using UnityEngine;
using TMPro;
using SteelHorse.Framework.Services.Audio;

namespace SteelHorse.Framework.UI
{
    [RequireComponent(typeof(TMP_InputField))]
    public class UIInputField : UISelectableBase
    {
        [SerializeField] private SfxCue _valueChangedSfxCue;

        private TMP_InputField _inputField;
        private TMP_InputField InputField => _inputField != null ? _inputField : (_inputField = GetComponent<TMP_InputField>());

        protected override void Awake()
        {
            base.Awake();
            InputField.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            InputField.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(string _) => PlaySfx(_valueChangedSfxCue);
    }
}
