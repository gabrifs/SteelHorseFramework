// Simple script that allows quick access to where the standard path to the save files live

using UnityEngine;
using UnityEditor;

namespace SteelHorse.Framework.Editor
{
    public class OpenPersistendData : MonoBehaviour
    {
        [MenuItem("Tools/Steel Horse/Open Persistent Data Path")]
        public static void OpenPath()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
    }
}