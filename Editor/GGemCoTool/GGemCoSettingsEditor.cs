using GGemCo.Scripts.Configs;
using UnityEditor;
using UnityEngine;

namespace GGemCo.Editor.GGemCoTool
{
    public class GGemCoSettingsEditor : EditorWindow
    {
        private const string SettingsPath = "Assets/GGemCo/GGemCoSettings.asset";

        [MenuItem("GGemCoTool/설정 ScriptableObject 만들기", false, 1)]
        public static void CreateSettings()
        {
            // 기존 설정 파일이 존재하면 선택
            GGemCoSettings existingSettings = AssetDatabase.LoadAssetAtPath<GGemCoSettings>(SettingsPath);
            if (existingSettings != null)
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = existingSettings;
                Debug.Log("GGemCoSettings 설정이 이미 존재합니다.");
                
                // 기존 설정 파일에서 define 심볼 업데이트
                UpdateScriptingDefineSymbols(existingSettings.useSpine2d);
                return;
            }

            // 새 ScriptableObject 생성
            GGemCoSettings newSettings = ScriptableObject.CreateInstance<GGemCoSettings>();

            // ScriptableObject를 프로젝트에 저장
            AssetDatabase.CreateAsset(newSettings, SettingsPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 생성한 설정 파일 선택
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newSettings;

            Debug.Log("GGemCoSettings 설정이 생성되었습니다.");

            // 새 설정 파일이 생성될 때, 초기 define 심볼 업데이트
            UpdateScriptingDefineSymbols(newSettings.useSpine2d);
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
