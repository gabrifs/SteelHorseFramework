using UnityEngine;

namespace SteelHorse.Framework.Services.Networking
{
    [CreateAssetMenu(menuName = "Steel Horse/Networking/Api Config", fileName = "New Api Config")]
    public class ApiConfig : ScriptableObject
    {
        public string BaseUrl { get { return _baseUrl; } }

        [SerializeField] private string _baseUrl = "https://";
    }
}
