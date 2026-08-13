using UnityEngine;
using SteelHorse.Framework.Services.Audio;
using SteelHorse.Framework.Services.Database;
using SteelHorse.Framework.Services.Input;
using SteelHorse.Framework.Services.Networking;
using SteelHorse.Framework.Services.Options;
using SteelHorse.Framework.Services.SceneLoading;

namespace SteelHorse.Framework.Services
{
    public class ServiceLocator : MonoBehaviour
    {
        public IAudioManager AudioManagerService { get; private set; }
        public IMusicPlayer MusicPlayerService { get; private set; }
        public ISceneLoader SceneLoaderService { get; private set; }
        public IApiClient ApiClientService { get; private set; }
        public IDatabaseService DatabaseService { get; private set; }
        public IInputDeviceService InputDeviceService { get; private set; }
        public IOptionsService OptionsService { get; private set; }

        public void Setup()
        {
            AudioManagerService = GetComponentInChildren<IAudioManager>();
            AudioManagerService.Setup();

            MusicPlayerService = GetComponentInChildren<IMusicPlayer>();
            MusicPlayerService.Setup();

            SceneLoaderService = GetComponentInChildren<ISceneLoader>();
            SceneLoaderService.Setup();

            ApiClientService = GetComponentInChildren<IApiClient>();
            ApiClientService.Setup();

            DatabaseService = GetComponentInChildren<IDatabaseService>();
            DatabaseService.Setup();

            InputDeviceService = GetComponentInChildren<IInputDeviceService>();
            InputDeviceService.Setup();

            OptionsService = GetComponentInChildren<IOptionsService>();
            OptionsService.Setup();
        }
    }
}
