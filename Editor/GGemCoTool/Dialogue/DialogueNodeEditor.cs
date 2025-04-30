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
                
                // nextNodeGuid 읽기 전용 처리
                GUI.enabled = false;
                EditorGUI.PropertyField(
                    new Rect(rect.x + rect.width * 0.55f, rect.y, rect.width * 0.45f, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("nextNodeGuid"), GUIContent.none);
                GUI.enabled = true;
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogueText"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fontSize"));
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("position"));
            // nextNodeGuid 읽기 전용 처리
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nextNodeGuid"));
            GUI.enabled = true;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("characterType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("characterUid"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("thumbnailImage"));
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startQuestUid"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startQuestStep"));
            
            GUILayout.Space(20);
            optionList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}