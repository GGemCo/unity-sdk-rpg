using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GGemCo.Editor
{
    public class ConnectionHandler
    {
        private DialogueEditorWindow editorWindow;

        public ConnectionHandler(DialogueEditorWindow window)
        {
            editorWindow = window;
        }

        public void DrawConnections()
        {
            Handles.color = Color.white;
            foreach (var node in editorWindow.nodes)
            {
                foreach (var option in node.options)
                {
                    if (!string.IsNullOrEmpty(option.nextNodeGuid))
                    {
                        DialogueNode targetNode = editorWindow.nodes.FirstOrDefault(n => n.guid == option.nextNodeGuid);
                        if (targetNode != null)
                        {
                            Vector2 startPos = option.connectionPoint * editorWindow.zoom + editorWindow.panOffset;
                            Vector2 endPos =
                                new Vector2(targetNode.position.x, targetNode.position.y + 30) * editorWindow.zoom +
                                editorWindow.panOffset;

                            // 선택지별로 연결선 클릭 체크
                            if (Handles.Button((startPos + endPos) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
                            {
                                // 드래그 시작
                                editorWindow.draggingFromNode = node;
                                editorWindow.draggingFromOption = option;
                                editorWindow.isDraggingConnection = true;
                            }

                            Handles.DrawBezier(
                                startPos,
                                endPos,
                                startPos + Vector2.right * 50f,
                                endPos + Vector2.left * 50f,
                                Color.white,
                                null,
                                3f
                            );
                        }
                    }
                }
            }

            // 드래그 중이면 마우스 따라가게
            if (editorWindow.isDraggingConnection && editorWindow.draggingFromOption != null)
            {
                Handles.DrawBezier(
                    editorWindow.draggingFromOption.connectionPoint,
                    Event.current.mousePosition,
                    editorWindow.draggingFromOption.connectionPoint + Vector2.right * 50f,
                    Event.current.mousePosition + Vector2.left * 50f,
                    Color.yellow,
                    null,
                    3f
                );

                editorWindow.Repaint();
            }
        }
    }
}
