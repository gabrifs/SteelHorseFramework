using System.Collections;
using TMPro;
using UnityEngine;

namespace SteelHorse.Framework.Services.SceneLoading
{
    public class LoadingTextAnimator : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private string[] _texts;
        [SerializeField] private float _delay = 0.5f;

        private Coroutine _routine;
        private int _index;

        public void StartAnimation()
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _index = 0;
            _routine = StartCoroutine(AnimationRoutine());
        }

        public void StopAnimation()
        {
            if (_routine == null) return;
            StopCoroutine(_routine);
            _routine = null;
        }

        private IEnumerator AnimationRoutine()
        {
            while (true)
            {
                _label.text = _texts[_index];
                _index = (_index + 1) % _texts.Length;
                yield return new WaitForSeconds(_delay);
            }
        }
    }
}
