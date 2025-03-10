using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;

namespace GGemCo.Scripts.SaveData
{
    public class SlotMetaInfo
    {
        public int SlotIndex;
        public string SaveTime;
        public int Level;
        public string ThumbnailFileName;
        public bool Exists;
    }
    /// <summary>
    /// 슬롯 메타데이터 구조
    /// </summary>
    public class SaveMetaData
    {
        public readonly List<SlotMetaInfo> Slots;

        public SaveMetaData(int maxSlots)
        {
            Slots = new List<SlotMetaInfo>();
            for (int i = 1; i <= maxSlots; i++)
            {
                Slots.Add(new SlotMetaInfo { SlotIndex = i, Level = 0, SaveTime = "", ThumbnailFileName = "", Exists = false });
            }
        }
    }
    /// <summary>
    /// 슬롯 및 메타데이터 관리 클래스
    /// </summary>
    public class SlotMetaDatController
    {
        private readonly string metaFilePath;
        private SaveMetaData MetaData { get; set; }

        public SlotMetaDatController(string saveDirectory, int maxSlots)
        {
            metaFilePath = Path.Combine(saveDirectory, "SaveMeta.json");

            // 메타파일이 없으면 기본 데이터 생성
            if (!File.Exists(metaFilePath))
            {
                Directory.CreateDirectory(saveDirectory);
                MetaData = new SaveMetaData(maxSlots);
                SaveMetaToFile();
            }
            else
            {
                MetaData = LoadMetaData();
            }
        }

        /// <summary>
        /// 슬롯의 정보를 업데이트하고 저장
        /// </summary>
        public void UpdateSlot(int slot, string thumbnailFileName, bool exists, int level)
        {
            string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var slotInfo = MetaData.Slots.Find(s => s.SlotIndex == slot);
            if (slotInfo != null)
            {
                slotInfo.SaveTime = saveTime;
                slotInfo.ThumbnailFileName = thumbnailFileName;
                slotInfo.Exists = exists;
                slotInfo.Level = level;
                SaveMetaToFile();
            }
        }

        /// <summary>
        /// 특정 슬롯 삭제 처리
        /// </summary>
        public void DeleteSlot(int slot)
        {
            UpdateSlot(slot, "", false, 0);
        }

        /// <summary>
        /// 메타데이터를 파일에 저장
        /// </summary>
        private void SaveMetaToFile()
        {
            string json = JsonConvert.SerializeObject(MetaData);
            File.WriteAllText(metaFilePath, json);
        }

        /// <summary>
        /// 메타데이터를 파일에서 로드
        /// </summary>
        private SaveMetaData LoadMetaData()
        {
            string json = File.ReadAllText(metaFilePath);
            return JsonConvert.DeserializeObject<SaveMetaData>(json);
        }
        /// <summary>
        /// 비어 있는 슬롯 index 가져오기 
        /// </summary>
        /// <returns></returns>
        public int GetEmptySlot()
        {
            return (from slotMetaInfo in MetaData.Slots where slotMetaInfo.Exists == false select slotMetaInfo.SlotIndex).FirstOrDefault();
        }
        /// <summary>
        /// 저장되어있는 메타 데이터 리스트 가져오기
        /// </summary>
        /// <returns></returns>
        public List<SlotMetaInfo> GetMetaDataSlots()
        {
            return MetaData.Slots;
        }
    }
}
