using System.Collections.Generic;
using UnityEngine;

namespace SteelHorse.Framework.Database
{
    [CreateAssetMenu(menuName = "Steel Horse/Database/Game Database", fileName = "Game Database")]
    public class GameDatabase : ScriptableObject
    {
        [SerializeField] private List<Database> _databases = new List<Database>();

        public bool TryGet<TDatabase>(out TDatabase database) where TDatabase : Database
        {
            foreach (Database candidate in _databases)
            {
                if (candidate is TDatabase match)
                {
                    database = match;
                    return true;
                }
            }

            database = null;
            return false;
        }

        public TDatabase Get<TDatabase>() where TDatabase : Database
        {
            TryGet(out TDatabase database);
            return database;
        }
    }
}
