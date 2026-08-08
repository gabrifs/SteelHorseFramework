using System.Collections.Generic;
using SteelHorse.Framework.Tags;
using UnityEditor;
using UnityEngine;

namespace SteelHorse.Framework.Editor
{
    [CustomPropertyDrawer(typeof(TagReference))]
    public class TagReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty keyProp = property.FindPropertyRelative("_key");
            TagDatabase database = TagDatabaseLocator.Find();

            EditorGUI.BeginProperty(position, label, property);

            if (database == null)
            {
                EditorGUI.LabelField(position, label.text, "No active Tag Database — select one and click 'Set as Active Tag Database'");
                EditorGUI.EndProperty();
                return;
            }

            IReadOnlyList<TagDefinition> tags = database.Tags;
            string[] options = new string[tags.Count];
            int selectedIndex = -1;

            for (int i = 0; i < tags.Count; i++)
            {
                options[i] = tags[i] == null || string.IsNullOrEmpty(tags[i].Key) ? "(unassigned)" : tags[i].Key;
                if (tags[i] != null && tags[i].Key == keyProp.stringValue)
                    selectedIndex = i;
            }

            int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, options);
            if (newIndex >= 0 && newIndex != selectedIndex && tags[newIndex] != null)
                keyProp.stringValue = tags[newIndex].Key;

            EditorGUI.EndProperty();
        }
    }
}
