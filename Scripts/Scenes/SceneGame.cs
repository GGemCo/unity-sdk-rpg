using System.Collections;
using GGemCo.Scripts.Core;
using GGemCo.Scripts.Items;
using GGemCo.Scripts.Maps;
using GGemCo.Scripts.Popup;
using GGemCo.Scripts.SaveData;
using GGemCo.Scripts.SystemMessage;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI;
using UnityEngine;

namespace GGemCo.Scripts.Scenes
{
    /// <summary>
    /// 게임 씬 관리 클래스
    /// </summary>
    public class SceneGame : DefaultScene
    {
        public static SceneGame Instance { get; private set; }

        private enum GameState { Begin, Combat, End, DirectionStart, DirectionEnd, QuestDialogueStart, QuestDialogueEnd }
        public enum GameSubState { Normal, BossChallenge, DialogueStart, DialogueEnd }

        private GameState State { get; set; }
        private GameSubState SubState { get; set; }
        private bool isStateDirty;

        [HideInInspector] public GameObject player;
        [Header("기본오브젝트")]
        [Tooltip("메인으로 사용되는 Camera")]
        public Camera mainCamera;
        [Tooltip("UI 에 사용되는 메인 Canvas")]
        public Canvas canvasUI;
        [Tooltip("드랍 아이템의 이름 text 오브젝트가 들어갈 오브젝트 입니다.")]
        public GameObject containerDropItemName;
        [Tooltip("워프로 맵 이동시 화면을 가려줄 검정화면")]
        public GameObject bgBlackForMapLoading;
        
        [Header("매니저")]
        [Tooltip("윈도우 매니저")]
        public UIWindowManager uIWindowManager;
        [Tooltip("시스템 메시지 매니저")]
        public SystemMessageManager systemMessageManager;
        [Tooltip("카메라 매니저")]
        public CameraManager cameraManager;
        [HideInInspector] public SaveDataManager saveDataManager;
        [HideInInspector] public CalculateManager calculateManager;
        [HideInInspector] public MapManager mapManager;
        [HideInInspector] public PopupManager popupManager;
        [HideInInspector] public DamageTextManager damageTextManager;
        [HideInInspector] public ItemManager itemManager;

        private void Awake()
        {
            if (TableLoaderManager.Instance == null)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Intro");
                return;
            }

            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeManagers();

            isStateDirty = false;
            SetState(GameState.Begin);
        }

        private void InitializeManagers()
        {
            GameObject managerContainer = new GameObject("Managers");

            calculateManager = CreateManager<CalculateManager>(managerContainer);
            mapManager = CreateManager<MapManager>(managerContainer);
            popupManager = CreateManager<PopupManager>(managerContainer);
            saveDataManager = CreateManager<SaveDataManager>(managerContainer);
            damageTextManager = CreateManager<DamageTextManager>(managerContainer);
            
            itemManager = new ItemManager();
            itemManager.Initialize();
        }

        private T CreateManager<T>(GameObject parent) where T : Component
        {
            GameObject obj = new GameObject(typeof(T).Name);
            obj.transform.SetParent(parent.transform);
            return obj.AddComponent<T>();
        }

        private void Start()
        {
            if (TableLoaderManager.Instance == null) return;
            mapManager.Initialize(bgBlackForMapLoading);
            mapManager.LoadMap(TableLoaderManager.Instance.TableConfig.GetStartMapUid());
            StartCoroutine(UpdateStateRoutine());
        }

        private IEnumerator UpdateStateRoutine()
        {
            while (true)
            {
                if (isStateDirty)
                {
                    OnStateChanged();
                    isStateDirty = false;
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void OnStateChanged()
        {
            switch (State)
            {
                case GameState.QuestDialogueStart:
                case GameState.QuestDialogueEnd:
                case GameState.Begin:
                case GameState.Combat:
                case GameState.End:
                case GameState.DirectionStart:
                case GameState.DirectionEnd:
                default:
                    break;
            }
        }

        private void SetState(GameState newState)
        {
            State = newState;
            isStateDirty = true;
        }

        public void SetSubState(GameSubState newSubState)
        {
            SubState = newSubState;
            isStateDirty = true;
        }

        public bool IsSubStateDialogueStart => SubState == GameSubState.DialogueStart;
        public bool IsStateDirectionStart => State == GameState.DirectionStart;
    }
}
