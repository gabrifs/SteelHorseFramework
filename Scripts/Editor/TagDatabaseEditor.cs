using SteelHorse.Framework.Tags;
using UnityEditor;
using UnityEngine;

namespace SteelHorse.Framework.Editor
{
    [CustomEditor(typeof(TagDatabase))]
    public class TagDatabaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TagDatabase database = (TagDatabase)target;

            if (TagDatabaseLocator.IsActive(database))
            {
                EditorGUILayout.HelpBox("This is the active Tag Database — it's the one shown in tag dropdowns.", MessageType.Info);
            }
            else if (GUILayout.Button("Set as Active Tag Database"))
            {
                TagDatabaseLocator.SetActive(database);
            }

            EditorGUILayout.Space();
            DrawDefaultInspector();
        }
    }
}
