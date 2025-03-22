using System;
using System.Collections.Generic;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.SystemMessage;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI.Window;

namespace GGemCo.Scripts.SaveData
{
    /// <summary>
    /// 장비 및 인벤토리 데이터를 관리하는 공통 클래스
    /// </summary>
    public abstract class ItemStorageData : DefaultData, ISaveData
    {
        // public 으로 해야 json 으로 저장된다. 
        public Dictionary<int, StructInventoryIcon> ItemCounts = new();
        // add 할때 남은 개수가 max overlay count 를 넘을때 마다 새로운 슬롯에 넣어야 한다.
        // ItemStorageData 에서는 계산만 하고 실제 window 에서 SetIconCount 를 하기 때문에
        // 임시로 저장할 배열이 필요한다.
        // FindEmptySlot 에서 빈 공간을 찾을때 사용한다.
        public Dictionary<int, StructInventoryIcon> TempItemCounts = new();
        private int maxSlotCount = 0;

        private int MaxSlotCount
        {
            get
            {
                if (maxSlotCount <= 0)
                    maxSlotCount = GetMaxSlotCount();
                return maxSlotCount;
            }
        }

        /// <summary>
        /// 아이템 개수 설정
        /// </summary>
        public void SetItemCount(int slotIndex, int itemUid, int value)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlotCount) return;

            ItemCounts[slotIndex] = new StructInventoryIcon(itemUid, value);
            SaveItemCounts();
        }
        /// <summary>
        /// 아이템 개수 빼기 
        /// </summary>
        /// <param name="slotIndex"></param>
        /// <param name="itemUid"></param>
        /// <param name="itemCount"></param>
        /// <returns></returns>
        public ResultCommon MinusItem(int slotIndex, int itemUid, int itemCount)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlotCount)
            {
                return new ResultCommon(ResultCommon.Type.Fail, "잘못된 슬롯 인덱스입니다.");
            }
            var info = TableLoaderManager.Instance.TableItem.GetDataByUid(itemUid);
            if (info == null || info.Uid <= 0)
            {
                return new ResultCommon(ResultCommon.Type.Fail, $"아이템 정보가 없습니다. itemUid: {itemUid}");
            }
            
            if (!ItemCounts.TryGetValue(slotIndex, out var item))
                return new ResultCommon(ResultCommon.Type.Fail, $"아이템 정보가 없습니다. slotIndex: {slotIndex}");
            
            List<StruckResultIconControl> controls = new List<StruckResultIconControl>();
            
            int count = item.ItemCount - itemCount;
            if (count <= 0)
            {
                controls.Add(new StruckResultIconControl(slotIndex, 0, 0));
            }
            else
            {
                controls.Add(new StruckResultIconControl(slotIndex, itemUid, count));
            }

            return new ResultCommon(ResultCommon.Type.Success, $"", controls);
        }
        /// <summary>
        /// 아이템 추가.
        /// </summary>
        public ResultCommon AddItem(int itemUid, int itemCount)
        {
            var info = TableLoaderManager.Instance.TableItem.GetDataByUid(itemUid);
            if (info == null || info.Uid <= 0)
            {
                return new ResultCommon(ResultCommon.Type.Fail, $"아이템 정보가 없습니다. itemUid: {itemUid}");
            }

            int maxOverlayCount = info.MaxOverlayCount;
            int remainingValue = itemCount;

            List<StruckResultIconControl> controls = new List<StruckResultIconControl>();
            // 1. 기존 중첩 가능한 아이템 채우기
            foreach (var structInventoryIcon in ItemCounts)
            {
                var slotIndex = structInventoryIcon.Key;
                var item = structInventoryIcon.Value;
                if (item.ItemUid != itemUid) continue;
                int availableSpace = maxOverlayCount - item.ItemCount;
                if (availableSpace > 0)
                {
                    int addedAmount = Math.Min(remainingValue, availableSpace);
                    int count = item.ItemCount + addedAmount;
                    
                    controls.Add(new StruckResultIconControl(slotIndex, itemUid, count));
                    
                    remainingValue -= addedAmount;
                    if (remainingValue <= 0) break;
                }
            }

            TempItemCounts.Clear();
            // 2. 남은 개수를 새 슬롯에 추가
            while (remainingValue > 0)
            {
                int emptyIndex = FindEmptySlot();
                // todo emptyIndex 에 추가한 걸 FindEmptySlot 에 반영해야 한다 
                if (emptyIndex == -1)
                {
                    return new ResultCommon(ResultCommon.Type.Fail, "인벤토리에 공간이 부족합니다.");
                }

                int addedAmount = Math.Min(remainingValue, maxOverlayCount);
                controls.Add(new StruckResultIconControl(emptyIndex, itemUid, addedAmount));
                TempItemCounts.TryAdd(emptyIndex, new StructInventoryIcon(itemUid, addedAmount));
                remainingValue -= addedAmount;
            }

            SaveItemCounts();
            return new ResultCommon(ResultCommon.Type.Success, "", controls);
        }
        /// <summary>
        /// 특정 슬롯에 아이템 개수 추가
        /// </summary>
        public ResultCommon AddItem(int slotIndex, int itemUid, int itemCount)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlotCount)
            {
                return new ResultCommon(ResultCommon.Type.Fail, "잘못된 슬롯 인덱스입니다.");
            }

            var info = TableLoaderManager.Instance.TableItem.GetDataByUid(itemUid);
            if (info == null || info.Uid <= 0)
            {
                return new ResultCommon(ResultCommon.Type.Fail, $"아이템 정보가 없습니다. itemUid: {itemUid}");
            }
            // 여유 있게 1개의 공간이 남아있는지 체크 
            int emptyIndex = FindEmptySlot();
            if (emptyIndex == -1)
            {
                return new ResultCommon(ResultCommon.Type.Fail, "인벤토리에 공간이 부족합니다.");
            }

            int maxOverlayCount = info.MaxOverlayCount;
            int remainingValue = itemCount;

            List<StruckResultIconControl> controls = new List<StruckResultIconControl>();
            
            // 1. 기존 중첩 가능한 아이템 채우기
            if (ItemCounts.TryGetValue(slotIndex, out var item))
            {
                int availableSpace = maxOverlayCount - item.ItemCount;
                if ((item.ItemUid == itemUid || item.ItemUid == 0) && availableSpace > 0)
                {
                    int addedAmount = Math.Min(remainingValue, availableSpace);
                    int count = item.ItemCount + addedAmount;
                    controls.Add(new StruckResultIconControl(slotIndex, itemUid, count));
                    
                    remainingValue -= addedAmount;
                }
            }
            else
            {
                controls.Add(new StruckResultIconControl(slotIndex, itemUid, itemCount));
                remainingValue -= itemCount;
            }
            
            TempItemCounts.Clear();
            // 2. 남은 개수를 새 슬롯에 추가
            while (remainingValue > 0)
            {
                emptyIndex = FindEmptySlot();
                if (emptyIndex == -1)
                {
                    return new ResultCommon(ResultCommon.Type.Fail, "인벤토리에 공간이 부족합니다.");
                }

                int addedAmount = Math.Min(remainingValue, maxOverlayCount);
                controls.Add(new StruckResultIconControl(emptyIndex, itemUid, addedAmount));
                TempItemCounts.TryAdd(emptyIndex, new StructInventoryIcon(itemUid, addedAmount));
                remainingValue -= addedAmount;
            }
            return new ResultCommon(ResultCommon.Type.Success, "", controls);
        }
        /// <summary>
        /// 특정 슬롯 제거
        /// </summary>
        public void RemoveItemCount(int slotIndex)
        {
            if (!ItemCounts.ContainsKey(slotIndex)) return;
            ItemCounts[slotIndex] = new StructInventoryIcon(0, 0);
            SaveItemCounts();
        }
        /// <summary>
        /// 빈 슬롯 찾기
        /// </summary>
        public int FindEmptySlot()
        {
            for (int i = 0; i < MaxSlotCount; i++)
            {
                if (!ItemCounts.ContainsKey(i) || ItemCounts[i].ItemUid <= 0 || ItemCounts[i].ItemCount <= 0)
                {
                    if (!TempItemCounts.ContainsKey(i) || TempItemCounts[i].ItemUid <= 0 || TempItemCounts[i].ItemCount <= 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 모든 아이템 목록 가져오기
        /// </summary>
        public Dictionary<int, StructInventoryIcon> GetAllItemCounts()
        {
            return ItemCounts;
        }

        protected abstract int GetMaxSlotCount();

        protected void SaveItemCounts()
        {
            SceneGame.Instance.saveDataManager.StartSaveData();
        }
        /// <summary>
        /// 모든 아이템 삭제
        /// </summary>
        public void RemoveAllItems()
        {
            ItemCounts.Clear();
            TempItemCounts.Clear();
        }
    }
}
