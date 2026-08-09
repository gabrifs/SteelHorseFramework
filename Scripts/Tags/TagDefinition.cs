using UnityEngine;
using UnityEngine.Localization;

namespace SteelHorse.Framework.Tags
{
    [CreateAssetMenu(menuName = "Steel Horse/Tags/Tag Definition", fileName = "New Tag Definition")]
    public class TagDefinition : ScriptableObject
    {
        public string Key { get { return _key; } }
        public LocalizedString DisplayName { get { return _displayName; } }
        public Color Color { get { return _color; } }

        [Tooltip("Stable identifier used to reference this tag in code and other assets. Must be unique.")]
        [SerializeField] private string _key;
        [SerializeField] private LocalizedString _displayName;
        [SerializeField] private Color _color = Color.white;
    }
}
