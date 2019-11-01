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
        SerializedProperty _contentHeadPadding;
        SerializedProperty _contentFootPadding;

        protected override void OnEnable()
        {
            base.OnEnable();
            _target = target as RecyclerScrollRect;
            _itemPrefab = _target.EditorGetItemPrefabSp(serializedObject);
            _header = _target.EditorGetHeaderSp(serializedObject);
            _footer = _target.EditorGetFooterSp(serializedObject);
            _spacing = _target.EditorGetSpacingSp(serializedObject);
            _loadOnStart = _target.EditorGetLoadOnStartSp(serializedObject);
            _contentHeadPadding = _target.EditorGetContentHeadPaddingSp(serializedObject);
            _contentFootPadding = _target.EditorGetContentFootPaddingSp(serializedObject);
            
            UpdateHeaderAndFooterAnchorSetting(); 
        }

        public override void OnInspectorGUI()
        {
            bool vertical = _target.vertical;
            bool horizontal = _target.horizontal;
            float contentHeadPadding = _contentHeadPadding.floatValue;
            float contentFootPadding = _contentFootPadding.floatValue;
            
            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                base.OnInspectorGUI();
                EditorGUILayout.PropertyField(_itemPrefab);
                EditorGUILayout.PropertyField(_header);
                EditorGUILayout.PropertyField(_footer);
                EditorGUILayout.PropertyField(_contentHeadPadding);
                EditorGUILayout.PropertyField(_contentFootPadding);
                EditorGUILayout.PropertyField(_spacing);
                EditorGUILayout.PropertyField(_loadOnStart);

                if (!cc.changed) return;

                RectTransform content = _target.content;
                if (content == null)
                {
                    return;
                }

                if (vertical != _target.vertical && _target.vertical)
                {
                    float size = content.rect.width;
                    content.anchorMin = new Vector2(0, 1);
                    content.anchorMax = new Vector2(1, 1);
                    content.offsetMin = Vector2.zero;
                    content.offsetMax = Vector2.zero;
                    content.sizeDelta = new Vector2(0, size);
                    _target.horizontal = false;
                    
                    UpdateHeaderAndFooterAnchorSetting(true);
                    UpdateHeaderAndFooterAnchorPos();
                }
                else if (horizontal != _target.horizontal && _target.horizontal)
                {
                    float size = content.rect.height;
                    content.anchorMin = new Vector2(0, 0);
                    content.anchorMax = new Vector2(0, 1);
                    content.offsetMin = Vector2.zero;
                    content.offsetMax = Vector2.zero;
                    content.sizeDelta = new Vector2(size, 0);
                    _target.vertical = false;
                    
                    UpdateHeaderAndFooterAnchorSetting(true);
                    UpdateHeaderAndFooterAnchorPos();
                }

                if (contentHeadPadding != _contentHeadPadding.floatValue)
                {
                    UpdateHeaderAnchorPos();
                }

                if (contentFootPadding != _contentFootPadding.floatValue)
                {
                    UpdateFooterAnchorPos();
                }
                    
                serializedObject.ApplyModifiedProperties();
            }
        }

        void UpdateHeaderAndFooterAnchorSetting(bool adjustSize = false)
        {
            if (_target.content == null)
            {
                return;
            }

            Vector2 size = Vector2.zero;
            if (_header.objectReferenceValue != null)
            {
                var headerTrans = _header.objectReferenceValue as RectTransform;
                if (adjustSize)
                {
                    size = headerTrans.rect.size;
                }
                
                if (_target.vertical)
                {
                    headerTrans.SetStretchAnchorTop();
                    if (adjustSize)
                    {
                        headerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.x);
                    }
                }
                else
                {
                    headerTrans.SetStretchAnchorLeft();
                    if (adjustSize)
                    {
                        headerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.y);
                    }
                }

            }

            if (_footer.objectReferenceValue != null)
            {
                var footerTrans = _footer.objectReferenceValue as RectTransform;
                if (adjustSize)
                {
                    size = footerTrans.rect.size;
                }
                
                if (_target.vertical)
                {
                    footerTrans.SetStretchAnchorBottom();
                    if (adjustSize)
                    {
                        footerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.x);
                    }
                }
                else
                {
                    footerTrans.SetStretchAnchorRight();
                    if (adjustSize)
                    {
                        footerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.y);
                    }
                }
            }
        }

        void UpdateHeaderAndFooterAnchorPos()
        {
            UpdateHeaderAnchorPos();
            UpdateFooterAnchorPos();
        }

        void UpdateHeaderAnchorPos()
        {
            UpdateHeaderOrFooterAnchorPos(_header, _contentHeadPadding, -1);
        }

        void UpdateFooterAnchorPos()
        {
            UpdateHeaderOrFooterAnchorPos(_footer, _contentFootPadding, 1);
        }

        void UpdateHeaderOrFooterAnchorPos(SerializedProperty sp, SerializedProperty padding, int factor)
        {
            if (sp.objectReferenceValue == null)
            {
                return;
            }

            Undo.RecordObject(sp.objectReferenceValue, "UpdateAnchorPos");
            var rectTrans = sp.objectReferenceValue as RectTransform;
            var pos = rectTrans.anchoredPosition;
            if (_target.vertical)
            {
                pos.x = 0;
                pos.y = factor * padding.floatValue;
            }
            else
            {
                pos.x = -factor * padding.floatValue;
                pos.y = 0;
            }
            rectTrans.anchoredPosition = pos;
        }
    }
}