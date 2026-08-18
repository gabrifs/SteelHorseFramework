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

        [Serializable]
        private struct ButtonEvent
        {
            public Button Button;
            public UnityEvent Event;
        }

        [Tooltip("Stay visible (but non-interactive) instead of hiding when a new panel is pushed on top of this one. Still hides fully when this panel itself is popped/closed.")]
        [SerializeField] private bool _stayActiveOnPush;
        [Tooltip("This panel's own UIPointer, if it has one. Optional — panels with no gamepad/keyboard-navigable focus don't need one.")]
        [SerializeField] private UIPointer _pointer;
        [SerializeField] private Selectable _defaultFocus;
        [SerializeField] private bool _poppableOnCancel = true;
        [SerializeField] private List<Button> _popButtons;
        [SerializeField] private List<PushEntry> _pushEntries;
        [SerializeField] private List<ButtonEvent> _buttonEvents;
        [SerializeField] private UnityEvent _onShow;
        [SerializeField] private UnityEvent _onHide;

        public event Action PopRequested;
        public event Action<MenuPanel, Selectable> PushRequested;

        public bool PoppableOnCancel => _poppableOnCancel;

        // Exposed so other scripts can AddListener in code (e.g. a controller resetting
        // its panel's state on show) instead of every consumer needing its own method
        // dragged into this panel's Inspector event list.
        public UnityEvent OnShow => _onShow;
        public UnityEvent OnHide => _onHide;

        private CanvasGroup _canvasGroup;
        private CanvasGroup Canvas => _canvasGroup != null ? _canvasGroup : (_canvasGroup = GetComponent<CanvasGroup>());

        // Optional: panels that are also their own Canvas (e.g. Town location panels)
        // need these disabled while hidden, since a CanvasGroup alpha of 0 alone still
        // renders and still gets raycast against every frame.
        private Canvas _canvas;
        private GraphicRaycaster _graphicRaycaster;

        protected virtual void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _graphicRaycaster = GetComponent<GraphicRaycaster>();

            // Force every panel hidden on startup regardless of how it was left authored
            // in the Inspector, instead of trusting each panel's alpha/interactable/etc.
            // to already be correct. MenuNavigator.Start() then explicitly Shows only the
            // root panel - guaranteed to run after every panel's Awake, since Unity runs
            // the whole scene's Awake phase before any Start - so the root wins regardless
            // of Awake ordering between panels. No OnHide fires here since this is just
            // establishing the starting state, not an actual hide transition.
            ApplyHiddenState();

            foreach (var btn in _popButtons)
                btn.onClick.AddListener(() => PopRequested?.Invoke());

            foreach (var entry in _pushEntries)
            {
                var target = entry.Target;
                var trigger = entry.Trigger;
                trigger.onClick.AddListener(() => PushRequested?.Invoke(target, trigger));
            }

            foreach (var entry in _buttonEvents)
            {
                var buttonEvent = entry.Event;
                entry.Button.onClick.AddListener(() => buttonEvent?.Invoke());
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var btn in _popButtons)
                btn.onClick.RemoveAllListeners();

            foreach (var entry in _pushEntries)
                entry.Trigger.onClick.RemoveAllListeners();

            foreach (var entry in _buttonEvents)
                entry.Button.onClick.RemoveAllListeners();
        }

        public void Pop() => PopRequested?.Invoke();

        public virtual void Show(Selectable overrideFocus = null)
        {
            Canvas.alpha = 1f;
            Canvas.interactable = true;
            Canvas.blocksRaycasts = true;

            if (_canvas != null)
                _canvas.enabled = true;
            if (_graphicRaycaster != null)
                _graphicRaycaster.enabled = true;
            if (_pointer != null)
                _pointer.Show();

            // On mobile there's no gamepad/keyboard navigation to seed, and leaving a
            // button auto-selected would show it highlighted despite nobody touching it.
            // SelectionGuard also disables itself on mobile so it doesn't restore this.
            if (PlatformUtility.IsMobilePlatform())
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                var focus = overrideFocus != null ? overrideFocus : _defaultFocus;
                if (focus != null)
                    EventSystem.current.SetSelectedGameObject(focus.gameObject);
            }

            _onShow?.Invoke();
        }

        // `covering` distinguishes being stacked under a new panel (MenuNavigator.Push)
        // from actually being closed (Pop/PopToRoot/Clear) - only the former respects
        // AlwaysActive, so a panel set to stay visible while covered still disappears
        // normally once it's the one being popped.
        public virtual void Hide(bool covering = false)
        {
            ApplyHiddenState(covering && _stayActiveOnPush);
            _onHide?.Invoke();
        }

        private void ApplyHiddenState(bool staysVisible = false)
        {
            if (!staysVisible)
                Canvas.alpha = 0f;

            Canvas.interactable = false;
            Canvas.blocksRaycasts = false;

            if (_canvas != null)
                _canvas.enabled = staysVisible;
            if (_graphicRaycaster != null)
                _graphicRaycaster.enabled = false;
            if (_pointer != null)
                _pointer.Hide();
        }
    }
}
