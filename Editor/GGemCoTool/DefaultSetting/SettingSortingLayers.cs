using GGemCo.Editor.GGemCoTool.Utils;
using GGemCo.Scripts.Configs;
using UnityEditor;
using UnityEngine;

namespace GGemCo.Editor.GGemCoTool.DefaultSetting
{
    public class SettingSortingLayers
    {
        private readonly string title = "Sorting Layer 추가하기";

        private readonly string[] sortingLayersToAdd = { ConfigSortingLayer.MapTerrain, ConfigSortingLayer.MapObject, ConfigSortingLayer.Character, ConfigSortingLayer.UI };

        public void OnGUI()
        {
            Common.OnGUITitle(title);

            if (GUILayout.Button(title))
            {
                AddSortingLayers();
            }
        }

        private void AddSortingLayers()
        {
            // Sorting Layer 추가를 위해 TagManager 가져오기
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty sortingLayersProp = tagManager.FindProperty("m_SortingLayers");

            int highestID = GetHighestSortingLayerID(sortingLayersProp); // 가장 높은 ID 찾기

            // Sorting Layer 추가
            foreach (string layer in sortingLayersToAdd)
            {
                if (!SortingLayerExists(sortingLayersProp, layer))
                {
                    sortingLayersProp.InsertArrayElementAtIndex(sortingLayersProp.arraySize);
                    SerializedProperty newLayer = sortingLayersProp.GetArrayElementAtIndex(sortingLayersProp.arraySize - 1);
                    newLayer.FindPropertyRelative("name").stringValue = layer;
                    newLayer.FindPropertyRelative("uniqueID").intValue = ++highestID; // ID를 1씩 증가시켜 설정
                }
            }

            // 변경 사항 저장
            tagManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(tagManager.targetObject);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(title, "Sorting Layer 추가 완료", "OK");
        }

        private bool SortingLayerExists(SerializedProperty sortingLayersProp, string layer)
        {
            for (int i = 0; i < sortingLayersProp.arraySize; i++)
            {
                if (sortingLayersProp.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue.Equals(layer))
                {
                    return true;
                }
            }
            return false;
        }

        private int GetHighestSortingLayerID(SerializedProperty sortingLayersProp)
        {
            int highestID = 0;

            for (int i = 0; i < sortingLayersProp.arraySize; i++)
            {
                int id = sortingLayersProp.GetArrayElementAtIndex(i).FindPropertyRelative("uniqueID").intValue;
                if (id > highestID)
                {
                    highestID = id;
                }
            }

            return highestID;
        }
    }
}
