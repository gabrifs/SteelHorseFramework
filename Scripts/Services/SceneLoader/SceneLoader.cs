using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SteelHorse.Framework.Services.SceneLoading
{
    public class SceneLoader : MonoBehaviour, ISceneLoader
    {
        [SerializeField] private CanvasGroup _loadingPanel;
        [SerializeField] private LoadingTextAnimator _loadingTextAnimator;

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            _loadingPanel.alpha = 1f;
            _loadingPanel.interactable = true;
            _loadingPanel.blocksRaycasts = true;
            if (_loadingTextAnimator != null)
                _loadingTextAnimator.StartAnimation();

            yield return SceneManager.LoadSceneAsync(sceneName);

            if (_loadingTextAnimator != null)
                _loadingTextAnimator.StopAnimation();
            _loadingPanel.alpha = 0f;
            _loadingPanel.interactable = false;
            _loadingPanel.blocksRaycasts = false;
        }
    }
}
