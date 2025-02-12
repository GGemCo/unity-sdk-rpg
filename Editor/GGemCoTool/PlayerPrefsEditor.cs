using GGemCo.Scripts.FgPlayerPrefs;
using UnityEditor;
using UnityEngine;

namespace GGemCo.Editor.GGemCoTool
{
    public class PlayerPrefsEditor : EditorWindow
    {
        [MenuItem("GGemCoTool/PlayerPrefs Editor", true)]
        public static void ShowWindow()
        {
            GetWindow<PlayerPrefsEditor>("PlayerPrefs Editor");
        }

        private Vector2 scrollPosition;

        private void OnGUI()
        {
            GUILayout.Label("PlayerPrefs Editor", EditorStyles.boldLabel);

            if (GUILayout.Button("Delete All PlayerPrefs"))
            {
                if (EditorUtility.DisplayDialog("Delete All PlayerPrefs",
                        "Are you sure you want to delete all PlayerPrefs?", "Yes", "No"))
                {
                    PlayerPrefs.DeleteAll();
                    EditorUtility.DisplayDialog("PlayerPrefs Deleted", "All PlayerPrefs have been deleted.", "OK");
                }
            }

            GUILayout.Space(10);

            GUILayout.Label("Stored PlayerPrefs:", EditorStyles.boldLabel);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

            ShowAllPlayerPrefs(); // PlayerPrefs 값을 출력

            GUILayout.EndScrollView();

            if (GUILayout.Button("Refresh"))
            {
                Repaint();
            }
        }

        private void ShowAllPlayerPrefs()
        {
            // PlayerPrefs에 저장된 데이터의 타입에 따라 값을 출력하는 예시

            string[] keys = new string[] { SoundSettings.bgmVolumeKey, SoundSettings.sfxVolumeKey};

            foreach (string key in keys)
            {
                if (PlayerPrefs.HasKey(key))
                {
                    // 데이터의 타입을 추측하여 적절한 메서드로 값을 가져옵니다.
                    if (IsInt(key))
                    {
                        int value = PlayerPrefs.GetInt(key);
                        GUILayout.Label($"{key} (int): {value}");
                    }
                    else if (IsFloat(key))
                    {
                        float value = PlayerPrefs.GetFloat(key);
                        GUILayout.Label($"{key} (float): {value}");
                    }
                    else
                    {
                        string value = PlayerPrefs.GetString(key);
                        GUILayout.Label($"{key} (string): {value}");
                    }
                }
                else
                {
                    GUILayout.Label($"{key}: (no value)");
                }
            }
        }

        // 키에 대응하는 값이 int 타입인지 확인하는 메서드
        private bool IsInt(string key)
        {
            // 특정 값의 범위를 예상하여 처리 (기본적으로 불가능)
            // 예시로 간단하게 string을 int로 변환 시도
            if (key.Contains("Int"))
            {
                return true;
            }
            return false;
        }

        // 키에 대응하는 값이 float 타입인지 확인하는 메서드
        private bool IsFloat(string key)
        {
            if (key.Contains("Float"))
            {
                return true;
            }
            return false;
        }
    }
}
