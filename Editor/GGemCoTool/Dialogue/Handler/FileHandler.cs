using System;
using Newtonsoft.Json;
using System.IO;
using GGemCo.Scripts;
using UnityEditor;
using UnityEngine;

namespace GGemCo.Editor
{
    /// <summary>
    /// 대사 저장, 불러오기
    /// </summary>
    public class FileHandler
    {
        private readonly DialogueEditorWindow editorWindow;

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
                    nextNodeGuid = node.nextNodeGuid,
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

        public void LoadFromJson(string fileName)
        {
            string jsonFilePath = $"Dialogue/{fileName}";
            try
            {
                TextAsset textFile = Resources.Load<TextAsset>($"{jsonFilePath}");
                if (textFile != null)
                {
                    string content = textFile.text;
                    if (string.IsNullOrEmpty(content)) return;
                    DialogueData data = JsonConvert.DeserializeObject<DialogueData>(content);

                    editorWindow.nodes.Clear();

                    foreach (var nodeData in data.nodes)
                    {
                        DialogueNode node = ScriptableObject.CreateInstance<DialogueNode>();
                        node.guid = nodeData.guid;
                        node.title = nodeData.title;
                        node.dialogueText = nodeData.dialogueText;
                        node.position = nodeData.position.ToVector2();
                        node.options = nodeData.options;
                        node.nextNodeGuid = nodeData.nextNodeGuid;
                        editorWindow.nodes.Add(node);
                    }
                }
                else
                {
                    GcLogger.LogError("파일이 없습니다. path: " + jsonFilePath);
                }
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"json 파일을 읽어오는데 오류가 발생하였습니다. path: {jsonFilePath}, error message: {ex.Message}");
            }
        }
    }
}