using System.Collections.Generic;
using System.IO;
using GGemCo.Scripts;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

namespace GGemCo.Editor
{
    public class CutsceneEditorWindow : EditorWindow
    {
        private const string Title = "연출툴";
        private CutsceneData data;
        private ReorderableList list;
        private const string IMPORT_FOLDER = "Assets/_test";

        private TextAsset selectedJson;
        
        [MenuItem("GGemCoTool/"+Title)]
        static void Open() => GetWindow<CutsceneEditorWindow>(Title);

        private void OnGUI()
        {
            GUILayout.Space(20);
            GUILayout.Label("JSON -> Timeline 생성", EditorStyles.boldLabel);
            selectedJson = (TextAsset)EditorGUILayout.ObjectField("JSON 파일", selectedJson, typeof(TextAsset), false);

            if (GUILayout.Button("JSON으로부터 타임라인 생성"))
            {
                if (selectedJson != null)
                    ImportJsonToTimeline(selectedJson);
                else
                    Debug.LogWarning("JSON 파일을 선택해주세요.");
            }
        }
        private void ImportJsonToTimeline(TextAsset jsonAsset)
        {
            CutsceneData cutsceneData = JsonConvert.DeserializeObject<CutsceneData>(jsonAsset.text);
            string assetName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(jsonAsset));

            if (!Directory.Exists(IMPORT_FOLDER))
                Directory.CreateDirectory(IMPORT_FOLDER);

            string timelinePath = Path.Combine(IMPORT_FOLDER, $"{assetName}.playable");
            var timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timelineAsset, timelinePath);

            if (cutsceneData == null || cutsceneData.events == null)
            {
                Debug.LogError("Json 파싱 실패 또는 이벤트 없음");
                return;
            }

            // 트랙 맵 (EventType -> Track)
            Dictionary<CutsceneEventType, TrackAsset> trackMap = new Dictionary<CutsceneEventType, TrackAsset>();

            foreach (var evt in cutsceneData.events)
            {
                // 트랙이 이미 있으면 재사용, 없으면 새로 생성
                if (!trackMap.TryGetValue(evt.type, out var track))
                {
                    track = timelineAsset.CreateTrack<CutsceneEventTrack>(null, $"{evt.type} Track");
                    trackMap[evt.type] = track;
                }

                // 클립 생성 및 설정
                var clip = track.CreateClip<CutsceneEventClip>();
                clip.start = evt.time;
                clip.duration = evt.duration > 0 ? evt.duration : 1.0; // 최소 duration 보장

                var asset = clip.asset as CutsceneEventClip;
                if (asset != null)
                {
                    asset.SetEvent(evt);
                }
            }

            EditorUtility.SetDirty(timelineAsset);
            AssetDatabase.SaveAssets();
            // TimelineEditorUtility.SelectTimelineAsset(timeline);

            GcLogger.Log($"Timeline 생성 완료: {timelinePath}");
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(Title, "Timeline 생성 완료", "OK");
        }
    }
}