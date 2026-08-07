using System.Collections.Generic;
using UnityEngine;

namespace SteelHorse.Framework.Tags
{
    [CreateAssetMenu(menuName = "Steel Horse/Tags/Tag Database", fileName = "Tag Database")]
    public class TagDatabase : ScriptableObject
    {
        public IReadOnlyList<TagDefinition> Tags { get { return _tags; } }

        [SerializeField] private List<TagDefinition> _tags = new List<TagDefinition>();

        public bool TryGetTag(string key, out TagDefinition tag)
        {
            tag = _tags.Find(t => t.Key == key);
            return tag != null;
        }

#if UNITY_EDITOR
        // Unity's list "+" button duplicates the previous element's values (including its key),
        // so warn early rather than let two tags silently share the same identifier.
        private void OnValidate()
        {
            HashSet<string> seenKeys = new HashSet<string>();
            foreach (TagDefinition tag in _tags)
            {
                if (string.IsNullOrEmpty(tag.Key))
                    continue;

                if (!seenKeys.Add(tag.Key))
                    Debug.LogWarning($"[TagDatabase] Duplicate tag key '{tag.Key}' in '{name}' — keys must be unique.", this);
            }
        }
#endif
    }
}
