using System.Collections.Generic;
using GGemCo.Scripts;
using UnityEditor;
using UnityEngine;

namespace GGemCo.Editor
{
    /// <summary>
    /// 대사 생성툴 - 메뉴
    /// </summary>
    public class ToolbarHandler
    {
        private readonly DialogueEditorWindow editorWindow;

        private readonly TableDialogue tableDialogue;
        private int selectedDialogueIndex;

        private readonly List<string> dialogueMemos = new List<string>();
        private readonly Dictionary<int, StruckTableDialogue> dialogueInfos = new Dictionary<int, StruckTableDialogue>(); 
        
        public ToolbarHandler(DialogueEditorWindow window)
        {
            editorWindow = window;
            
            var tableLoaderManager = new TableLoaderManager();
            tableDialogue = tableLoaderManager.LoadDialogueTable();
            LoadCutsceneInfoData();
        }

        private void LoadCutsceneInfoData()
        {
            if (tableDialogue == null) return;
            Dictionary<int, Dictionary<string, string>> npcDictionary = tableDialogue.GetDatas();
             
            dialogueMemos.Clear();
            dialogueInfos.Clear();
            int index = 0;
            // foreach 문을 사용하여 딕셔너리 내용을 출력
            foreach (KeyValuePair<int, Dictionary<string, string>> outerPair in npcDictionary)
            {
                var info = tableDialogue.GetDataByUid(outerPair.Key);
                if (info.Uid <= 0) continue;
                dialogueMemos.Add($"{info.Uid} - {info.Memo}");
                dialogueInfos.TryAdd(index, info);
                index++;
            }
        }

        public void DrawToolbar()
        {
            if (editorWindow == null) return;
            GUILayout.BeginVertical(EditorStyles.toolbar, GUILayout.Width(250));

            // if (GUILayout.Button("자동 배치", EditorStyles.toolbarButton))
            // {
            //     AutoLayout();
            // }
            EditorGUILayout.Space(20);
            selectedDialogueIndex = EditorGUILayout.Popup("", selectedDialogueIndex, dialogueMemos.ToArray());
            if (GUILayout.Button("대사 불러오기", EditorStyles.toolbarButton))
            {
                if (editorWindow.nodes?.Count > 0)
                {
                    bool result = EditorUtility.DisplayDialog("불러오기", "현재 대사 Node 가 만들어진 상태입니다.\n저장 하셨나요?", "네", "아니요");
                    if (result)
                    {
                        var info = dialogueInfos.GetValueOrDefault(selectedDialogueIndex);
                        editorWindow.FileHandler?.LoadFromJson(info.FileName);
                    }
                }
                else
                {
                    var info = dialogueInfos.GetValueOrDefault(selectedDialogueIndex);
                    editorWindow.FileHandler?.LoadFromJson(info.FileName);
                }
            }
            EditorGUILayout.Space();
            Common.GUILine(2);
            EditorGUILayout.Space();
            if (GUILayout.Button("노드 추가", EditorStyles.toolbarButton))
            {
                editorWindow.NodeHandler?.AddNode();
            }

            if (GUILayout.Button("저장", EditorStyles.toolbarButton))
            {
                editorWindow.FileHandler?.SaveToJson();
            }

            // 100% 보기 버튼
            if (GUILayout.Button("100% 보기", EditorStyles.toolbarButton))
            {
                editorWindow.ZoomPanHandler?.SetZoom(1.0f);
                editorWindow.panOffset = Vector2.zero; // 위치도 초기화
            }
            EditorGUILayout.Space();
            Common.GUILine(2);
            EditorGUILayout.Space();
            if (GUILayout.Button("모두 지우기", EditorStyles.toolbarButton))
            {
                bool result = EditorUtility.DisplayDialog("삭제", "정말로 삭제하시겠습니까?", "확인", "취소");
                if (result)
                {
                    editorWindow?.nodes?.Clear();
                }
            }

            // // 화면 꽉차게 보기 버튼
            // if (GUILayout.Button("화면 꽉차게 보기", EditorStyles.toolbarButton))
            // {
            //     editorWindow.ZoomPanHandler?.FitViewToNodes();
            // }

            GUILayout.EndVertical();
        }
    }
}