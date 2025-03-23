using System;
using System.Collections.Generic;
using System.Linq;
using GGemCo.Scripts.Items;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.SystemMessage;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI;
using GGemCo.Scripts.UI.Window;

namespace GGemCo.Scripts.SaveData
{
    /// <summary>
    /// 세이브 데이터 - 인벤토리 아이템 정보
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

            List<StruckResultIconControl> controls = new List<StruckResultIconControl>();
            // 아이템 이동
            int toItemCount = toItem.ItemCount + moveAmount;
            controls.Add(new StruckResultIconControl(toIndex, toItem.ItemUid, toItemCount));
            
            int fromItemCount = fromItem.ItemCount - moveAmount;

            // fromIndex 슬롯이 비었으면 제거
            controls.Add(fromItemCount <= 0
                ? new StruckResultIconControl(fromIndex, 0, 0)
                : new StruckResultIconControl(fromIndex, fromItem.ItemUid, fromItemCount));

            return new ResultCommon(ResultCommon.Type.Success, $"{moveAmount}개 아이템이 {fromIndex} → {toIndex}로 이동되었습니다.", controls);
        }
        /// <summary>
        /// 인벤토리에 있는 같은 아이템을 작은 인덱스부터 최대 중첩 개수까지 합친다.
        /// </summary>
        public void MergeAllItems()
        {
            Dictionary<int, List<int>> itemSlotGroups = new Dictionary<int, List<int>>();
            Dictionary<int, int> itemSubCategoryMap = new Dictionary<int, int>();
            
            // 1. 기존 아이템 데이터를 백업 (초기화 전에 저장)
            var itemBackup = ItemCounts.ToDictionary(entry => entry.Key, entry => entry.Value);

            // 2. 아이템 정렬을 위해 SubCategory 정보 가져오기
            var sortedItems = itemBackup
                .Where(p => p.Value.ItemUid > 0) // 빈 슬롯 제외
                .Select(p =>
                {
                    var info = TableLoaderManager.Instance.TableItem.GetDataByUid(p.Value.ItemUid);
                    return new
                    {
                        SlotIndex = p.Key,
                        ItemUid = p.Value.ItemUid,
                        ItemCount = p.Value.ItemCount,
                        SubCategory = info.SubCategory == ItemConstants.SubCategory.None ? int.MaxValue : (int)info.SubCategory // SubCategory가 없으면 가장 뒤로 정렬
                    };
                })
                .OrderBy(item => item.SubCategory)  // SubCategory 기준 정렬
                .ThenBy(item => item.SlotIndex)     // 같은 SubCategory 내에서는 슬롯 인덱스 기준 정렬
                .ToList();

            // 3. 정렬된 아이템을 그룹화
            foreach (var item in sortedItems)
            {
                if (!itemSlotGroups.ContainsKey(item.ItemUid))
                {
                    itemSlotGroups[item.ItemUid] = new List<int>();
                    itemSubCategoryMap[item.ItemUid] = item.SubCategory;
                }
                itemSlotGroups[item.ItemUid].Add(item.SlotIndex);
            }

            // 4. 기존 데이터를 확실히 초기화
            ItemCounts.Clear();

            // 5. 각 아이템 그룹별 병합 후 새로운 정렬된 슬롯에 배치
            int newSlotIndex = 0;

            foreach (var group in itemSlotGroups.OrderBy(g => itemSubCategoryMap[g.Key])) // SubCategory 기준으로 병합 순서 결정
            {
                int itemUid = group.Key;
                List<int> slots = group.Value;

                if (slots.Count < 1) continue;

                // 아이템 정보 가져오기
                var info = TableLoaderManager.Instance.TableItem.GetDataByUid(itemUid);
                if (info == null || info.Uid <= 0) continue;

                int maxOverlayCount = info.MaxOverlayCount; // 최대 중첩 개수

                // 기존 백업 데이터에서 총 개수 가져오기
                int totalItemCount = slots.Sum(slot => itemBackup.ContainsKey(slot) ? itemBackup[slot].ItemCount : 0);

                // 6. 병합 후 새로운 슬롯에 재배치
                while (totalItemCount > 0)
                {
                    int addAmount = Math.Min(totalItemCount, maxOverlayCount);
                    ItemCounts[newSlotIndex] = new StructInventoryIcon(itemUid, addAmount);
                    totalItemCount -= addAmount;
                    newSlotIndex++;
                }
            }

            // 7. 변경된 데이터 저장
            SaveItemCounts();
        }
        /// <summary>
        /// 아이템 나누기
        /// </summary>
        /// <param name="slotIndex">인벤토리에 있는 슬롯 index</param>
        /// <param name="itemUid"></param>
        /// <param name="itemCount">원래 가지고 있던 count</param>
        /// <param name="splitItemCount">나누려고 하는 count</param>
        public ResultCommon SplitItem(int slotIndex, int itemUid, int itemCount, int splitItemCount)
        {
            TempItemCounts.Clear();
            int emptySlot = FindEmptySlot();
            if (emptySlot < 0)
            {
                return new ResultCommon(ResultCommon.Type.Fail, "인벤토리에 빈 공간이 없습니다.");
            }
            if (itemUid <= 0)
            {
                return new ResultCommon(ResultCommon.Type.Fail, "나누려고 하는 아이템 정보가 없습니다.");
            }
            if (splitItemCount <= 0)
            {
                return new ResultCommon(ResultCommon.Type.Fail, "나누려고 하는 아이템 개수가 잘 못되었습니다.");
            }
            var info = TableLoaderManager.Instance.TableItem.GetDataByUid(itemUid);
            if (info == null || info.Uid <= 0)
            {
                return new ResultCommon(ResultCommon.Type.Fail, "나누려고 하는 아이템 테이블 정보가 없습니다.");
            }

            List<StruckResultIconControl> controls = new List<StruckResultIconControl>();
            int count = itemCount - splitItemCount;
            if (count <= 0)
            {
                controls.Add(new StruckResultIconControl(slotIndex, 0, 0));
            }
            else {
                controls.Add(new StruckResultIconControl(slotIndex, itemUid, count));
            }
            
            controls.Add(new StruckResultIconControl(emptySlot, itemUid, splitItemCount));
            
            return new ResultCommon(ResultCommon.Type.Success, "", controls); 
        }

    }
}