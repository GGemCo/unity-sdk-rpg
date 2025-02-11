using UnityEditor;
using UnityEngine;

namespace GGemCo.Editor.GGemCoTool
{
    public static class Common
    {
        public static void OnGUITitle(string title)
        {
            GUILayout.Label($"[ {title} ]", EditorStyles.whiteLargeLabel);
        }
    }
}