namespace SteelHorse.Framework.Services.SceneLoading
{
    public interface ISceneLoader
    {
        void Setup();

        void LoadScene(string sceneName);
    }
}
