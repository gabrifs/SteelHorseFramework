using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SteelHorse.Framework.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MenuPanel : MonoBehaviour
    {
        [Serializable]
        private struct PushEntry
        {
            public Button Trigger;
            public MenuPanel Target;
        }

        [SerializeField] private Selectable _defaultFocus;
        [SerializeField] private bool _poppableOnCancel = true;
        [SerializeField] private List<Button> _popButtons;
        [SerializeField] private List<PushEntry> _pushEntries;
        [SerializeField] private UnityEvent _onShow;
        [SerializeField] private UnityEvent _onHide;

        public event Action PopRequested;
        public event Action<MenuPanel, Selectable> PushRequested;

        public bool PoppableOnCancel => _poppableOnCancel;

        private CanvasGroup _canvasGroup;
        private CanvasGroup Canvas => _canvasGroup != null ? _canvasGroup : (_canvasGroup = GetComponent<CanvasGroup>());

        protected virtual void Awake()
        {
            foreach (var btn in _popButtons)
                btn.onClick.AddListener(() => PopRequested?.Invoke());

            foreach (var entry in _pushEntries)
            {
                var target = entry.Target;
                var trigger = entry.Trigger;
                trigger.onClick.AddListener(() => PushRequested?.Invoke(target, trigger));
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var btn in _popButtons)
                btn.onClick.RemoveAllListeners();

            foreach (var entry in _pushEntries)
                entry.Trigger.onClick.RemoveAllListeners();
        }

        public void Pop() => PopRequested?.Invoke();

        public virtual void Show(Selectable overrideFocus = null)
        {
            Canvas.alpha = 1f;
            Canvas.interactable = true;
            Canvas.blocksRaycasts = true;

            var focus = overrideFocus != null ? overrideFocus : _defaultFocus;
            if (focus != null)
                EventSystem.current.SetSelectedGameObject(focus.gameObject);

            _onShow?.Invoke();
        }

        public virtual void Hide()
        {
            Canvas.alpha = 0f;
            Canvas.interactable = false;
            Canvas.blocksRaycasts = false;

            _onHide?.Invoke();
        }
    }
}
