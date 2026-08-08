using System.Collections.Generic;
using UnityEngine;

namespace SteelHorse.Framework.Tags
{
    [CreateAssetMenu(menuName = "Steel Horse/Tags/Tag Database", fileName = "Tag Database")]
    public class TagDatabase : SteelHorse.Framework.Database.Database<TagDefinition>
    {
        public IReadOnlyList<TagDefinition> Tags { get { return Entries; } }

        public bool TryGetTag(string key, out TagDefinition tag)
        {
            tag = _entries.Find(t => t != null && t.Key == key);
            return tag != null;
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

#if UNITY_EDITOR
        // Unity's list "+" button duplicates the previous element's reference, so warn early rather
        // than let the same Tag Definition asset (or two with a hand-typed matching key) end up
        // twice in the list.
        private void OnValidate()
        {
            HashSet<string> seenKeys = new HashSet<string>();
            foreach (TagDefinition tag in _entries)
            {
                if (tag == null || string.IsNullOrEmpty(tag.Key))
                    continue;

                if (!seenKeys.Add(tag.Key))
                    Debug.LogWarning($"[TagDatabase] Duplicate tag key '{tag.Key}' in '{name}' — keys must be unique.", this);
            }
        }
#endif
    }
}
