using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GGemCo.Editor
{
    /// <summary>
    /// 대사 노드 커스텀 inspector 
    /// </summary>
    [CustomEditor(typeof(DialogueNode))]
    public class DialogueNodeEditor : UnityEditor.Editor
    {
        private ReorderableList optionList;

        private void OnEnable()
        {
            optionList = new ReorderableList(serializedObject,
                serializedObject.FindProperty("options"),
                true, true, true, true)
            {
                drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, "선택지 목록");
                }
            };

            optionList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = optionList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, rect.width * 0.5f, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("optionText"), GUIContent.none);
                EditorGUI.PropertyField(
                    new Rect(rect.x + rect.width * 0.55f, rect.y, rect.width * 0.45f, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("nextNodeGuid"), GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("title"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogueText"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("position"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("characterType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("characterUid"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fontSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("thumbnailImage"));

            optionList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}