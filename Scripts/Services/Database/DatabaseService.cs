using UnityEngine;

namespace SteelHorse.Framework.Services.Database
{
    public class DatabaseService : MonoBehaviour, IDatabaseService
    {
        [SerializeField] private SteelHorse.Framework.Database.GameDatabase _gameDatabase;

        public void Setup() { }

        public TDatabase Get<TDatabase>() where TDatabase : SteelHorse.Framework.Database.Database
        {
            return _gameDatabase.Get<TDatabase>();
        }

        public bool TryGet<TDatabase>(out TDatabase database) where TDatabase : SteelHorse.Framework.Database.Database
        {
            return _gameDatabase.TryGet(out database);
        }
    }
}
