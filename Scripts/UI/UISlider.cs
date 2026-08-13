using UnityEngine;
using UnityEngine.UI;
using SteelHorse.Framework.Services.Audio;

namespace SteelHorse.Framework.UI
{
    [RequireComponent(typeof(Slider))]
    public class UISlider : UISelectableBase
    {
        [SerializeField] private SfxCue _valueChangedSfxCue;
        [Tooltip("Minimum time between value-changed SFX plays. A dragged Slider's onValueChanged can fire many times per frame - without this, that plays this cue just as many times, layering/cutting off the same clip into an audible stutter.")]
        [SerializeField] private float _valueChangedSfxMinInterval = 0.1f;

        private Slider _slider;
        private Slider Slider => _slider != null ? _slider : (_slider = GetComponent<Slider>());

        // Unscaled so the click still plays at the expected pace while the game is
        // paused (Time.timeScale = 0) - Options can be opened from a paused game.
        private float _lastValueChangedSfxTime = float.NegativeInfinity;

        protected override void Awake()
        {
            base.Awake();
            Slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            Slider.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(float _)
        {
            if (Time.unscaledTime - _lastValueChangedSfxTime < _valueChangedSfxMinInterval)
                return;

            _lastValueChangedSfxTime = Time.unscaledTime;
            PlaySfx(_valueChangedSfxCue);
        }
    }
}
