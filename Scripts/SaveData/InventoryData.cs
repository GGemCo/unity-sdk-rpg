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
    /// 인벤토리 아이템 개수 관리
    /// </summary>
    public class InventoryData : ItemStorageData
    {
        /// <summary>
        /// 초기화. Awake 단계에서 실행
        /// </summary>
        /// <param name="loader"></param>
        /// <param name="saveDataContainer"></param>
        public void Initialize(TableLoaderManager loader, SaveDataContainer saveDataContainer = null)
        {
            ItemCounts.Clear();
            if (saveDataContainer?.InventoryData != null)
            {
                ItemCounts = new Dictionary<int, StructInventoryIcon>(saveDataContainer.InventoryData.ItemCounts);
            }
        }

        protected override int GetMaxSlotCount()
        {
            return SceneGame.Instance.uIWindowManager
                .GetUIWindowByUid<UIWindowInventory>(UIWindowManager.WindowUid.Inventory)?.maxCountIcon ?? 0;
        }

        protected override void SaveItemCounts()
        {
            SceneGame.Instance.saveDataManager.StartSaveData();
        }
        
        /// <summary>
        /// 같은 아이템 uid 끼리 합치기 
        /// </summary>
        /// <param name="fromIndex">옮길 아이템이 있는 슬롯</param>
        /// <param name="toIndex">옮길 대상 슬롯</param>
        /// <returns>성공 여부</returns>
        public ResultCommon MergeItem(int fromIndex, int toIndex)
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