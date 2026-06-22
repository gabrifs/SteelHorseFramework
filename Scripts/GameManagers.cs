using UnityEngine;
using SteelHorse.Framework.Services;

namespace SteelHorse.Framework
{
    public class GameManagers : MonoBehaviour
    {
        public static GameManagers Instance { get; private set; }

        public ServiceLocator Services { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Services = GetComponentInChildren<ServiceLocator>();
            Services.Setup();
        }
    }
}
