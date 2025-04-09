using UnityEditor;
using UnityEngine;

namespace GGemCo.Editor
{
    public abstract class Common
    {
        public static void OnGUITitle(string title)
        {
            GUILayout.Label($"[ {title} ]", EditorStyles.whiteLargeLabel);
        }
    }
}