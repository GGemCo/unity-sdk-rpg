using System;
using System.Collections.Generic;
using System.IO;
using GGemCo.Scripts;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GGemCo.Editor
{
    public class QuestEditorWindow : EditorWindow
    {
        private Quest quest = new Quest();
        private Vector2 scrollPos;
        private ReorderableList stepList;
        private ReorderableList rewardItemList;
        private const float LabelWidth = 70f;
        
        private TableLoaderManager tableLoaderManager;
        private TableQuest tableQuest;
        private int selectedQuestIndex;
        private List<string> questTitle = new List<string>();
        private Dictionary<int, StruckTableQuest> questInfos = new Dictionary<int, StruckTableQuest>(); 

        private QuestStepListDrawer questStepListDrawer;
        private RewardItemListDrawer rewardItemListDrawer;
        
        [MenuItem(ConfigEditor.NameToolQuest, false, (int)ConfigEditor.ToolOrdering.Quest)]
        public static void ShowWindow()
        {
            GetWindow<QuestEditorWindow>(ConfigEditor.NameToolQuest);
        }
        private void OnEnable()
        {
            tableLoaderManager = new TableLoaderManager();
            tableQuest = tableLoaderManager.LoadQuestTable();
            LoadQuestInfoData();
            
            quest.steps ??= new List<QuestStep>();
            quest.reward ??= new QuestReward();
            quest.reward.items ??= new List<RewardItem>();
            
            questStepListDrawer = new QuestStepListDrawer(quest.steps);
            rewardItemListDrawer = new RewardItemListDrawer(quest.reward);
        }

        private void LoadQuestInfoData()
        {
            Dictionary<int, Dictionary<string, string>> npcDictionary = tableQuest.GetDatas();
             
            questTitle.Clear();
            questInfos.Clear();
            int index = 0;
            // foreach 문을 사용하여 딕셔너리 내용을 출력
            foreach (KeyValuePair<int, Dictionary<string, string>> outerPair in npcDictionary)
            {
                var info = tableQuest.GetDataByUid(outerPair.Key);
                if (info.Uid <= 0) continue;
                questTitle.Add($"{info.Uid} - {info.Title}");
                questInfos.TryAdd(index, info);
                index++;
            }
        }

        private int previousIndex;
        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = LabelWidth; // 라벨 너비 축소
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            Common.OnGUITitle("저장/불러오기");
            selectedQuestIndex = EditorGUILayout.Popup("연출 선택", selectedQuestIndex, questTitle.ToArray());
            if (previousIndex != selectedQuestIndex)
            {
                // 선택이 바뀌었을 때 실행할 코드
                // Debug.Log($"선택이 변경되었습니다: {questTitle[selectedQuestIndex]}");
                if (LoadQuestFromJson())
                {
                    previousIndex = selectedQuestIndex;
                }
                else
                {
                    selectedQuestIndex = previousIndex;
                }
            }
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("JSON 저장")) SaveQuestToJson();
            if (GUILayout.Button("JSON 불러오기")) LoadQuestFromJson();
            if (GUILayout.Button("새로 만들기")) NewQuestJson();
            GUILayout.EndHorizontal();
            
            Common.GUILineBlue(2);
            // 기본정보
            Common.OnGUITitle("퀘스트 기본 정보");
            // nextNodeGuid 읽기 전용 처리
            GUI.enabled = false;
            quest.uid = EditorGUILayout.IntField("Uid", quest.uid);
            quest.title = EditorGUILayout.TextField("제목", quest.title);
            GUI.enabled = true;
           
            // 단계별 정보
            Common.GUILineBlue(2);
            questStepListDrawer.List.DoLayoutList();

            // 보상
            Common.GUILineBlue(2);
            rewardItemListDrawer.DoLayout();

            GUILayout.Space(30);

            EditorGUILayout.EndScrollView();
        }

        private void NewQuestJson()
        {
            bool result = EditorUtility.DisplayDialog("새로만들기", "현재 불러온 내용이 초기화 됩니다.\n계속 진행할가요?", "네", "아니요");
            if (!result) return;
            quest = new Quest();
            OnEnable();
        }

        /// <summary>
        /// json 으로 저장하기
        /// </summary>
        private void SaveQuestToJson()
        {
            bool result = EditorUtility.DisplayDialog("저장하기", "현재 선택된 퀘스트에 저장하시겠습니까?", "네", "아니요");
            if (!result) return;
            var info = questInfos.GetValueOrDefault(selectedQuestIndex);
            if (info == null) return;
            string fileName = info.FileName;
            string path = Path.Combine(QuestConstants.GetJsonFolderPath(), fileName+".json");
            string json = JsonConvert.SerializeObject(quest, Formatting.Indented);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(ConfigEditor.NameToolQuest, "Json 저장하기 완료", "OK");
        }
        /// <summary>
        /// json 불러오기
        /// </summary>
        private bool LoadQuestFromJson()
        {
            bool result = EditorUtility.DisplayDialog("불러오기", "현재 불러온 내용이 초기화 됩니다.\n계속 진행할가요?", "네", "아니요");
            if (!result) return false;
            
            var info = questInfos.GetValueOrDefault(selectedQuestIndex);
            if (info == null) return false;
            string path = Path.Combine(QuestConstants.JsonFolderName, info.FileName);
            
            try
            {
                TextAsset textFile = Resources.Load<TextAsset>($"{path}");
                if (textFile != null)
                {
                    string content = textFile.text;
                    if (!string.IsNullOrEmpty(content))
                    {
                        quest = JsonConvert.DeserializeObject<Quest>(content);
                        if (quest != null)
                        {
                            OnEnable(); // 리스트 다시 초기화
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"퀘스트 json 파일을 불러오는중 오류가 발생했습니다. {path}: {ex.Message}");
            }

            return false;
        }
    }
}
