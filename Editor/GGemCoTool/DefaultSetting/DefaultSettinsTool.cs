using UnityEditor;

namespace GGemCo.Editor.GGemCoTool.DefaultSetting
{
    public class DefaultSettinsTool : EditorWindow
    {
        private readonly SettingTags settingTags = new SettingTags();
        private readonly SettingSortingLayers settingSortingLayers = new SettingSortingLayers();
        private readonly SettingLayers settingLayers = new SettingLayers();

        [MenuItem("GGemCoTool/기본 셋팅하기")]
        public static void ShowWindow()
        {
            GetWindow<DefaultSettinsTool>("기본 셋팅하기");
        }

        private void OnGUI()
        {
            settingTags.OnGUI();
            EditorGUILayout.Space(10);
            settingSortingLayers.OnGUI();
            EditorGUILayout.Space(10);
            settingLayers.OnGUI();
        }
    }
}