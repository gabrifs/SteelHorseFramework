using UnityEngine;
using UnityEngine.InputSystem;

namespace SteelHorse.Framework.Services.SceneLoading
{
    // Drives a simple scene transition, usually Timeline-based (e.g. an intro/logo scene): LoadNextScene is
    // called from the Timeline itself (a Signal Emitter/Receiver at its end) and can also be
    // triggered early by any of the configured skip inputs.
    public class SkippableSceneLoader : MonoBehaviour
    {
        [SerializeField] private string _nextSceneName;
        [SerializeField] private InputActionReference[] _skipActions;

        private bool _hasLoaded;

        private void Start()
        {
            foreach (var actionReference in _skipActions)
            {
                if (actionReference != null)
                    actionReference.action.performed += OnSkipPerformed;
            }
        }

        private void OnDestroy()
        {
            foreach (var actionReference in _skipActions)
            {
                if (actionReference != null)
                    actionReference.action.performed -= OnSkipPerformed;
            }
        }

        private void OnSkipPerformed(InputAction.CallbackContext ctx) => LoadNextScene();

        public void LoadNextScene()
        {
            // Timeline end and a skip input can both call this - only act on the first.
            if (_hasLoaded)
                return;

            _hasLoaded = true;
            GameManagers.Instance.Services.SceneLoaderService.LoadScene(_nextSceneName);
        }
    }
}
