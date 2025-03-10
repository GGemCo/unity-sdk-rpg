using System.IO;
using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Core;
using GGemCo.Scripts.ScriptableSettings;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace GGemCo.Scripts.SaveData
{
    /// <summary>
    /// 저장할 데이터 컨테이너 클래스
    /// </summary>
    public class SaveDataContainer
    {
        // public StruckPlayerSaveData StruckPlayerSaveData;
        public PlayerData PlayerData;
        public InventoryData InventoryData;
        public QuestData QuestData;
    }
    /// <summary>
    /// 세이브 데이터 메인 매니저
    /// </summary>
    public class SaveDataManager : MonoBehaviour
    {
        public PlayerData Player { get; private set; }
        public InventoryData Inventory { get; private set; }
        public QuestData Quest { get; private set; }

        private TableLoaderManager tableLoaderManager;
        private SlotMetaDatController slotMetaDatController;
        private SaveFileController saveFileController;
        private ThumbnailController thumbnailController;
        
        // 최대 저장 슬롯 개수
        private int maxSaveSlotCount;
        // 썸네일 width 
        private int thumbnailWidth;
        // 게임 데이터 저장 경로
        private string saveDirectory;
        // 썸네일 저장 경로
        private string thumbnailDirectory;

        // 이 시간안에 저장 요청이 오면 기존 요청은 취소된다.
        private float saveDelay;
        // 강제로 저장할 시간
        private float forceSaveInterval;
        // 마지막 저장된 시간
        private float lastSaveTime;
        // 현재 진행중인 slot index
        private int currentSaveSlot;

        private void Awake()
        {
            tableLoaderManager = TableLoaderManager.Instance;
            if (tableLoaderManager == null) return;

            InitializeSaveDirectory();
            InitializeControllerAndData();
        }
        /// <summary>
        /// 기본 정보를 GGemCo Settings 에서 불러온다.
        /// </summary>
        private void InitializeSaveDirectory()
        {
            GGemCoSaveSettings saveSettings = AddressableSettingsLoader.Instance.saveSettings;
            saveDelay = saveSettings.saveDataDelay;
            forceSaveInterval = saveSettings.saveDataForceSaveInterval;
            thumbnailWidth = saveSettings.saveDataThumnailWidth;
            maxSaveSlotCount = saveSettings.saveDataMaxSlotCount;

            saveDirectory = saveSettings.SaveDataFolderName;
            thumbnailDirectory = saveSettings.SaveDataThumnailFolderName;
            Directory.CreateDirectory(saveDirectory);
            Directory.CreateDirectory(thumbnailDirectory);
        }
        /// <summary>
        /// 슬롯 관리, 파일 관리, 썸네일 관리 매니저 초기화
        /// </summary>
        private void InitializeControllerAndData()
        {
            slotMetaDatController = new SlotMetaDatController(saveDirectory, maxSaveSlotCount);
            saveFileController = new SaveFileController(saveDirectory, maxSaveSlotCount);
            thumbnailController = new ThumbnailController(thumbnailDirectory, thumbnailWidth);
            // 각 데이터 클래스 초기화
            Player = new PlayerData();
            Inventory = new InventoryData();
            Quest = new QuestData();

            PlayerPrefsManager prefsManager = new PlayerPrefsManager();
            currentSaveSlot = prefsManager.LoadSaveDataSlotIndex();
            
            // 로드한 세이브 데이터 가져오기 
            SaveDataContainer saveDataContainer = SaveDataLoader.Instance.GetSaveDataContainer();
            // 데이터가 없으면 강제로 한번 저장하기 
            if (saveDataContainer == null)
            {
                SaveData();
            }

            // 초기화 실행
            Player.Initialize(tableLoaderManager, saveDataContainer);
            Inventory.Initialize(tableLoaderManager, saveDataContainer);
            Quest.Initialize(tableLoaderManager, saveDataContainer);
            
            // 로드한 세이브 데이터 가져온 후 Loader 는 삭제 처리 
            Destroy(SaveDataLoader.Instance.gameObject);
        }
        private void Start()
        {
            lastSaveTime = Time.time;
            // 강제 저장 시작 
            InvokeRepeating(nameof(ForceSave), forceSaveInterval, forceSaveInterval);
        }
        /// <summary>
        /// 저장하기 시작
        /// </summary>
        public void StartSaveData()
        {
            CancelInvoke(nameof(SaveData));
            Invoke(nameof(SaveData), saveDelay);
        }
        /// <summary>
        /// 강제 저장하기
        /// </summary>
        private void ForceSave()
        {
            if (Time.time - lastSaveTime >= forceSaveInterval)
            {
                // GcLogger.Log("강제 저장");
                SaveData();
            }
        }

        /// <summary>
        /// 현재 데이터를 선택한 슬롯에 저장 + 메타파일 업데이트
        /// </summary>
        public void SaveData()
        {
            if (currentSaveSlot < 1 || currentSaveSlot > maxSaveSlotCount)
            {
                GcLogger.LogError("잘못된 슬롯 번호입니다.");
                return;
            }

            string filePath = saveFileController.GetSaveFilePath(currentSaveSlot);
            string thumbnailFileName = $"Thumbnail_{currentSaveSlot}.png";
            
            SaveDataContainer saveData = new SaveDataContainer
            {
                PlayerData = Player,
                InventoryData = Inventory,
            };

            string json = JsonConvert.SerializeObject(saveData);
            File.WriteAllText(filePath, json);
            GcLogger.Log($"데이터가 저장되었습니다. 슬롯 {currentSaveSlot}");
            
            // 썸네일 캡처 후 저장
            if (thumbnailWidth > 0)
            {
                StartCoroutine(thumbnailController.CaptureThumbnail(currentSaveSlot));
            }
            
            // 메타파일 업데이트
            slotMetaDatController.UpdateSlot(currentSaveSlot, thumbnailFileName, true, Player.CurrentLevel);
        }
        /// <summary>
        /// 슬롯 삭제 + 메타파일 업데이트
        /// </summary>
        public void DeleteData(int slot)
        {
            string filePath = saveFileController.GetSaveFilePath(slot);
            string thumbnailPath = thumbnailController.GetThumbnailPath(slot);

            if (File.Exists(filePath)) File.Delete(filePath);
            if (File.Exists(thumbnailPath)) File.Delete(thumbnailPath);
            
            slotMetaDatController.DeleteSlot(slot);
        }
    }
}