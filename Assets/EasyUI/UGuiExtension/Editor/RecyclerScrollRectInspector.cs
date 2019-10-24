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

        protected override void OnEnable()
        {
            base.OnEnable();
            _target = target as RecyclerScrollRect;
            _itemPrefab = _target.EditorGetItemPrefabSp(serializedObject);
            _header = _target.EditorGetHeaderSp(serializedObject);
            _footer = _target.EditorGetFooterSp(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            bool vertical = _target.vertical;
            bool horizontal = _target.horizontal;
            
            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                base.OnInspectorGUI();
                if (cc.changed)
                {
                    if (vertical != _target.vertical && _target.vertical)
                    {
                        _target.content.anchorMin = new Vector2(0, 1);
                        _target.content.anchorMax = new Vector2(1, 1);
                        _target.horizontal = false;
                    }
                    else if (horizontal != _target.horizontal && _target.horizontal)
                    {
                        _target.content.anchorMin = new Vector2(0, 0);     
                        _target.content.anchorMax = new Vector2(0, 1);
                        _target.vertical = false;
                    }
                    
                    _target.content.offsetMax = Vector2.zero;
                    _target.content.offsetMin = Vector2.zero;
                }
            }
            EditorGUILayout.PropertyField(_itemPrefab);
            EditorGUILayout.PropertyField(_header);
            EditorGUILayout.PropertyField(_footer);
        }
    }
}