// GameManagers is an intentionally thin, game-agnostic DontDestroyOnLoad singleton.
// It owns the ServiceLocator (audio, scene loading, etc.) and nothing else.
//
// Game-specific singletons (e.g. SessionService) must NOT be added here — doing so
// would couple the framework to game code and break reusability across projects.
// Instead, attach them as sibling MonoBehaviours on the same prefab: they inherit
// DontDestroyOnLoad from this root object and manage their own Instance references.
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
