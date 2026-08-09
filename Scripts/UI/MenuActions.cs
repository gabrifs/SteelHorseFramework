using UnityEngine;

namespace SteelHorse.Framework.UI
{
    // Public methods here are meant to be wired to MenuPanel's Button Events
    // (or any other UnityEvent) from the Inspector, so panels don't need a
    // bespoke controller for a single Quit/LoadScene button.
    public class MenuActions : MonoBehaviour
    {
        public void QuitApplication()
        {
            Application.Quit();
        }

        public void LoadScene(string sceneName)
        {
            GameManagers.Instance.Services.SceneLoaderService.LoadScene(sceneName);
        }
    }
}
