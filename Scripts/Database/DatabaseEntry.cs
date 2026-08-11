using UnityEngine;

namespace SteelHorse.Framework.Database
{
    public abstract class DatabaseEntry : ScriptableObject
    {
        public string Key { get { return _key; } }

        [Tooltip("Stable identifier used to reference this entry in code and other assets. Must be unique.")]
        [SerializeField] private string _key;
    }
}
