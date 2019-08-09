using UnityEditor;

namespace EasyUI.Editor
{
    public class SettingsWindow : EditorWindow
    {
        Settings _settings;

        [MenuItem("Window/EasyUI/Settings")]
        static void Init()
        {
            var window = GetWindow(typeof(SettingsWindow));
            window.Show();
        }

        void OnEnable()
        {
            _settings = Settings.instance;
        }

        void OnGUI()
        {
            if (_settings == null)
            {
                return;
            }

            var editor = UnityEditor.Editor.CreateEditor(_settings);
            editor.OnInspectorGUI();
        }
    }
}