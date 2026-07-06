using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SteelHorse.Framework.UI
{
    public class TabsMenuPanel : MenuPanel
    {
        [Serializable]
        private struct TabEntry
        {
            public Button TabButton;
            public CanvasGroup Content;
        }

        [SerializeField] private List<TabEntry> _tabs;
        [SerializeField] private int _defaultTabIndex = 0;
        [SerializeField] private Button _prevTabButton;
        [SerializeField] private Button _nextTabButton;

        private int _currentTabIndex;

        protected override void Awake()
        {
            base.Awake();

            _currentTabIndex = _defaultTabIndex;

            for (int i = 0; i < _tabs.Count; i++)
            {
                int index = i;
                _tabs[i].TabButton.onClick.AddListener(() => SelectTab(index));
            }

            if (_prevTabButton != null)
                _prevTabButton.onClick.AddListener(PrevTab);

            if (_nextTabButton != null)
                _nextTabButton.onClick.AddListener(NextTab);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var tab in _tabs)
                tab.TabButton.onClick.RemoveAllListeners();

            if (_prevTabButton != null)
                _prevTabButton.onClick.RemoveAllListeners();

            if (_nextTabButton != null)
                _nextTabButton.onClick.RemoveAllListeners();
        }

        public override void Show(Selectable overrideFocus = null)
        {
            base.Show(overrideFocus);
            SelectTab(_currentTabIndex);
        }

        public void SelectTab(int index)
        {
            _currentTabIndex = index;
            for (int i = 0; i < _tabs.Count; i++)
            {
                var active = i == index;
                _tabs[i].Content.alpha = active ? 1f : 0f;
                _tabs[i].Content.interactable = active;
                _tabs[i].Content.blocksRaycasts = active;
            }
        }

        private void PrevTab() => SelectTab((_currentTabIndex - 1 + _tabs.Count) % _tabs.Count);
        private void NextTab() => SelectTab((_currentTabIndex + 1) % _tabs.Count);
    }
}
