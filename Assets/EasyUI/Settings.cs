using UnityEngine;

namespace EasyUI
{
    [CreateAssetMenu(fileName = "Settings", menuName = "EasyUI/Settings")]
    public class Settings : ScriptableObject
    {
        public static Settings instance => Resources.Load<Settings>("Settings");
        
        [SerializeField]
        Color _dialogBkgColor;

        public Color dialogBkgColor => _dialogBkgColor;
    }
}