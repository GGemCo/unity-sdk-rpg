using System;
using System.Collections.Generic;
using System.Linq;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.SystemMessage;
using GGemCo.Scripts.UI;
using GGemCo.Scripts.UI.Window;
using GGemCo.Scripts.Utils;
using Unity.Plastic.Newtonsoft.Json;

namespace GGemCo.Scripts.SaveData
{
    /// <summary>
    /// 인벤토리 아이템 관리
    /// </summary>
    public class InventoryData : DefaultData, ISaveData
    {
        private Dictionary<int, StructInventoryIcon> itemCounts = new Dictionary<int, StructInventoryIcon>();
        public const string PlayerPrefsKeyInventoryItemCount = "GGemCo_PlayerPrefs_Inventory_Item_Count";
        private int maxCountIcon;
        
        /// <summary>
        /// 초기화. Awake 에서 호출 중
        /// </summary>
        public void Initialize()
        {
            itemCounts.Clear();
            LoadItemCount();
        }

        private void LoadItemCount()
        {
            string itemCountJson = PlayerPrefsLoad(PlayerPrefsKeyInventoryItemCount);
            
            // JSON 파일을 읽기
            try
            {
                if (string.IsNullOrEmpty(itemCountJson)) return;
                itemCounts = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, StructInventoryIcon>>(itemCountJson);
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"Error reading : {ex.Message}");
            }
        }

        private void SaveItemCounts()
        {
            if (itemCounts == null) return;
            string json = JsonConvert.SerializeObject(itemCounts);
            PlayerPrefsSave(PlayerPrefsKeyInventoryItemCount, json);
        }

        public void RemoveItemCount(int slotIndex)
        {
            var item = itemCounts.GetValueOrDefault(slotIndex);
            if (item == null) return;
            item.ItemUid = 0;
            item.ItemCount = 0;
            SaveItemCounts();
        }

        /// <summary>
        /// 아이템 개수 셋팅하기
        /// </summary>
        /// <param name="slotIndex"></param>
        /// <param name="itemUid"></param>
        /// <param name="value">셋팅할 개수</param>
        public void SetItemCount(int slotIndex, int itemUid, int value)
        {
            var item = itemCounts.GetValueOrDefault(slotIndex);
            if (item == null)
            {
                AddItem(slotIndex, itemUid, value);
            }
            else
            {
                item.ItemUid = itemUid;
                item.ItemCount = value;
                SaveItemCounts();
            }
        }
        /// <summary>
        /// 아이템 개수 추가하기
        /// </summary>
        /// <param name="itemUid"></param>
        /// <param name="value">추가할 개수</param>
        public ResultCommon AddItem(int itemUid, int value)
        {
            if (maxCountIcon <= 0)
            {
                UIWindowInventory uiWindowInventory =
                    SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowInventory>(UIWindowManager.WindowUid
                        .Inventory);
                if (uiWindowInventory != null)
                {
                    maxCountIcon = uiWindowInventory.maxCountIcon;
                }
            }
            
            var result = itemCounts.Values.FirstOrDefault(item => item.ItemUid == itemUid);
            if (result != null)
            {
                result.ItemCount += value;
                SaveItemCounts();
            }
            else
            {
                int emptyIndex = -1;
                for (int index = 0; index < maxCountIcon; index++)
                {
                    var item = itemCounts.GetValueOrDefault(index);
                    if (item == null || item.ItemCount <= 0)
                    {
                        emptyIndex = index;
                        break;
                    }
                }
                // 남은 슬롯이 있는지 체크
                if (emptyIndex == -1)
                {
                    return new ResultCommon(ResultCommon.Type.Fail, "인벤토리가 꽉 찾습니다.");
                }
                else
                {
                    AddItem(emptyIndex, itemUid, value);
                }
            }

            return new ResultCommon(ResultCommon.Type.Success);
        }
        /// <summary>
        /// 아이템 추가하기
        /// </summary>
        /// <param name="index">위치</param>
        /// <param name="itemUid">아이템 고유번호</param>
        /// <param name="value">개수</param>
        /// <returns></returns>
        private void AddItem(int index, int itemUid, int value)
        {
            if (itemUid <= 0) return;
            if (!itemCounts.TryGetValue(index, out var icon))
            {
                itemCounts.TryAdd(index, new StructInventoryIcon(itemUid, value));
            }
            else
            {
                icon.ItemUid = itemUid;
                icon.ItemCount += value;
            }

            SaveItemCounts();
        }

        public int GetItemCount(int index, int itemUid)
        {
            return itemCounts[index].ItemCount;
        }
        public int GetItemCount(int itemUid)
        {
            int count = 0;
            foreach (var info in itemCounts)
            {
                if (info.Value.ItemUid == itemUid)
                {
                    count += info.Value.ItemCount;
                }
            }
            return count;
        }

        public Dictionary<int, StructInventoryIcon> GetAllItemCounts()
        {
            return itemCounts;
        }
    }
}