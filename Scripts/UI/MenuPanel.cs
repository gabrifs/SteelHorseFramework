using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SteelHorse.Framework.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MenuPanel : MonoBehaviour
    {
        [SerializeField] private Selectable _defaultFocus;
        [SerializeField] private bool _poppableOnCancel = true;
        [SerializeField] private UnityEvent _onShow;
        [SerializeField] private UnityEvent _onHide;

        public bool PoppableOnCancel => _poppableOnCancel;

        private CanvasGroup _canvasGroup;
        private CanvasGroup Canvas => _canvasGroup != null ? _canvasGroup : (_canvasGroup = GetComponent<CanvasGroup>());

        public void Show(Selectable overrideFocus = null)
        {
            Canvas.alpha = 1f;
            Canvas.interactable = true;
            Canvas.blocksRaycasts = true;

            var focus = overrideFocus != null ? overrideFocus : _defaultFocus;
            if (focus != null)
                EventSystem.current.SetSelectedGameObject(focus.gameObject);

            _onShow?.Invoke();
        }

        public void Hide()
        {
            Canvas.alpha = 0f;
            Canvas.interactable = false;
            Canvas.blocksRaycasts = false;

            _onHide?.Invoke();
        }
    }
}
