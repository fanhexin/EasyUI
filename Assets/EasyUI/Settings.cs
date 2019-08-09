using UnityEngine;

namespace EasyUI
{
    [CreateAssetMenu(fileName = "Settings", menuName = "EasyUI/Settings")]
    public class Settings : ScriptableObject
    {
        static Settings _instance;
        public static Settings instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<Settings>("Settings");
                }

                return _instance;
            }
        }

        [SerializeField]
        Color _dialogBkgColor;

        [SerializeField, Tooltip("触发Animator enter动画的trigger名称")]
        string _enterTriggerName = "panel_enter";
        
        [SerializeField, Tooltip("触发Animator exit动画的trigger名称")]
        string _exitTriggerName = "panel_exit";

        public Color dialogBkgColor => _dialogBkgColor;
        public string animatorEnterTriggerName => _enterTriggerName;
        public string animatorExitTriggerName => _exitTriggerName;
    }
}