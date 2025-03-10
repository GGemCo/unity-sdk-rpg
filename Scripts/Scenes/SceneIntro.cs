using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Core;
using GGemCo.Scripts.SaveData;
using GGemCo.Scripts.ScriptableSettings;
using GGemCo.Scripts.UI.WindowLoadSaveData;
using GGemCo.Scripts.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo.Scripts.Scenes
{
    /// <summary>
    /// 인트로 씬
    /// </summary>
    public class SceneIntro : MonoBehaviour
    {
        public static SceneIntro Instance { get; private set; }
        [HideInInspector] public AddressableSettingsLoader addressableSettingsLoader;
        
        [Header("기본오브젝트")]
        [Tooltip("계속하기 버튼")]
        public Button buttonGameContinue;
        [Tooltip("새로운 게임 버튼")]
        public Button buttonNewGame;
        [Tooltip("불러오기 버튼")]
        public Button buttonOpenSaveDataWindow;
        [Tooltip("옵션 버튼")]
        public Button buttonOption;
        [Tooltip("게임종료 버튼")]
        public Button buttonGameExit;
        [Tooltip("불러오기 window")]
        public UIWindowLoadSaveData uIWindowLoadSaveData;

        [HideInInspector] public int currentSaveDataSlotIndex;
        private SlotMetaDatController slotMetaDatController;
        private SaveFileController saveFileController;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeAddressableSettingLoader();
            LoadCurrentSaveDataSlotIndex();
            InitButtons();
        }
        /// <summary>
        /// PlayerPrefs 에서 진행중인 게임이 있는지 체크
        /// </summary>
        private void LoadCurrentSaveDataSlotIndex()
        {
            PlayerPrefsManager prefsManager = new PlayerPrefsManager();
            currentSaveDataSlotIndex = prefsManager.LoadSaveDataSlotIndex();
        }
        /// <summary>
        /// GGemCo Settings 파일 읽어오기
        /// </summary>
        private void InitializeAddressableSettingLoader()
        {
            GameObject gameObjectAddressableSettingsLoader = new GameObject("AddressableSettingsLoader");
            addressableSettingsLoader = gameObjectAddressableSettingsLoader.AddComponent<AddressableSettingsLoader>();
            _ = addressableSettingsLoader.InitializeAsync();
            addressableSettingsLoader.OnLoadSettings += InitializeSlotMetaDataManager;
        }

        private void OnDestroy()
        {
            addressableSettingsLoader.OnLoadSettings -= InitializeSlotMetaDataManager;
            
            buttonGameContinue?.onClick.RemoveListener(OnClickGameContinue);
            buttonNewGame?.onClick.RemoveListener(OnClickNewGame);
            buttonOpenSaveDataWindow?.onClick.RemoveListener(OnClickOpenSaveDataWindow);
            buttonOption?.onClick.RemoveListener(OnClickOption);
            buttonGameExit?.onClick.RemoveListener(OnClickGameExit);
        }
        /// <summary>
        /// 세이븓 데이터 슬롯 정보를 읽어서 버튼 처리 
        /// </summary>
        private void InitializeSlotMetaDataManager()
        {
            GGemCoSaveSettings saveSettings = addressableSettingsLoader.saveSettings;
            slotMetaDatController = new SlotMetaDatController(saveSettings.SaveDataFolderName, saveSettings.saveDataMaxSlotCount);
            saveFileController = new SaveFileController(saveSettings.SaveDataFolderName, saveSettings.saveDataMaxSlotCount);
            
            // 남은 슬롯 개수를 채크해서 없으면 buttonNewGame 버튼 disable 처리 
            int slotIndex = slotMetaDatController.GetEmptySlot();
            if (slotIndex <= 0)
            {
                buttonNewGame?.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 버튼 초기화. 진행중인 게임이 없을때는 계속하기, 불러오기 버튼은 안보이도록 처리 
        /// </summary>
        private void InitButtons()
        {
            buttonGameContinue?.onClick.AddListener(OnClickGameContinue);
            buttonNewGame?.onClick.AddListener(OnClickNewGame);
            buttonOpenSaveDataWindow?.onClick.AddListener(OnClickOpenSaveDataWindow);
            buttonOption?.onClick.AddListener(OnClickOption);
            buttonGameExit?.onClick.AddListener(OnClickGameExit);
            // 진행중인 게임이 없을때 
            if (currentSaveDataSlotIndex <= 0)
            {
                buttonGameContinue?.gameObject.SetActive(false);
                buttonOpenSaveDataWindow?.gameObject.SetActive(false);
            }
        }
        private void PlayModeStateChanged(PlayModeStateChange state)
        {
            GcLogger.Log("PlayModeStateChanged: "+state);
        }
        
        /// <summary>
        /// 계속 하기
        /// </summary>
        private void OnClickGameContinue()
        {
            // PlayerPrefs 에서 가져온 값이 있는지 체크 
            if (currentSaveDataSlotIndex <= 0) return;
            GcLogger.Log("currentSaveDataSlotIndex: " + currentSaveDataSlotIndex);
            SceneManager.ChangeScene(ConfigDefine.SceneNameLoading);
        }
        /// <summary>
        /// 새로운 게임
        /// </summary>
        private void OnClickNewGame()
        {
            // 남은 슬롯이 있는지 체크
            int slotIndex = slotMetaDatController.GetEmptySlot();
            if (slotIndex <= 0)
            {
                GcLogger.LogError("남은 저장 슬롯이 없습니다. 저장되어있는 데이터를 지워주세요.");
                return;
            }
            GcLogger.Log("slotindex : " + slotIndex);
            
            // 빈 슬롯에 해당하는 디폴트 세이브 파일 만들어 주기
            saveFileController.SaveData(slotIndex, new SaveDataContainer());
            
            // PlayerPrefs 에 저장하기
            PlayerPrefsManager prefsManager = new PlayerPrefsManager();
            prefsManager.SaveSaveDataSlotIndex(slotIndex);
            
            SceneManager.ChangeScene(ConfigDefine.SceneNameLoading);
        }
        /// <summary>
        /// 불러오기
        /// </summary>
        private void OnClickOpenSaveDataWindow()
        {
            if (uIWindowLoadSaveData == null) return;
            uIWindowLoadSaveData.Show(true);
        }
        /// <summary>
        /// 옵션
        /// </summary>
        private void OnClickOption()
        {
        }
        /// <summary>
        /// 게임 종료
        /// </summary>
        private void OnClickGameExit()
        {
            Application.Quit();
        }
    }
}
