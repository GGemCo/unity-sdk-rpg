using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace GGemCo.Scripts
{
    /// <summary>
    /// 연출 매니저
    /// </summary>
    public class CutsceneManager : MonoBehaviour
    {
        private enum State { Idle, Loading, Ready, Playing, Finished }
        private State currentState;

        private CutsceneData currentCutscene;
        private float playTimer;
        private int currentIndex;
        private float originalOrthographicSize;
        private DialogueBalloonPool dialogueBalloonPool;

        // 연출중 생성된 캐릭터 관리
        private readonly Dictionary<CharacterConstants.Type, Dictionary<int, GameObject>> createCharacters =
            new Dictionary<CharacterConstants.Type, Dictionary<int, GameObject>>();
        
        // 연출 컨트롤러
        private readonly List<ICutsceneController> controllers = new List<ICutsceneController>();
        private readonly Dictionary<CutsceneEventType, ICutsceneController> eventMap = new();
        private CameraMoveController cameraMoveController;
        private CameraZoomController cameraZoomController;
        private CameraShakeController cameraShakeController;
        private CameraChangeTargetController cameraChangeTargetController;
        private CharacterMoveController characterMoveController;
        private DialogueBalloonController dialogueBalloonController;

        private void Awake()
        {
            createCharacters.Clear();
            playTimer = 0f;
            currentIndex = 0;
            currentState = State.Idle;
        }

        private void Start()
        {
            // 기존 컨트롤러 초기화 이후
            dialogueBalloonPool = new DialogueBalloonPool(SceneGame.Instance.containerDialogueBalloon.transform); // 부모는 선택
            
            // 컨트롤러 수동 생성 + 등록
            controllers.Add(new CameraMoveController(this));
            controllers.Add(new CameraZoomController(this));
            controllers.Add(new CameraShakeController(this));
            controllers.Add(new CameraChangeTargetController(this));
            controllers.Add(new CharacterMoveController(this));
            controllers.Add(new DialogueBalloonController(this, dialogueBalloonPool));

            // 이벤트 타입에 따라 대응되는 컨트롤러 등록
            eventMap[CutsceneEventType.CameraMove] = controllers.OfType<CameraMoveController>().FirstOrDefault();
            eventMap[CutsceneEventType.CameraZoom] = controllers.OfType<CameraZoomController>().FirstOrDefault();
            eventMap[CutsceneEventType.CameraShake] = controllers.OfType<CameraShakeController>().FirstOrDefault();
            eventMap[CutsceneEventType.CameraChangeTarget] = controllers.OfType<CameraChangeTargetController>().FirstOrDefault();
            eventMap[CutsceneEventType.CharacterMove] = controllers.OfType<CharacterMoveController>().FirstOrDefault();
            eventMap[CutsceneEventType.DialogueBalloon] = controllers.OfType<DialogueBalloonController>().FirstOrDefault();
        }
        /// <summary>
        /// 초기화
        /// </summary>
        private void Reset()
        {
            createCharacters.Clear();
            playTimer = 0f;
            currentIndex = 0;
        }
        /// <summary>
        /// 연출 플레이
        /// </summary>
        /// <param name="uid"></param>
        public void PlayCutscene(int uid)
        {
            var info = TableLoaderManager.Instance.TableCutscene.GetDataByUid(uid);
            if (info == null)
            {
                return;
            }
            Reset();
            currentState = State.Loading;

            TextAsset asset = Resources.Load<TextAsset>($"Cutscene/{info.FileName}");
            if (asset == null)
            {
                GcLogger.LogError("연출 json 파일이 없습니다. " + info.FileName);
                return;
            }
            originalOrthographicSize = SceneGame.Instance.mainCamera.orthographicSize;

            currentCutscene = JsonConvert.DeserializeObject<CutsceneData>(asset.text);

            // 리소스 생성, 프리팹 로딩, 사운드 등 선행 처리
            StartCoroutine(PrepareAndPlay());
        }
        /// <summary>
        /// 연출 준비
        /// </summary>
        /// <returns></returns>
        private IEnumerator PrepareAndPlay()
        {
            currentState = State.Ready;

            foreach (var cutsceneEvent in currentCutscene.events)
            {
                if (eventMap.TryGetValue(cutsceneEvent.type, out var controller))
                {
                    yield return StartCoroutine(controller.Ready(cutsceneEvent));
                }
            }

            GcLogger.Log("모든 컨트롤러 준비 완료 → 연출 시작");
            currentState = State.Playing;
        }

        private void Update()
        {
            if (currentState != State.Playing || currentCutscene == null) return;

            playTimer += Time.deltaTime;

            while (currentIndex < currentCutscene.events.Count &&
                   currentCutscene.events[currentIndex].time <= playTimer)
            {
                var evt = currentCutscene.events[currentIndex];
                if (eventMap.TryGetValue(evt.type, out var controller))
                {
                    controller.Trigger(evt);
                }

                currentIndex++;
            }

            foreach (var controller in controllers)
            {
                controller.Update();
            }

            if (!(playTimer > currentCutscene.duration)) return;
            OnCutsceneEnd();
            
        }
        /// <summary>
        /// 연출 종료
        /// </summary>
        private void OnCutsceneEnd()
        {
            GcLogger.Log("연출 종료");
            currentState = State.Finished;
            foreach (var controller in controllers)
            {
                controller.End();
            }
            // 만들었던 캐릭터 지우기
            foreach (var dic1 in createCharacters)
            {
                foreach (var dic2 in dic1.Value)
                {
                    Destroy(dic2.Value);
                }
            }
            // 카메라 player 따라가
            
            // 원래 카메라 size 로 되돌리기
            SceneGame.Instance.cameraManager?.ReSetZoom();
        }
        /// <summary>
        /// 연출에 필요한 캐릭터 생성 후 추가
        /// </summary>
        /// <param name="type"></param>
        /// <param name="characterUid"></param>
        /// <param name="character"></param>
        public void AddCharacter(CharacterConstants.Type type, int characterUid, GameObject character)
        {
            if (!createCharacters.ContainsKey(type))
            {
                createCharacters.Add(type, new Dictionary<int, GameObject>());
            }
            createCharacters[type].Add(characterUid, character);
        }
        /// <summary>
        /// 연출 중 생성된 캐릭터에서 찾기
        /// </summary>
        /// <param name="type"></param>
        /// <param name="characterUid"></param>
        /// <returns></returns>
        public Transform GetCharacter(CharacterConstants.Type type, int characterUid)
        {
            return createCharacters.GetValueOrDefault(type)?.GetValueOrDefault(characterUid)?.transform;
        }
        public bool IsPlaying() => currentState == State.Playing;
    }
}
