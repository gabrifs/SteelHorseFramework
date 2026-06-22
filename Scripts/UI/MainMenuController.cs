using UnityEngine;
using UnityEngine.UI;

namespace SteelHorse.Framework.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string _gameSceneName;

        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;
        

        private void Awake()
        {
            _playButton.onClick.AddListener(OnPlayClicked);
            _quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveListener(OnPlayClicked);
            _quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        private void OnPlayClicked()
        {
            GameManagers.Instance.Services.SceneLoaderService.LoadScene(_gameSceneName);
        }

        private void OnQuitClicked()
        {
            Application.Quit();
        }
    }
}
