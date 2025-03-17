using System;
using System.Collections.Generic;
using System.Linq;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.SystemMessage;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI;
using GGemCo.Scripts.UI.Window;

namespace GGemCo.Scripts.SaveData
{
    /// <summary>
    /// 인벤토리 아이템 관리
    /// </summary>
    public class InventoryData : DefaultData, ISaveData
    {
        public Dictionary<int, StructInventoryIcon> ItemCounts = new Dictionary<int, StructInventoryIcon>();
        private int maxCountIcon;
        
        /// <summary>
        /// 초기화. Awake 에서 호출 중
        /// </summary>
        public void Initialize(TableLoaderManager loader, SaveDataContainer saveDataContainer = null)
        {
            ItemCounts.Clear();
            LoadItemCount(saveDataContainer);
        }

        private void LoadItemCount(SaveDataContainer saveDataContainer)
        {
            if (saveDataContainer?.InventoryData == null) return;
            ItemCounts.Clear();
            foreach (var info in saveDataContainer.InventoryData.ItemCounts)
            {
                ItemCounts.TryAdd(info.Key, info.Value);
            }
        }

        private void SaveItemCounts()
        {
            if (ItemCounts == null) return;
            SceneGame.Instance.saveDataManager.StartSaveData();
        }

        public void RemoveItemCount(int slotIndex)
        {
            var item = ItemCounts.GetValueOrDefault(slotIndex);
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
            var item = ItemCounts.GetValueOrDefault(slotIndex);
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

            // 아이템 정보 가져오기
            var info = TableLoaderManager.Instance.TableItem.GetDataByUid(itemUid);
            if (info == null || info.Uid <= 0)
            {
                return new ResultCommon(ResultCommon.Type.Fail, $"아이템 정보가 없습니다. itemUid: {itemUid}");
            }

            int maxOverlayCount = info.MaxOverlayCount; // 최대 중첩 개수
            int totalNeededSlots = 0; // 추가할 아이템을 위해 필요한 슬롯 개수

            // 1. 기존 아이템 중 중첩 가능한 공간이 있는지 확인
            int remainingValue = value;
            foreach (var item in ItemCounts.Values.Where(i => i.ItemUid == itemUid))
            {
                int availableSpace = maxOverlayCount - item.ItemCount;
                if (availableSpace > 0)
                {
                    int addedAmount = Math.Min(remainingValue, availableSpace);
                    remainingValue -= addedAmount;
                    if (remainingValue <= 0) break; // 모든 아이템이 추가되면 종료
                }
            }

            // 2. 남은 개수를 저장할 새로운 슬롯이 필요한 경우 개수 확인
            while (remainingValue > 0)
            {
                totalNeededSlots++;
                remainingValue -= maxOverlayCount;
            }

            // 3. 현재 남은 슬롯 개수 확인
            int emptySlotCount = 0;
            for (int i = 0; i < maxCountIcon; i++)
            {
                var item = ItemCounts.GetValueOrDefault(i);
                if (item == null || item.ItemCount <= 0)
                {
                    emptySlotCount++;
                }
            }

            // 4. 필요한 슬롯이 부족하면 실패 반환
            if (emptySlotCount < totalNeededSlots)
            {
                return new ResultCommon(ResultCommon.Type.Fail, "인벤토리에 공간이 부족합니다.");
            }

            // 5. 아이템 추가 (중첩 가능한 경우 먼저 추가)
            remainingValue = value;
            foreach (var item in ItemCounts.Values.Where(i => i.ItemUid == itemUid))
            {
                int availableSpace = maxOverlayCount - item.ItemCount;
                if (availableSpace > 0)
                {
                    int addedAmount = Math.Min(remainingValue, availableSpace);
                    item.ItemCount += addedAmount;
                    remainingValue -= addedAmount;
                    if (remainingValue <= 0) break; // 모든 아이템이 추가되면 종료
                }
            }

            // 6. 남은 개수를 새로운 슬롯에 추가
            while (remainingValue > 0)
            {
                int emptyIndex = FindEmptySlot();
                if (emptyIndex == -1) break; // 공간이 없으면 종료 (이론상 발생하지 않음)

                int addedAmount = Math.Min(remainingValue, maxOverlayCount);
                AddItem(emptyIndex, itemUid, addedAmount);
                remainingValue -= addedAmount;
            }

            SaveItemCounts();
            return new ResultCommon(ResultCommon.Type.Success);
        }
        // 빈 슬롯 찾기
        private int FindEmptySlot()
        {
            for (int i = 0; i < maxCountIcon; i++)
            {
                var item = ItemCounts.GetValueOrDefault(i);
                if (item == null || item.ItemCount <= 0)
                {
                    return i;
                }
            }
            return -1; // 빈 슬롯 없음
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
            if (!ItemCounts.TryGetValue(index, out var icon))
            {
                ItemCounts.TryAdd(index, new StructInventoryIcon(itemUid, value));
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
            return ItemCounts[index].ItemCount;
        }
        public int GetItemCount(int itemUid)
        {
            int count = 0;
            foreach (var info in ItemCounts)
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
            return ItemCounts;
        }
        /// <summary>
        /// 특정 슬롯의 아이템을 다른 슬롯으로 합친다.
        /// </summary>
        /// <param name="fromIndex">옮길 아이템이 있는 슬롯</param>
        /// <param name="toIndex">옮길 대상 슬롯</param>
        /// <returns>성공 여부</returns>
        public ResultCommon MoveItemToSlot(int fromIndex, int toIndex)
        {
            // 이동할 슬롯과 대상 슬롯이 동일하면 무시
            if (fromIndex == toIndex)
            {
                return new ResultCommon(ResultCommon.Type.Fail, "같은 슬롯으로 이동할 수 없습니다.");
            }

            // fromIndex 아이템 존재 확인
            if (!ItemCounts.TryGetValue(fromIndex, out var fromItem) || fromItem.ItemCount <= 0)
            {
                return new ResultCommon(ResultCommon.Type.Fail, $"이동할 아이템이 없습니다. 슬롯: {fromIndex}");
            }

            // toIndex 아이템 존재 확인
            if (!ItemCounts.TryGetValue(toIndex, out var toItem) || toItem.ItemCount <= 0)
            {
                return new ResultCommon(ResultCommon.Type.Fail, $"대상 슬롯에 아이템이 없습니다. 슬롯: {toIndex}");
            }

            // 같은 아이템인지 확인
            if (fromItem.ItemUid != toItem.ItemUid)
            {
                return new ResultCommon(ResultCommon.Type.Fail, "다른 종류의 아이템은 합칠 수 없습니다.");
            }

            // 아이템 정보 가져오기 (최대 중첩 개수 확인)
            var info = TableLoaderManager.Instance.TableItem.GetDataByUid(fromItem.ItemUid);
            if (info == null || info.Uid <= 0)
            {
                return new ResultCommon(ResultCommon.Type.Fail, $"아이템 정보가 없습니다. itemUid: {fromItem.ItemUid}");
            }

            int maxOverlayCount = info.MaxOverlayCount; // 최대 중첩 개수
            int availableSpace = maxOverlayCount - toItem.ItemCount; // toIndex 슬롯의 남은 공간

            // 합칠 공간이 없는 경우
            if (availableSpace <= 0)
            {
                return new ResultCommon(ResultCommon.Type.Fail, "대상 슬롯에 합칠 공간이 없습니다.");
            }

            // 이동 가능한 개수 계산
            int moveAmount = Math.Min(fromItem.ItemCount, availableSpace);

            // 아이템 이동
            toItem.ItemCount += moveAmount;
            fromItem.ItemCount -= moveAmount;

            // fromIndex 슬롯이 비었으면 제거
            if (fromItem.ItemCount <= 0)
            {
                // ItemCounts.Remove(fromIndex);
                RemoveItemCount(fromIndex);
            }

            SaveItemCounts();
            return new ResultCommon(ResultCommon.Type.Success, $"{moveAmount}개 아이템이 {fromIndex} → {toIndex}로 이동되었습니다.");
        }
        /// <summary>
        /// 인벤토리에 있는 같은 아이템을 작은 인덱스부터 최대 중첩 개수까지 합친다.
        /// </summary>
        public void MergeAllItems()
        {
            Dictionary<int, List<int>> itemSlotGroups = new Dictionary<int, List<int>>();

            // 1. 같은 아이템을 가진 슬롯 그룹화 (인덱스 순 정렬)
            foreach (var pair in ItemCounts.OrderBy(p => p.Key))
            {
                int itemUid = pair.Value.ItemUid;
                if (itemUid <= 0) continue; // 빈 슬롯 무시

                if (!itemSlotGroups.ContainsKey(itemUid))
                {
                    itemSlotGroups[itemUid] = new List<int>();
                }
                itemSlotGroups[itemUid].Add(pair.Key);
            }

            // 2. 각 아이템 그룹별 병합 진행
            foreach (var group in itemSlotGroups)
            {
                int itemUid = group.Key;
                List<int> slots = group.Value;

                if (slots.Count < 2) continue; // 병합할 필요 없음

                // 아이템 정보 가져오기
                var info = TableLoaderManager.Instance.TableItem.GetDataByUid(itemUid);
                if (info == null || info.Uid <= 0) continue;

                int maxOverlayCount = info.MaxOverlayCount; // 최대 중첩 개수
                int totalItemCount = slots.Sum(slot => ItemCounts[slot].ItemCount); // 전체 개수
        
                // 3. 작은 인덱스부터 채우기
                for (int i = 0; i < slots.Count; i++)
                {
                    int slot = slots[i];

                    // 현재 슬롯에 넣을 개수
                    int addAmount = Math.Min(totalItemCount, maxOverlayCount);
                    ItemCounts[slot].ItemCount = addAmount;
                    totalItemCount -= addAmount;

                    // 남은 개수가 없으면 이후 슬롯 제거
                    if (totalItemCount <= 0)
                    {
                        for (int j = i + 1; j < slots.Count; j++)
                        {
                            RemoveItemCount(slots[j]);
                        }
                        break;
                    }
                }
            }

            SaveItemCounts();
        }
    }
}