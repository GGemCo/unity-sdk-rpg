using GGemCo.Scripts;
using UnityEditor;
using UnityEngine;
using EventType = GGemCo.Scripts.EventType;

namespace GGemCo.Editor
{
    /// <summary>
    /// 연출 클립 inspector 
    /// </summary>
    [CustomPropertyDrawer(typeof(CutsceneEvent))]
    public class CutsceneEventDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
        {
            var timeProp = property.FindPropertyRelative("time");
            var typeProp = property.FindPropertyRelative("type");

            var cameraMoveProp = property.FindPropertyRelative("cameraMove");
            var cameraZoomProp = property.FindPropertyRelative("cameraZoom");
            var cameraShakeProp = property.FindPropertyRelative("cameraShake");
            var cameraChangeTargetProp = property.FindPropertyRelative("cameraChangeTarget");
            
            var characterMoveProp = property.FindPropertyRelative("characterMove");
            
            var dialogueBalloonProp = property.FindPropertyRelative("dialogueBalloon");

            var line = pos;
            line.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(line, timeProp);
            line.y += line.height + 2;

            EditorGUI.PropertyField(line, typeProp);
            line.y += line.height + 2;

            EventType eventType = (EventType)typeProp.enumValueIndex;

            switch (eventType)
            {
                case EventType.CameraMove:
                    EditorGUI.PropertyField(line, cameraMoveProp, true);
                    break;
                case EventType.CameraZoom:
                    EditorGUI.PropertyField(line, cameraZoomProp, true);
                    break;
                case EventType.CameraShake:
                    EditorGUI.PropertyField(line, cameraShakeProp, true);
                    break;
                case EventType.CameraChangeTarget:
                    EditorGUI.PropertyField(line, cameraChangeTargetProp, true);
                    break;
                case EventType.CharacterMove:
                    EditorGUI.PropertyField(line, characterMoveProp, true);
                    break;
                case EventType.DialogueBalloon:
                    EditorGUI.PropertyField(line, dialogueBalloonProp, true);
                    break;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var typeProp = property.FindPropertyRelative("type");
            EventType eventType = (EventType)typeProp.enumValueIndex;

            float baseHeight = EditorGUIUtility.singleLineHeight * 2 + 6;
            switch (eventType)
            {
                case EventType.CameraMove:
                    return baseHeight + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("cameraMove"));
                case EventType.CameraZoom:
                    return baseHeight + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("cameraZoom"));
                case EventType.CameraShake:
                    return baseHeight + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("cameraShake"));
                case EventType.CameraChangeTarget:
                    return baseHeight + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("cameraChangeTarget"));
                
                case EventType.CharacterMove: 
                    return baseHeight + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("characterMove"));
                
                case EventType.DialogueBalloon: 
                    return baseHeight + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("dialogueBalloon"));
                default: return baseHeight;
            }
        }
    }

}