using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SteelHorse.Framework.UI
{
    // Animates a RectTransform "cursor" that smoothly follows the currently
    // selected UI element. Requires DOTween.
    public class UIPointer : MonoBehaviour
    {
        [SerializeField] private RectTransform _pointer;
        [SerializeField] private float _moveDuration = 0.15f;

        private RectTransform _currentTarget;

        private IEnumerator Start()
        {
            // Set _currentTarget before yielding so Update doesn't fire a tween
            // on the very first frame when the pointer snaps to its initial position.
            if (EventSystem.current == null) yield break;
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                var selectedRect = selected.GetComponent<RectTransform>();
                if (selectedRect != null)
                    _currentTarget = selectedRect;
            }

            yield return null; // wait one frame for Canvas layout to resolve sizes

            if (_currentTarget != null)
            {
                _pointer.sizeDelta = _currentTarget.rect.size;
                _pointer.position = _currentTarget.position;
                _pointer.gameObject.SetActive(true);
            }
        }

        private void Update()
        {
            if (EventSystem.current == null) return;
            var selected = EventSystem.current.currentSelectedGameObject;

            if (selected != null)
            {
                var selectedRect = selected.GetComponent<RectTransform>();
                var selectable = selected.GetComponent<Selectable>();
                if (selectedRect != null && (selectable == null || selectable.IsInteractable()))
                {
                    if (!_pointer.gameObject.activeSelf)
                        _pointer.gameObject.SetActive(true);

                    if (selectedRect != _currentTarget)
                    {
                        _currentTarget = selectedRect;
                        _pointer.DOMove(_currentTarget.position, _moveDuration).SetUpdate(true);
                        _pointer.DOSizeDelta(_currentTarget.rect.size, _moveDuration).SetUpdate(true);
                    }

                    return;
                }
            }

            if (_pointer.gameObject.activeSelf)
                _pointer.gameObject.SetActive(false);
        }
    }
}
