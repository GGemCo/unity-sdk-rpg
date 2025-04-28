using Newtonsoft.Json;
using System.IO;
using GGemCo.Scripts;
using UnityEditor;
using UnityEngine;

namespace GGemCo.Editor
{
    public class FileHandler
    {
        private DialogueEditorWindow editorWindow;

        public FileHandler(DialogueEditorWindow window)
        {
            editorWindow = window;
        }

        public void SaveToJson()
        {
            DialogueData data = new DialogueData();

            foreach (var node in editorWindow.nodes)
            {
                DialogueNodeData nodeData = new DialogueNodeData
                {
                    guid = node.guid,
                    title = node.title,
                    dialogueText = node.dialogueText,
                    position = new Vec2(node.position),
                    characterType = node.characterType,
                    characterUid = node.characterUid,
                    fontSize = node.fontSize,
                    thumbnailImage = node.thumbnailImage,
                    options = node.options
                };
                data.nodes.Add(nodeData);
            }

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            string path = EditorUtility.SaveFilePanel("Save Dialogue JSON", Application.dataPath, "DialogueData", "json");

            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, json);
                AssetDatabase.Refresh();
            }
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("대사 생성툴", "Json 저장하기 완료", "OK");

        }

        public void LoadFromJson()
        {
            string path = EditorUtility.OpenFilePanel("Load Dialogue JSON", Application.dataPath, "json");

            if (!string.IsNullOrEmpty(path))
            {
                string json = File.ReadAllText(path);
                DialogueData data = JsonUtility.FromJson<DialogueData>(json);

                editorWindow.nodes.Clear();

                foreach (var nodeData in data.nodes)
                {
                    DialogueNode node = ScriptableObject.CreateInstance<DialogueNode>();
                    node.guid = nodeData.guid;
                    node.title = nodeData.title;
                    node.dialogueText = nodeData.dialogueText;
                    node.position = node.position;
                    node.options = nodeData.options;
                    editorWindow.nodes.Add(node);
                }
            }
        }
    }
}