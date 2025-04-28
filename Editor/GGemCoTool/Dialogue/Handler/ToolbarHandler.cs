using UnityEditor;
using UnityEngine;

namespace GGemCo.Editor
{
    public class ToolbarHandler
    {
        private DialogueEditorWindow editorWindow;

        public ToolbarHandler(DialogueEditorWindow window)
        {
            editorWindow = window;
        }

        public void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            // if (GUILayout.Button("자동 배치", EditorStyles.toolbarButton))
            // {
            //     AutoLayout();
            // }

            if (GUILayout.Button("노드 추가", EditorStyles.toolbarButton))
            {
                editorWindow.NodeHandler?.AddNode();
            }

            if (GUILayout.Button("저장", EditorStyles.toolbarButton))
            {
                editorWindow.FileHandler?.SaveToJson();
            }

            if (GUILayout.Button("불러오기", EditorStyles.toolbarButton))
            {
                editorWindow.FileHandler?.LoadFromJson();
            }
            // 100% 보기 버튼
            if (GUILayout.Button("100% 보기", EditorStyles.toolbarButton))
            {
                editorWindow.ZoomPanHandler?.SetZoom(1.0f);
                editorWindow.panOffset = Vector2.zero; // 위치도 초기화
            }

            // 화면 꽉차게 보기 버튼
            if (GUILayout.Button("화면 꽉차게 보기", EditorStyles.toolbarButton))
            {
                editorWindow.ZoomPanHandler?.FitViewToNodes();
            }

            GUILayout.EndHorizontal();
        }
    }
}