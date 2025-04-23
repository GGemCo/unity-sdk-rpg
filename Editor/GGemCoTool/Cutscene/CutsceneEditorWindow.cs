using System.Collections.Generic;
using System.IO;
using GGemCo.Scripts;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;
using EventType = GGemCo.Scripts.EventType;

namespace GGemCo.Editor
{
    [CustomEditor(typeof(CutsceneEventClip))]
    public class CutsceneEventClipEditor : UnityEditor.Editor
    {
        private const string Title = "연출툴";
        SerializedProperty eventsProp;
        private readonly string jsonFolderPath = Application.dataPath+"/Resources/Cutscene/";

        void OnEnable()
        {
            serializedObject.Update();
            eventsProp = serializedObject.FindProperty("events");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(eventsProp, true);
            
            if (GUILayout.Button("이 클립이 포함된 타임라인을 JSON으로 저장"))
            {
                ExportTimelineFromClip((CutsceneEventClip)target);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        private void ExportTimelineFromClip(CutsceneEventClip clip)
        {
            // 타임라인 찾기
            TimelineAsset timeline = FindTimelineAsset(clip);
            if (timeline == null)
            {
                GcLogger.LogWarning("TimelineAsset을 찾을 수 없습니다.");
                return;
            }

            // JSON 저장
            GcLogger.Log(timeline);
            ExportToJson(timeline);
            // CutsceneJsonExporter.Export(timeline);
        }
        private TimelineAsset FindTimelineAsset(CutsceneEventClip clip)
        {
            // 현재 열린 Timeline에서 찾아봄
            var director = TimelineEditor.inspectedDirector;
            if (director != null && director.playableAsset is TimelineAsset timelineAsset)
            {
                return timelineAsset;
            }
    
            // 대체 방법: 강제로 Timeline을 검색하거나 연결
            return null;
        }
        
        private void ExportToJson(TimelineAsset timeline)
        {
            // var director = Selection.activeGameObject?.GetComponent<PlayableDirector>();
            // if (director == null || !(director.playableAsset is TimelineAsset timeline))
            // {
            //     Debug.LogError("타임라인이 있는 오브젝트를 선택해주세요.");
            //     return;
            // }

            var events = new List<CutsceneEvent>();

            foreach (var track in timeline.GetOutputTracks())
            {
                if (!(track is CutsceneEventTrack)) continue;
                
                if (track.muted) continue;
                
                foreach (var clip in track.GetClips())
                {
                    if (clip.asset is CutsceneEventClip cutsceneClip)
                    {
                        foreach (var e in cutsceneClip.events)
                        {
                            if (e != null &&
                                ((e.type == EventType.CharacterMove &&
                                  e.characterMove.characterType == CharacterConstants.Type.None)
                                 || (e.type == EventType.CameraChangeTarget &&
                                     e.cameraChangeTarget.characterType == CharacterConstants.Type.None)
                                ))
                            {
                                GcLogger.LogError($"type: {e.type} / 캐릭터 타입을 정하지 않았습니다.");
                                continue;
                            }

                            var evtCopy = new CutsceneEvent
                            {
                                time = (float)(clip.start),
                                duration = (float)clip.duration,
                                type = e.type,
                                cameraMove = e.type == EventType.CameraMove ? e.cameraMove : null,
                                cameraZoom = e.type == EventType.CameraZoom ? e.cameraZoom : null,
                                cameraShake = e.type == EventType.CameraShake ? e.cameraShake : null,
                                cameraChangeTarget = e.type == EventType.CameraChangeTarget ? e.cameraChangeTarget : null,
                                
                                characterMove = e.type == EventType.CharacterMove ? e.characterMove : null,
                                
                                dialogueBalloon = e.type == EventType.DialogueBalloon ? e.dialogueBalloon : null,
                            };
                            events.Add(evtCopy);
                        }
                    }
                }
            }

            events.Sort((a, b) => a.time.CompareTo(b.time));

            var data = new CutsceneData
            {
                duration = events.Count > 0 ? (events[^1].time + events[^1].duration) : 0f,
                events = events
            };
            
            string json = JsonConvert.SerializeObject(data, Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

            // string path = EditorUtility.SaveFilePanel("Save Cutscene JSON", "", "Cutscene.json", "json");
            string path = Path.Combine(jsonFolderPath, timeline.name + ".json");
            File.WriteAllText(path, json);
            Debug.Log($"Saved cutscene to: {path}");
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(Title, "Json 저장하기 완료", "OK");
        }
    }
    public class CutsceneEditorWindow : EditorWindow
    {
        private const string Title = "연출툴";
        private CutsceneData _data;
        private ReorderableList _list;
        private readonly string jsonFolderPath = Application.dataPath+"/Resources/Cutscene/";
        private string importFolder = "Assets/_test";

        private TextAsset selectedJson;
        
        [MenuItem("GGemCoTool/"+Title)]
        static void Open() => GetWindow<CutsceneEditorWindow>(Title);

        private void OnEnable()
        {
        }

        private void OnGUI()
        {
            GUILayout.Space(20);
            GUILayout.Label("📥 JSON → Timeline 생성", EditorStyles.boldLabel);
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
            CutsceneData data = JsonConvert.DeserializeObject<CutsceneData>(jsonAsset.text);
            string assetName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(jsonAsset));

            if (!Directory.Exists(importFolder))
                Directory.CreateDirectory(importFolder);

            string timelinePath = Path.Combine(importFolder, $"{assetName}.playable");
            var timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timelineAsset, timelinePath);

            if (data == null || data.events == null)
            {
                Debug.LogError("Json 파싱 실패 또는 이벤트 없음");
                return;
            }

            // 트랙 맵 (EventType -> Track)
            Dictionary<EventType, TrackAsset> trackMap = new Dictionary<EventType, TrackAsset>();

            foreach (var evt in data.events)
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