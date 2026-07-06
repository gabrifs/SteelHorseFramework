using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SteelHorse.Framework.UI
{
    public class MenuNavigator : MonoBehaviour
    {
        // A frame pairs a panel with the focus to restore on the panel below when popped.
        private struct StackFrame
        {
            public MenuPanel Panel;
            public Selectable ReturnFocusOnPop;
        }

        [SerializeField] private MenuPanel _rootPanel;

        private readonly Stack<StackFrame> _history = new();
        private InputAction _cancelAction;

        private void Awake()
        {
            if (_rootPanel != null)
                Push(_rootPanel);
        }

        private void Start()
        {
            // Resolved in Start rather than Awake: EventSystem.current may not be
            // initialised yet when multiple objects share the default execution order.
            if (EventSystem.current.TryGetComponent<InputSystemUIInputModule>(out var inputModule))
            {
                _cancelAction = inputModule.cancel.action;
                _cancelAction.performed += OnCancelPerformed;
            }
        }

        private void OnDestroy()
        {
            if (_cancelAction != null)
                _cancelAction.performed -= OnCancelPerformed;

            foreach (var frame in _history)
            {
                frame.Panel.PopRequested -= Pop;
                frame.Panel.PushRequested -= Push;
            }
        }

        public void Push(MenuPanel panel, Selectable returnFocusOnPop = null)
        {
            if (_history.TryPeek(out var current))
                current.Panel.Hide();

            panel.PopRequested += Pop;
            panel.PushRequested += Push;

            _history.Push(new StackFrame { Panel = panel, ReturnFocusOnPop = returnFocusOnPop });
            panel.Show();
        }

        public void Pop()
        {
            if (_history.Count <= 1)
                return;

            var top = _history.Pop();
            top.Panel.PopRequested -= Pop;
            top.Panel.PushRequested -= Push;
            top.Panel.Hide();

            _history.Peek().Panel.Show(top.ReturnFocusOnPop);
        }

        public void PopToRoot()
        {
            while (_history.Count > 1)
            {
                var frame = _history.Pop();
                frame.Panel.PopRequested -= Pop;
                frame.Panel.PushRequested -= Push;
                frame.Panel.Hide();
            }

            if (_history.TryPeek(out var root))
                root.Panel.Show();
        }

        public void Clear()
        {
            while (_history.Count > 0)
            {
                var frame = _history.Pop();
                frame.Panel.PopRequested -= Pop;
                frame.Panel.PushRequested -= Push;
                frame.Panel.Hide();
            }
        }

        private void OnCancelPerformed(InputAction.CallbackContext ctx)
        {
            if (_history.TryPeek(out var top) && top.Panel.PoppableOnCancel)
                Pop();
        }
    }
}
