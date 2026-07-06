using TMPro;
using UnityEngine;

namespace SteelHorse.Framework.UI
{
    public class VersionLabel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        private void Awake() => _label.text = Application.version;
    }
}
