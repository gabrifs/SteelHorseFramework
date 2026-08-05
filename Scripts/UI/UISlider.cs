using UnityEngine;
using UnityEngine.UI;
using SteelHorse.Framework.Services.Audio;

namespace SteelHorse.Framework.UI
{
    [RequireComponent(typeof(Slider))]
    public class UISlider : UISelectableBase
    {
        [SerializeField] private SfxCue _valueChangedSfxCue;

        private Slider _slider;
        private Slider Slider => _slider != null ? _slider : (_slider = GetComponent<Slider>());

        protected override void Awake()
        {
            base.Awake();
            Slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            Slider.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(float _) => PlaySfx(_valueChangedSfxCue);
    }
}
