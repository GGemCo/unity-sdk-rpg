using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GGemCo.Editor
{
    /// <summary>
    /// PlayerPrefs 데이터 관리 툴
    /// </summary>
    public class EtcEditor : EditorWindow
    {
        [MenuItem("GGemCoTool/게임 데이터 저장 폴더 열기")]
        private static void OpenGameDataFolder()
        {
            string path = Application.persistentDataPath;

            if (Directory.Exists(path))
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                Process.Start(new ProcessStartInfo()
                {
                    FileName = path,
                    UseShellExecute = true
                });
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                Process.Start("open", path);
#endif
            }
            else
            {
                UnityEngine.Debug.LogError($"폴더를 찾을 수 없습니다: {path}");
            }
        }

    }
}
