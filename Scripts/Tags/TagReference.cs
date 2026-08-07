using System;
using UnityEngine;

namespace SteelHorse.Framework.Tags
{
    // Field type for assigning tags on other assets/components — drawn as a dropdown
    // by TagReferenceDrawer, populated from the project's single TagDatabase.
    [Serializable]
    public class TagReference
    {
        public string Key { get { return _key; } }

        [SerializeField] private string _key;
    }
}
