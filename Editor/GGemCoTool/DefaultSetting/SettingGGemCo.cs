using GGemCo.Editor.GGemCoTool.Utils;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.ScriptableSettings;
using UnityEditor;
using UnityEngine;

namespace GGemCo.Editor.GGemCoTool.DefaultSetting
{
    public class SettingGGemCo
    {
        private readonly string title = "설정 ScriptableObject 추가하기";
        private const string SettingsPath = "Assets/GGemCo/GGemCoSettings.asset";

        public void OnGUI()
        {
            Common.OnGUITitle(title);

            if (GUILayout.Button(title))
            {
                CreateSettings();
            }
        }

        private static void CreateSettings()
        {
            // 기존 설정 파일이 존재하면 선택
            GGemCoSettings existing = AssetDatabase.LoadAssetAtPath<GGemCoSettings>(SettingsPath);
            if (existing != null)
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = existing;
                Debug.Log("GGemCoSettings 설정이 이미 존재합니다.");
                
                // 기존 설정 파일에서 define 심볼 업데이트
                UpdateScriptingDefineSymbols(existing.useSpine2d);
                return;
            }

            // 새 ScriptableObject 생성
            GGemCoSettings @new = ScriptableObject.CreateInstance<GGemCoSettings>();

            // ScriptableObject를 프로젝트에 저장
            AssetDatabase.CreateAsset(@new, SettingsPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 생성한 설정 파일 선택
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = @new;

            Debug.Log("GGemCoSettings 설정이 생성되었습니다.");

            // 새 설정 파일이 생성될 때, 초기 define 심볼 업데이트
            UpdateScriptingDefineSymbols(@new.useSpine2d);
        }

        private static void UpdateScriptingDefineSymbols(bool enable)
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);

            if (enable)
            {
                if (!symbols.Contains(ConfigDefine.SpineDefineSymbol))
                {
                    symbols += $";{ConfigDefine.SpineDefineSymbol}";
                }
            }
            else
            {
                if (symbols.Contains(ConfigDefine.SpineDefineSymbol))
                {
                    symbols = symbols.Replace(ConfigDefine.SpineDefineSymbol, "").Replace(";;", ";").Trim(';');
                }
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbols);
            Debug.Log($"Scripting Define Symbols updated: {symbols}");
        }
    }
}
