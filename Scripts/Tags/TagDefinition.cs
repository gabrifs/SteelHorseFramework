using SteelHorse.Framework.Database;
using UnityEngine;
using UnityEngine.Localization;

namespace SteelHorse.Framework.Tags
{
    [CreateAssetMenu(menuName = "Steel Horse/Tags/Tag Definition", fileName = "New Tag Definition")]
    public class TagDefinition : DatabaseEntry
    {
        public LocalizedString DisplayName { get { return _displayName; } }
        public Color Color { get { return _color; } }

        [SerializeField] private LocalizedString _displayName;
        [SerializeField] private Color _color = Color.white;
    }
}
