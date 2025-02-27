using System.Collections;
using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Core;
using GGemCo.Scripts.Maps;
using GGemCo.Scripts.Popup;
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
        [HideInInspector] public Camera mainCamera;
        
        [HideInInspector] public SaveDataManager saveDataManager;
        [HideInInspector] public CalculateManager calculateManager;
        [HideInInspector] public CameraManager cameraManager;
        [HideInInspector] public UIWindowManager uIWindowManager;
        [HideInInspector] public MapManager mapManager;
        [HideInInspector] public PopupManager popupManager;
        [HideInInspector] public DamageTextManager damageTextManager;

        private Canvas canvasUI;

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

        private void CacheReferences()
        {
            mainCamera = GameObject.FindWithTag(ConfigTags.GetMainCamera())?.GetComponent<Camera>();
            canvasUI = GameObject.FindWithTag(ConfigTags.GetCanvasUI())?.GetComponent<Canvas>();
            cameraManager = GameObject.FindWithTag(ConfigTags.GetMainCamera())?.GetComponent<CameraManager>();
        }

        private void InitializeManagers()
        {
            GameObject managerContainer = new GameObject("Managers");

            calculateManager = CreateManager<CalculateManager>(managerContainer);
            uIWindowManager = CreateManager<UIWindowManager>(managerContainer);
            mapManager = CreateManager<MapManager>(managerContainer);
            popupManager = CreateManager<PopupManager>(managerContainer);
            saveDataManager = CreateManager<SaveDataManager>(managerContainer);
            damageTextManager = CreateManager<DamageTextManager>(managerContainer);
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
            CacheReferences();
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
