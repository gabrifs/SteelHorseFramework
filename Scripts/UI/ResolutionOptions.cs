using UnityEngine;

namespace SteelHorse.Framework.UI
{
    [CreateAssetMenu(menuName = "Steel Horse/UI/Resolution Options", fileName = "New Resolution Options")]
    public class ResolutionOptions : ScriptableObject
    {
        public ResolutionSetting[] Resolutions { get { return _resolutions; } }

        [SerializeField] private ResolutionSetting[] _resolutions;
    }
}
