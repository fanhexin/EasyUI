using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EasyUI.Editor
{
    [CustomPropertyDrawer(typeof(PanelNameAttribute))]
    public class TransitionDrawer : PropertyDrawer
    {
        static string[] _panelNames;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_panelNames == null)
            {
                _panelNames = EditorUtil.FindAssets<GameObject>()
                    .Where(x => x.GetComponent<UIPanel>() != null)
                    .Select(x => x.name)
                    .ToArray();
            }
            
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginChangeCheck();
            string panelName = property.stringValue;
            int index = string.IsNullOrEmpty(panelName)? 0 : Array.IndexOf(_panelNames, panelName);
            index = EditorGUI.Popup(position, index, _panelNames);
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = _panelNames[index];
                property.serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}