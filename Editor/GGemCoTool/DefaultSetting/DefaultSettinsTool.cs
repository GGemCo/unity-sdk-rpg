using UnityEditor;

namespace GGemCo.Editor.GGemCoTool.DefaultSetting
{
    public class DefaultSettinsTool : EditorWindow
    {
        private readonly SettingTags settingTags = new SettingTags();
        private readonly SettingManagers settingManagers = new SettingManagers();
        
        [MenuItem("GGemCoTool/기본 셋팅하기")]
        public static void ShowWindow()
        {
            GetWindow<DefaultSettinsTool>("기본 셋팅하기");
        }

        private void OnGUI()
        {
            settingTags.OnGUI();
            EditorGUILayout.Space(10);
            
            // todo #define 을 추가할 수 있다. 코드가 복잡해 지지 않을가?
            // settingManagers.OnGUI();
        }
    }
}