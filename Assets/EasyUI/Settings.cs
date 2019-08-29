#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace EasyUI
{
    [CreateAssetMenu(fileName = "Settings", menuName = "EasyUI/Settings")]
    public class Settings : ScriptableObject
    {
        const string FILE_NAME = "EasyUISettings";
        static Settings _instance;
        public static Settings instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                
                _instance = Resources.Load<Settings>(FILE_NAME);
#if UNITY_EDITOR
                if (_instance != null)
                {
                    return _instance;
                }

                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                _instance = CreateInstance<Settings>();
                AssetDatabase.CreateAsset(_instance, $"Assets/Resources/{FILE_NAME}.asset");
#endif

                return _instance;
            }
        }

        [SerializeField]
        Color _dialogBkgColor = new Color(0, 0, 0, 0.5f);

        [SerializeField, Tooltip("触发Animator enter动画的trigger名称")]
        string _enterTriggerName = "panel_enter";
        
        [SerializeField, Tooltip("触发Animator exit动画的trigger名称")]
        string _exitTriggerName = "panel_exit";

        public Color dialogBkgColor => _dialogBkgColor;
        public string animatorEnterTriggerName => _enterTriggerName;
        public string animatorExitTriggerName => _exitTriggerName;
    }
}