using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SteelHorse.Framework.Services.SceneLoading
{
    public class SceneLoader : MonoBehaviour, ISceneLoader
    {
        [SerializeField] private CanvasGroup _loadingPanel;
        [SerializeField] private LoadingTextAnimator _loadingTextAnimator;
        [SerializeField] private float _fadeDuration = 0.3f;

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            _loadingPanel.blocksRaycasts = true;

            if (_loadingTextAnimator != null)
                _loadingTextAnimator.StartAnimation();

            yield return Fade(0f, 1f);

            // Flush unreferenced assets and collect before loading the new scene to
            // prevent both scenes from being resident in memory simultaneously.
            yield return Resources.UnloadUnusedAssets();
            System.GC.Collect();

            yield return SceneManager.LoadSceneAsync(sceneName);

            yield return Fade(1f, 0f);
            
            if (_loadingTextAnimator != null)
                _loadingTextAnimator.StopAnimation();

            _loadingPanel.blocksRaycasts = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _loadingPanel.alpha = Mathf.Lerp(from, to, elapsed / _fadeDuration);
                yield return null;
            }
            _loadingPanel.alpha = to;
        }
    }
}
