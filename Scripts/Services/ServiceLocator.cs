using UnityEngine;
using SteelHorse.Framework.Services.Audio;
using SteelHorse.Framework.Services.SceneLoading;

namespace SteelHorse.Framework.Services
{
    public class ServiceLocator : MonoBehaviour
    {
        public IAudioManager AudioManagerService { get; private set; }
        public ISceneLoader SceneLoaderService { get; private set; }

        public void Setup()
        {
            AudioManagerService = GetComponentInChildren<IAudioManager>();
            AudioManagerService.Setup();

            SceneLoaderService = GetComponentInChildren<ISceneLoader>();
        }
    }
}
