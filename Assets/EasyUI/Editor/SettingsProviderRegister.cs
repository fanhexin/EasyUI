using UnityEditor;

namespace EasyUI.Editor
{
    public static class SettingsProviderRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Project/EasyUI", SettingsScope.Project)
            {
                label = "EasyUI",
                guiHandler = context =>
                {
                    var editor = UnityEditor.Editor.CreateEditor(Settings.instance);
                    editor.OnInspectorGUI();
                }
            };

            return provider;
        }
    }
}