using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SteelHorse.Framework.UI
{
    public class PauseGame : MonoBehaviour
    {
        public static bool IsPaused { get; private set; }

        // Set by game code to prevent pausing while a blocking screen is active
        public static bool IsPauseBlocked { get; set; }

        [SerializeField] private MenuNavigator _navigator;
        [SerializeField] private MenuPanel _pausePanel;
        [SerializeField] private InputActionReference _pauseActionReference;

        [Header("Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private string _quitSceneName;

        private InputAction _pauseAction;

        private void Awake()
        {
            _pausePanel.PopRequested += OnPausePanelPopped;

            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(Resume);

            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void Start()
        {
            // Resolved in Start rather than Awake: EventSystem.current may not be
            // initialised yet when multiple objects share the default execution order.
            if (_pauseActionReference != null)
            {
                _pauseAction = _pauseActionReference.action;
                _pauseAction.performed += OnPausePerformed;
            }
        }

        private void OnDestroy()
        {
            _pausePanel.PopRequested -= OnPausePanelPopped;

            if (_pauseAction != null)
                _pauseAction.performed -= OnPausePerformed;

            if (_resumeButton != null)
                _resumeButton.onClick.RemoveAllListeners();

            if (_quitButton != null)
                _quitButton.onClick.RemoveAllListeners();

            if (IsPaused)
                ResetState();
        }

        public void Pause()
        {
            if (IsPaused || IsPauseBlocked) return;
            IsPaused = true;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            _navigator.Push(_pausePanel);
        }

        public void Resume()
        {
            if (!IsPaused) return;
            ResetState();
            _navigator.Clear();
        }

        private void OnQuitClicked()
        {
            Resume();
            GameManagers.Instance.Services.SceneLoaderService.LoadScene(_quitSceneName);
        }

        private void OnPausePerformed(InputAction.CallbackContext ctx) => Pause();

        private void OnPausePanelPopped() => ResetState();

        private void ResetState()
        {
            if (!IsPaused) return;
            IsPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}
