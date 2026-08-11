using System.Collections.Generic;
using UnityEngine;

namespace SteelHorse.Framework.Tags
{
    [CreateAssetMenu(menuName = "Steel Horse/Tags/Tag Database", fileName = "Tag Database")]
    public class TagDatabase : SteelHorse.Framework.Database.KeyedDatabase<TagDefinition>
    {
        public IReadOnlyList<TagDefinition> Tags { get { return Entries; } }

        public bool TryGetTag(string key, out TagDefinition tag)
        {
            return TryGetByKey(key, out tag);
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
