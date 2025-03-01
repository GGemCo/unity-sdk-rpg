using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GGemCo.Scripts.SaveData;

namespace GGemCo.Editor.GGemCoTool
{
    public class PlayerPrefsEditor : EditorWindow
    {
        private const string Title = "PlayerPrefs 데이터 관리";
        
        [MenuItem("GGemCoTool/"+Title)]
        public static void ShowWindow()
        {
            GetWindow<PlayerPrefsEditor>(Title);
        }

        private Vector2 scrollPosition;
        private readonly Dictionary<string, string> playerPrefsData = new Dictionary<string, string>();

        private void OnGUI()
        {
            GUILayout.Space(10);

            if (GUILayout.Button("모든 데이터 삭제하기"))
            {
                if (EditorUtility.DisplayDialog("모든 데이터 삭제하기",
                        "정말 삭제하시겠습니까?", "네", "아니오"))
                {
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();
                    LoadPlayerPrefs();
                    EditorUtility.DisplayDialog("모든 데이터 삭제하기", "모든 데이터가 삭제되었습니다.", "OK");
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button("새로고침"))
            {
                LoadPlayerPrefs();
                Repaint();
            }

            GUILayout.Space(10);

            GUILayout.Label("PlayerPrefs 데이터:", EditorStyles.boldLabel);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

            // 테이블 형식으로 PlayerPrefs 데이터 표시
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("Key", EditorStyles.boldLabel, GUILayout.Width(250));
            GUILayout.Label("Value", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Type", EditorStyles.boldLabel, GUILayout.Width(100));
            GUILayout.Label("Actions", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.EndHorizontal();

            // **컬렉션이 수정되는 문제 해결**
            List<string> keysToDelete = new List<string>();
            foreach (var kvp in playerPrefsData)
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(kvp.Key, GUILayout.Width(250));
                GUILayout.Label(kvp.Value, GUILayout.Width(150));
                GUILayout.Label(DetectType(kvp.Key), GUILayout.Width(100));

                if (GUILayout.Button("삭제", GUILayout.Width(80)))
                {
                    keysToDelete.Add(kvp.Key); // 리스트에 삭제할 키 추가
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            
            // **루프 종료 후 삭제 처리**
            if (keysToDelete.Count > 0)
            {
                foreach (string key in keysToDelete)
                {
                    PlayerPrefs.DeleteKey(key);
                }
                PlayerPrefs.Save();
                LoadPlayerPrefs();
                Repaint();
            }
        }

        private void OnEnable()
        {
            LoadPlayerPrefs();
        }

        private void LoadPlayerPrefs()
        {
            playerPrefsData.Clear();

            // PlayerPrefs 키 목록을 알 수 있는 방법이 없기 때문에, 예상할 수 있는 키들을 미리 등록해야 함
            string[] keys = GetAllPossibleKeys();

            foreach (string key in keys)
            {
                if (PlayerPrefs.HasKey(key))
                {
                    playerPrefsData[key] = GetValueAsString(key);
                }
            }
        }

        private string[] GetAllPossibleKeys()
        {
            // EditorPrefs를 사용하여 키 목록을 유지하는 방법 (이전에 저장된 키를 추적 가능)
            List<string> keys = new List<string>();

            if (EditorPrefs.HasKey("PlayerPrefs_Keys"))
            {
                string savedKeys = EditorPrefs.GetString("PlayerPrefs_Keys");
                keys.AddRange(savedKeys.Split(','));
            }

            // 여기에 미리 알고 있는 키 추가 가능 (예제)
            keys.Add(PlayerData.PlayerPrefsKeyLevel);
            keys.Add(PlayerData.PlayerPrefsKeyExp);

            return keys.Distinct().ToArray();
        }

        private string GetValueAsString(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetString(key);
            }
            return "N/A";
        }

        private string DetectType(string key)
        {
            if (IsInt(key)) return "int";
            if (IsFloat(key)) return "float";
            return "string";
        }

        private bool IsInt(string key)
        {
            int value;
            return int.TryParse(PlayerPrefs.GetString(key, ""), out value);
        }

        private bool IsFloat(string key)
        {
            float value;
            return float.TryParse(PlayerPrefs.GetString(key, ""), out value);
        }
    }
}
