using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SteelHorse.Framework.UI
{
    public class MenuNavigator : MonoBehaviour
    {
        [Serializable]
        private struct PushEntry
        {
            public Button Trigger;
            public MenuPanel Target;
        }

        // A frame pairs a panel with the focus to restore on the panel below when popped.
        private struct StackFrame
        {
            public MenuPanel Panel;
            public Selectable ReturnFocusOnPop;
        }

        [SerializeField] private MenuPanel _rootPanel;
        [SerializeField] private List<PushEntry> _pushEntries;
        [SerializeField] private List<Button> _popButtons;

        private readonly Stack<StackFrame> _history = new();
        private InputAction _cancelAction;

        private void Awake()
        {
            foreach (var entry in _pushEntries)
            {
                var target = entry.Target;
                var trigger = entry.Trigger;
                entry.Trigger.onClick.AddListener(() => Push(target, trigger));
            }

            foreach (var button in _popButtons)
                button.onClick.AddListener(Pop);

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

            foreach (var entry in _pushEntries)
                entry.Trigger.onClick.RemoveAllListeners();

            foreach (var button in _popButtons)
                button.onClick.RemoveAllListeners();
        }

        public void Push(MenuPanel panel, Selectable returnFocusOnPop = null)
        {
            if (_history.TryPeek(out var current))
                current.Panel.Hide();

            _history.Push(new StackFrame { Panel = panel, ReturnFocusOnPop = returnFocusOnPop });
            panel.Show();
        }

        public void Pop()
        {
            if (_history.Count <= 1)
                return;

            var top = _history.Pop();
            top.Panel.Hide();
            _history.Peek().Panel.Show(top.ReturnFocusOnPop);
        }

        public void PopToRoot()
        {
            while (_history.Count > 1)
                _history.Pop().Panel.Hide();

            if (_history.TryPeek(out var root))
                root.Panel.Show();
        }

        private void OnCancelPerformed(InputAction.CallbackContext ctx)
        {
            if (_history.TryPeek(out var top) && top.Panel.PoppableOnCancel)
                Pop();
        }
    }
}
