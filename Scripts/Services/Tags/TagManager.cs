using SteelHorse.Framework.Tags;
using UnityEngine;

namespace SteelHorse.Framework.Services.Tags
{
    public class TagManager : MonoBehaviour, ITagManager
    {
        [SerializeField] private TagDatabase _database;

        public void Setup() { }

        public bool TryGetTag(string key, out TagDefinition tag)
        {
            return _database.TryGetTag(key, out tag);
        }

        public bool TryGetTag(TagReference reference, out TagDefinition tag)
        {
            if (reference == null)
            {
                tag = null;
                return false;
            }

            return TryGetTag(reference.Key, out tag);
        }
    }
}
