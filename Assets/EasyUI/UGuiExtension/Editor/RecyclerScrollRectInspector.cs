using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace EasyUI.UGuiExtension.Editor
{
    [CustomEditor(typeof(RecyclerScrollRect))]
    public class RecyclerScrollRectInspector : ScrollRectEditor
    {
        RecyclerScrollRect _target;
        SerializedProperty _itemPrefab;
        SerializedProperty _header;
        SerializedProperty _footer;
        SerializedProperty _spacing;
        SerializedProperty _loadOnStart;

        protected override void OnEnable()
        {
            base.OnEnable();
            _target = target as RecyclerScrollRect;
            _itemPrefab = _target.EditorGetItemPrefabSp(serializedObject);
            _header = _target.EditorGetHeaderSp(serializedObject);
            _footer = _target.EditorGetFooterSp(serializedObject);
            _spacing = _target.EditorGetSpacingSp(serializedObject);
            _loadOnStart = _target.EditorGetLoadOnStartSp(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            bool vertical = _target.vertical;
            bool horizontal = _target.horizontal;
            
            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                base.OnInspectorGUI();
                EditorGUILayout.PropertyField(_itemPrefab);
                EditorGUILayout.PropertyField(_header);
                EditorGUILayout.PropertyField(_footer);
                EditorGUILayout.PropertyField(_spacing);
                EditorGUILayout.PropertyField(_loadOnStart);

                if (!cc.changed) return;

                RectTransform content = _target.content;
                if (content != null)
                {
                    if (vertical != _target.vertical && _target.vertical)
                    {
                        content.anchorMin = new Vector2(0, 1);
                        content.anchorMax = new Vector2(1, 1);
                        _target.horizontal = false;
                    }
                    else if (horizontal != _target.horizontal && _target.horizontal)
                    {
                        content.anchorMin = new Vector2(0, 0);     
                        content.anchorMax = new Vector2(0, 1);
                        _target.vertical = false;
                    }
                    
                    content.offsetMax = Vector2.zero;
                    content.offsetMin = Vector2.zero;
                }
                    
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}