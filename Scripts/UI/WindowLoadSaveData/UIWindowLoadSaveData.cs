using System.Collections.Generic;
using System.IO;
using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Core;
using GGemCo.Scripts.SaveData;
using GGemCo.Scripts.Scenes;
using UnityEngine;

namespace GGemCo.Scripts.UI.WindowLoadSaveData
{
    public class UIWindowLoadSaveData : MonoBehaviour
    {
        [Header("기본오브젝트")]
        [Tooltip("세이브 데이터를 보여줄 슬롯 Prefab")]
        public GameObject elementSaveDataSlot;
        [Tooltip("슬롯 프리팹이 들어갈 Panel")]
        public GameObject containerelementSaveDataSlot;

        private int currentCheckSlotIndex;
        private List<UIElementSaveDataSlot> uiElementSaveDataSlots;
        private AddressableSettingsLoader addressableSettingsLoader;

        private void Awake()
        {
            uiElementSaveDataSlots = new List<UIElementSaveDataSlot>();
            currentCheckSlotIndex = 0;
        }
        
        private void Start()
        {
            if (SceneIntro.Instance == null) return;
            addressableSettingsLoader = SceneIntro.Instance.addressableSettingsLoader;
            addressableSettingsLoader.OnLoadSettings += InitializeSaveDataSlots;
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            PlayerPrefsManager prefsManager = new PlayerPrefsManager();
            currentCheckSlotIndex = prefsManager.LoadSaveDataSlotIndex();
            SetSelectElement(currentCheckSlotIndex);
        }

        private SaveFileController saveFileController;
        private ThumbnailController thumbnailController;
        private SlotMetaDatController slotMetaDatController;
        private void InitializeSaveDataSlots()
        {
            if (elementSaveDataSlot == null || containerelementSaveDataSlot == null) return;
            int maxSlotCount = addressableSettingsLoader.saveSettings.saveDataMaxSlotCount;
            string saveDirectory = addressableSettingsLoader.saveSettings.SaveDataFolderName;
            
            int thumbnailWidth = addressableSettingsLoader.saveSettings.saveDataThumnailWidth;
            string thumbnailDirectory =addressableSettingsLoader.saveSettings.SaveDataThumnailFolderName;
            
            slotMetaDatController = new SlotMetaDatController(saveDirectory, maxSlotCount);
            saveFileController = new SaveFileController(saveDirectory, maxSlotCount);
            thumbnailController = new ThumbnailController(thumbnailDirectory, thumbnailWidth);
            
            List<SlotMetaInfo> slotMetaInfos = slotMetaDatController.GetMetaDataSlots();
            
            for (int i = 0; i < maxSlotCount; i++)
            {
                GameObject slot = Instantiate(elementSaveDataSlot, containerelementSaveDataSlot.transform);
                if (slot == null) continue;
                UIElementSaveDataSlot uiElementSaveDataSlot = slot.GetComponent<UIElementSaveDataSlot>();
                if (uiElementSaveDataSlot == null) continue;
                uiElementSaveDataSlots.Add(uiElementSaveDataSlot);
                SlotMetaInfo slotMetaInfo = slotMetaInfos[i];
                if (slotMetaInfo != null)
                {
                    bool isCheck = slotMetaInfo.SlotIndex == SceneIntro.Instance.currentSaveDataSlotIndex;
                    if (isCheck)
                    {
                        currentCheckSlotIndex = slotMetaInfo.SlotIndex;
                    }
                    uiElementSaveDataSlot.Initialize(slotMetaInfo, thumbnailController, isCheck);
                }
            }
        }
        private void OnDestroy()
        {
            if (SceneIntro.Instance == null) return;
            addressableSettingsLoader.OnLoadSettings -= InitializeSaveDataSlots;
        }

        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }

        public void OnClickClose()
        {
            Show(false);
        }
        /// <summary>
        /// 불러오기
        /// </summary>
        public void OnClickLoad()
        {
            if (currentCheckSlotIndex <= 0) return;
            PlayerPrefsManager playerPrefsManager = new PlayerPrefsManager();
            playerPrefsManager.SaveSaveDataSlotIndex(currentCheckSlotIndex);
            SceneManager.ChangeScene(ConfigDefine.SceneNameLoading);
        }
        /// <summary>
        /// 삭제하기
        /// </summary>
        public void OnClickDelete()
        {
            if (currentCheckSlotIndex <= 0) return;
            
            string filePath = saveFileController.GetSaveFilePath(currentCheckSlotIndex);
            string thumbnailPath = thumbnailController.GetThumbnailPath(currentCheckSlotIndex);

            if (File.Exists(filePath)) File.Delete(filePath);
            if (File.Exists(thumbnailPath)) File.Delete(thumbnailPath);
            
            slotMetaDatController.DeleteSlot(currentCheckSlotIndex);
        }
        /// <summary>
        /// element 선택처리 하기  
        /// </summary>
        /// <param name="slotIndex"></param>
        public void SetSelectElement(int slotIndex)
        {
            if (uiElementSaveDataSlots.Count <= 0) return;
            if (slotIndex <= 0) return;
            // 체크 아이콘 모두 off
            foreach (var slot in uiElementSaveDataSlots)
            {
                slot.SetIconCheck(false);
            }
            // 선택한것만 체크 아이콘 on
            currentCheckSlotIndex = slotIndex;
            uiElementSaveDataSlots[slotIndex-1]?.SetIconCheck(true);
        }
    }
}