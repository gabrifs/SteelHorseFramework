using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SteelHorse.Framework.UI
{
    // Animates a RectTransform "cursor" that smoothly follows the currently
    // selected UI element. Lives on its own Screen Space Overlay canvas so it
    // renders above every other canvas; because of that, the selected
    // element's rect is re-projected through screen space before being
    // applied, so it lines up correctly no matter which canvas (render mode,
    // camera, or CanvasScaler factor) the selected element belongs to.
    // Requires DOTween.
    [RequireComponent(typeof(Canvas))]
    public class UIPointer : MonoBehaviour
    {
        [SerializeField] private RectTransform _pointer;
        [SerializeField] private float _moveDuration = 0.15f;

        private Canvas _pointerCanvas;
        private RectTransform _pointerParent;
        private RectTransform _currentTarget;

        private void Awake()
        {
            _pointerCanvas = GetComponent<Canvas>();
            _pointerParent = _pointer.parent as RectTransform;
        }

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

            if (_currentTarget != null && TryGetScreenRect(_currentTarget, out var center, out var size))
            {
                _pointer.sizeDelta = size;
                _pointer.position = center;
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
                        if (TryGetScreenRect(_currentTarget, out var center, out var size))
                        {
                            _pointer.DOMove(center, _moveDuration).SetUpdate(true);
                            _pointer.DOSizeDelta(size, _moveDuration).SetUpdate(true);
                        }
                    }

                    return;
                }
            }

            if (_pointer.gameObject.activeSelf)
                _pointer.gameObject.SetActive(false);
        }

        // Converts the target rect's world corners into screen space, then back
        // into the pointer canvas's space. Going through screen space is what
        // lets this work across canvases with different render modes, cameras,
        // or CanvasScaler scale factors instead of assuming a shared space.
        private bool TryGetScreenRect(RectTransform target, out Vector3 center, out Vector2 size)
        {
            center = default;
            size = default;

            var targetCanvasComponent = target.GetComponentInParent<Canvas>();
            if (targetCanvasComponent == null || _pointerParent == null)
                return false;
            var targetCanvas = targetCanvasComponent.rootCanvas;

            var targetCamera = targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : targetCanvas.worldCamera;
            var pointerCamera = _pointerCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _pointerCanvas.worldCamera;

            var corners = new Vector3[4];
            target.GetWorldCorners(corners);

            var screenMin = RectTransformUtility.WorldToScreenPoint(targetCamera, corners[0]); // bottom-left
            var screenMax = RectTransformUtility.WorldToScreenPoint(targetCamera, corners[2]); // top-right

            if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(_pointerParent, screenMin, pointerCamera, out var worldMin))
                return false;
            if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(_pointerParent, screenMax, pointerCamera, out var worldMax))
                return false;

            center = (worldMin + worldMax) * 0.5f;

            var localMin = _pointerParent.InverseTransformPoint(worldMin);
            var localMax = _pointerParent.InverseTransformPoint(worldMax);
            size = new Vector2(Mathf.Abs(localMax.x - localMin.x), Mathf.Abs(localMax.y - localMin.y));

            return true;
        }
    }
}
