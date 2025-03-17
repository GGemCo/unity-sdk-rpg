using System;
using System.Collections.Generic;
using System.Linq;
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
        public Dictionary<int, StructInventoryIcon> ItemCounts = new Dictionary<int, StructInventoryIcon>();
        protected int maxCountIcon = 0;

        /// <summary>
        /// 아이템 개수 설정
        /// </summary>
        public void SetItemCount(int slotIndex, int itemUid, int value)
        {
            // 아이템을 빈 슬롯으로 이동했을때 itemUid, value 값이 0 이 올 수 있다.
            if (!ItemCounts.TryGetValue(slotIndex, out var item))
            {
                ItemCounts[slotIndex] = new StructInventoryIcon(itemUid, value);
            }
            else
            {
                item.ItemUid = itemUid;
                item.ItemCount = value;
            }

            SaveItemCounts();
        }

        /// <summary>
        /// 아이템 개수 추가
        /// </summary>
        public ResultCommon AddItem(int itemUid, int itemCount)
        {
            if (maxCountIcon <= 0)
            {
                maxCountIcon = GetMaxSlotCount();
            }

            var info = TableLoaderManager.Instance.TableItem.GetDataByUid(itemUid);
            if (info == null || info.Uid <= 0)
            {
                return new ResultCommon(ResultCommon.Type.Fail, $"아이템 정보가 없습니다. itemUid: {itemUid}");
            }

            int maxOverlayCount = info.MaxOverlayCount;
            int remainingValue = itemCount;

            // 1. 기존 중첩 가능한 아이템 채우기
            foreach (var item in ItemCounts.Values.Where(i => i.ItemUid == itemUid))
            {
                int availableSpace = maxOverlayCount - item.ItemCount;
                if (availableSpace > 0)
                {
                    int addedAmount = Math.Min(remainingValue, availableSpace);
                    item.ItemCount += addedAmount;
                    remainingValue -= addedAmount;
                    if (remainingValue <= 0) break;
                }
            }

            // 2. 남은 개수를 새 슬롯에 추가
            while (remainingValue > 0)
            {
                int emptyIndex = FindEmptySlot();
                if (emptyIndex == -1)
                {
                    return new ResultCommon(ResultCommon.Type.Fail, "인벤토리에 공간이 부족합니다.");
                }

                int addedAmount = Math.Min(remainingValue, maxOverlayCount);
                SetItemCount(emptyIndex, itemUid, addedAmount);
                remainingValue -= addedAmount;
            }

            SaveItemCounts();
            return new ResultCommon(ResultCommon.Type.Success);
        }

        /// <summary>
        /// 특정 슬롯에 아이템 개수를 추가
        /// </summary>
        public ResultCommon AddItemToSlot(int slotIndex, int itemUid, int value)
        {
            if (maxCountIcon <= 0)
            {
                maxCountIcon = GetMaxSlotCount();
            }
            if (slotIndex < 0 || slotIndex >= maxCountIcon)
            {
                return new ResultCommon(ResultCommon.Type.Fail, "잘못된 슬롯 인덱스입니다.");
            }

            var info = TableLoaderManager.Instance.TableItem.GetDataByUid(itemUid);
            if (info == null || info.Uid <= 0)
            {
                return new ResultCommon(ResultCommon.Type.Fail, $"아이템 정보가 없습니다. itemUid: {itemUid}");
            }

            int maxOverlayCount = info.MaxOverlayCount;

            if (ItemCounts.TryGetValue(slotIndex, out var item))
            {
                // 같은 아이템이면 개수 증가
                if (item.ItemUid == 0 || item.ItemUid == itemUid)
                {
                    int availableSpace = maxOverlayCount - item.ItemCount;
                    int addedAmount = Math.Min(value, availableSpace);
                    item.ItemUid = itemUid;
                    item.ItemCount += addedAmount;
                    if (item.ItemCount <= 0)
                    {
                        RemoveItemCount(slotIndex);
                    }
                    SaveItemCounts();

                    int remaining = value - addedAmount;
                    return remaining > 0 
                        ? AddItem(itemUid, remaining)
                        : new ResultCommon(ResultCommon.Type.Success, "아이템 추가 성공");
                }
                else
                {
                    return new ResultCommon(ResultCommon.Type.Fail, "다른 아이템이 있는 슬롯입니다.");
                }
            }
            else
            {
                // 빈 슬롯이면 새 아이템 추가
                int addedAmount = Math.Min(value, maxOverlayCount);
                ItemCounts[slotIndex] = new StructInventoryIcon(itemUid, addedAmount);
                SaveItemCounts();

                int remaining = value - addedAmount;
                return remaining > 0 
                    ? AddItem(itemUid, remaining) 
                    : new ResultCommon(ResultCommon.Type.Success, "아이템 추가 성공");
            }
        }
        
        /// <summary>
        /// 특정 슬롯 제거
        /// </summary>
        public void RemoveItemCount(int slotIndex)
        {
            if (ItemCounts.ContainsKey(slotIndex))
            {
                ItemCounts[slotIndex] = new StructInventoryIcon(0, 0);
                SaveItemCounts();
            }
        }

        /// <summary>
        /// 빈 슬롯 찾기
        /// </summary>
        public int FindEmptySlot()
        {
            if (maxCountIcon <= 0)
            {
                maxCountIcon = GetMaxSlotCount();
            }
            for (int i = 0; i < maxCountIcon; i++)
            {
                if (!ItemCounts.ContainsKey(i) || ItemCounts[i].ItemUid <= 0 || ItemCounts[i].ItemCount <= 0)
                {
                    return i;
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
        protected abstract void SaveItemCounts();
    }
}
