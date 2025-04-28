using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GGemCo.Editor
{
    public class NodeHandler
    {
        private DialogueEditorWindow editorWindow;

        public NodeHandler(DialogueEditorWindow window)
        {
            editorWindow = window;
        }

        public void DrawNodes()
        {
            Matrix4x4 oldMatrix = GUI.matrix;
            GUIUtility.ScaleAroundPivot(Vector2.one * editorWindow.zoom, Vector2.zero);
            GUI.matrix = Matrix4x4.TRS(editorWindow.panOffset, Quaternion.identity, Vector3.one) * GUI.matrix;
            
            DialogueNode nodeToDelete = null; // 삭제할 노드 예약

            foreach (DialogueNode node in editorWindow.nodes)
            {
                DrawNode(node, ref nodeToDelete);
            }

            if (nodeToDelete != null)
            {
                DeleteNode(nodeToDelete);
            }
            
            GUI.matrix = oldMatrix;
        }
        
        private void DrawNode(DialogueNode node, ref DialogueNode nodeToDelete)
        {
            GUIStyle style = new GUIStyle(GUI.skin.window)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
            if (node == editorWindow.selectedNode)
            {
                style.normal.background =
                    EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
                style.border = new RectOffset(12, 12, 12, 12);
            }

            GUILayout.BeginArea(new Rect(node.position, new Vector2(250, 150)), style);
            EditorGUILayout.LabelField(node.title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(node.dialogueText, EditorStyles.wordWrappedLabel);

            foreach (var option in node.options)
            {
                Rect optionRect = GUILayoutUtility.GetRect(new GUIContent($"▶ {option.optionText}"), GUI.skin.label);
                EditorGUI.LabelField(optionRect, $"▶ {option.optionText}");

                // 선택지 연결 포인트 저장
                option.connectionPoint = new Vector2(node.position.x + 250, node.position.y + optionRect.y + optionRect.height / 2); // 오른쪽 바깥쪽에서 출발
            }

            if (GUILayout.Button("연결하기"))
            {
                if (editorWindow.selectedNode != null && editorWindow.selectedNode != node)
                {
                    editorWindow.selectedNode.options.Add(new DialogueOption
                    {
                        optionText = "새 선택지",
                        nextNodeGuid = node.guid
                    });
                }
            }

            if (GUILayout.Button("삭제하기"))
            {
                // Undo.RecordObject(this, "Delete Node");
                nodeToDelete = node; // 바로 삭제하지 않고 예약
            }

            GUILayout.EndArea();
        }
        
        private void DeleteNode(DialogueNode node)
        {
            if (editorWindow.nodes.Contains(node))
            {
                editorWindow.nodes.Remove(node);
                foreach (DialogueNode n in editorWindow.nodes)
                {
                    foreach (var option in n.options)
                    {
                        if (option.nextNodeGuid == node.guid)
                            option.nextNodeGuid = null;
                    }
                }
            }
        }
        public void ProcessNodeEvents(Event e)
        {
            if (editorWindow.nodes == null) return;

            // 마우스 좌표를 줌 및 패닝을 반영하여 변환
            Vector2 adjustedMousePosition = (e.mousePosition - editorWindow.panOffset) / editorWindow.zoom;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                editorWindow.draggingNode = GetNodeAtPoint(adjustedMousePosition);
                if (editorWindow.draggingNode != null)
                {
                    editorWindow.selectedNode = editorWindow.draggingNode;
                    Selection.activeObject = editorWindow.draggingNode;
                    editorWindow.draggingOffset = editorWindow.draggingNode.position - adjustedMousePosition;
                }
                else
                {
                    editorWindow.selectedNode = null;
                    Selection.activeObject = null;
                }
            }

            if (e.type == EventType.MouseDrag && e.button == 0 && editorWindow.draggingNode != null)
            {
                editorWindow.draggingNode.position = adjustedMousePosition + editorWindow.draggingOffset;
                GUI.changed = true;
            }

            if (e.type == EventType.MouseUp && e.button == 0)
            {
                editorWindow.draggingNode = null;
            }
        }

        private DialogueNode GetNodeAtPoint(Vector2 point)
        {
            foreach (DialogueNode node in editorWindow.nodes)
            {
                Rect rect = new Rect(node.position, new Vector2(250, 150));
                if (rect.Contains(point))
                    return node;
            }
            return null;
        }

        public void HandleEvents()
        {
            if (editorWindow.isDraggingConnection && Event.current.type == EventType.MouseUp)
            {
                // 드랍 위치에 있는 노드를 찾아서 연결
                DialogueNode targetNode = FindNodeAtPosition(Event.current.mousePosition);
                if (targetNode != null && editorWindow.draggingFromOption != null)
                {
                    editorWindow.draggingFromOption.nextNodeGuid = targetNode.guid;
                }

                // 드래그 종료
                editorWindow.draggingFromNode = null;
                editorWindow.draggingFromOption = null;
                editorWindow.isDraggingConnection = false;
            }
        }

        private DialogueNode FindNodeAtPosition(Vector2 pos)
        {
            return editorWindow.nodes.FirstOrDefault(node =>
                new Rect(node.position, new Vector2(250, 150)).Contains(pos)
            );
        }
        
        public void AddNode(Vector2 nodePosition)
        {
            DialogueNode node = ScriptableObject.CreateInstance<DialogueNode>();
            node.guid = Guid.NewGuid().ToString();
            node.position = nodePosition;
            editorWindow.nodes.Add(node);
        }
        public void AddNode()
        {
            DialogueNode node = ScriptableObject.CreateInstance<DialogueNode>();
            node.guid = Guid.NewGuid().ToString();
            node.position = new Vector2(editorWindow.position.width/2, editorWindow.position.height/2);
            editorWindow.nodes.Add(node);
        }

    }
}
