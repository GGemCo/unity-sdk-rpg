using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GGemCo.Editor.GGemCoTool.DefaultSetting
{
    public class SettingManagers
    {
        private readonly string objectNameManager = "Manager";
        private readonly string sceneNameGame = "Game";
        private readonly string pattern = @"\(([^)]+)\)";
        private readonly string title = "매니저 추가하기";
        // 괄호() 안에 있는 영문으로 오브젝트 이름을 지정하고, cs 파일을 찾는다.
        private readonly string[] essentialManagerNames = new[] { "맵 매니저(MapManager)", "UI 윈도우 매니저(UIWindowManager)", "데이터 저장 매니저(SaveDataManager)" };
        private readonly string[] optionManagerNames = new[] { "퀘스트 매니저(QuestManager)", "팝업 매니저(PopupManager)", "계산 매니저(CalculateManager)" };
        private bool[] addManagers;
        
        public void OnGUI()
        {
            Common.OnGUITitle(title);
            GUILayout.Label("필수 매니저(자동으로 추가됩니다.)", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            foreach (var t in essentialManagerNames)
            {
                EditorGUILayout.LabelField($"- {t}", EditorStyles.label);
            }
            --EditorGUI.indentLevel;
            
            EditorGUILayout.Space(10);
            
            GUILayout.Label("추가 매니저", EditorStyles.boldLabel);
            addManagers ??= new bool[optionManagerNames.Length];
            for (int i = 0; i < optionManagerNames.Length; ++i)
            {
                addManagers[i] = EditorGUILayout.ToggleLeft($"{optionManagerNames[i]}", addManagers[i]);
            }
            // EditorGUILayout.HelpBox ("ProgressBar doesn't have height", MessageType.Info);
            if (GUILayout.Button(title))
            {
                AddManagers();
            }
        }

        private void AddManagers()
        {
            // Get the currently active scene
            Scene activeScene = SceneManager.GetActiveScene();

            // Check if the scene's name is "Game"
            if (activeScene.name != sceneNameGame)
            {
                EditorUtility.DisplayDialog(title, $"{sceneNameGame} 씬에서 추가해주세요.", "OK");
                return;
            }
            
            // Check if "Manager" object exists
            GameObject managerObject = GameObject.Find(objectNameManager);
            if (managerObject == null)
            {
                // If not, create the "Manager" empty object
                managerObject = new GameObject(objectNameManager);
            }

            string existManager = "";
            foreach (var name in essentialManagerNames)
            {
                Match match = Regex.Match(name, pattern);
                if (match.Success != true) continue;
                string objectName = match.Groups[1].Value; // 괄호 안의 내용을 출력
                // Check if "SaveDataManager" already exists as a child of "Manager"
                Transform managerTransform = managerObject.transform.Find(objectName);
                
                
                if (managerTransform == null)
                {
                    // If not, create the "SaveDataManager" empty object and make it a child of "Manager"
                    GameObject newManagerObject = new GameObject(objectName);
                    newManagerObject.transform.SetParent(managerObject.transform);

                    // 현재 어셈블리에서 클래스 이름을 찾아 해당 타입을 얻음
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    // Type type = assembly.GetType(className);
                    // string a = "GGemCoLibrary.Core." + objectName;
                    string a = objectName;
                    Type type = Type.GetType(a);
                    if (type != null)
                    {
                        // Add SaveDataManager.cs script to the SaveDataManager object
                        newManagerObject.AddComponent(type);
                    }
                }
                else
                {
                    // Debug.Log("SaveDataManager already exists under Manager.");
                    existManager += objectName+"\n";
                }
            }

            if (existManager != "")
            {
                existManager = "이미 존재하는 매니저는 추가되지 않았습니다.\n" + existManager;
            }
            EditorUtility.DisplayDialog(title, "매니저가 추가되었습니다.\n"+existManager, "OK");
        }
    }
}