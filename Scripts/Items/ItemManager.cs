﻿using System;
using System.Collections;
using System.Collections.Generic;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GGemCo.Scripts.Items
{
    /// <summary>
    /// 몬스터 사냥 후 드랍되는 아이템 관리 매니저
    /// </summary>
    public class ItemManager
    {
        // 드랍되는 아이템 pool size
        private int poolSize;
        // 드랍되는 아이템 pool queue
        private readonly Queue<Item> poolDropItem = new Queue<Item>();
        // 드랍되는 아이템을 보기 쉽게 하기위한 container 
        private GameObject containerPoolDropItem;
        // 드랍되는 아이템 prefab
        private GameObject prefabDropItem;
        // 드랍되는 아이템 pool size 값을 원래 값으로 초기화 하는 coroutine
        private Coroutine reducePoolCoroutine;
        // 드랍되는 아이템 pool size 값을 원래 값으로 초기화 시간
        private readonly float poolReduceTime = 10f;
        
        public enum MonsterDropRateType
        {
            None,
            ItemDropGroupUid,
            // 아무것도 드랍하지 않는다
            Nothing
        }
        public enum ItemDropGroup
        {
            None,
            ItemCategory,
            ItemSubCategory,
            ItemUid,
            ExcludeItemUid,
            // 아무것도 드랍하지 않는다
            Nothing,
        }

        private TableItem tableItem;
        private Dictionary<ItemConstants.Category, List<StruckTableItem>> dictionaryByCategory;
        private Dictionary<ItemConstants.SubCategory, List<StruckTableItem>> dictionaryBySubCategory;
        private Dictionary<int, List<StruckTableItemDropGroup>> dropGroupDictionary = new Dictionary<int, List<StruckTableItemDropGroup>>();
        private Dictionary<int, List<StruckTableMonsterDropRate>> monsterDropDictionary = new Dictionary<int, List<StruckTableMonsterDropRate>>();

        public void Initialize()
        {
            poolSize = 1;
            poolDropItem.Clear();
            InitializePool();
            tableItem = TableLoaderManager.Instance.TableItem;
            dictionaryByCategory = TableLoaderManager.Instance.TableItem.DictionaryByCategory;
            dictionaryBySubCategory = TableLoaderManager.Instance.TableItem.DictionaryBySubCategory;
            dropGroupDictionary = TableLoaderManager.Instance.TableItemDropGroup.DropGroupDictionary;
            monsterDropDictionary = TableLoaderManager.Instance.TableMonsterDropRate.MonsterDropDictionary;
        }
        /// <summary>
        /// Addressable 에 등록된 damageText 를 불러와서 pool 을 만든다 
        /// </summary>
        private void InitializePool()
        {
            Addressables.LoadAssetAsync<GameObject>(ConfigAddressableKeys.SpriteDropItem).Completed += OnPrefabLoaded;
        }
        private void OnPrefabLoaded(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                containerPoolDropItem = new GameObject("ContainerPoolDropItem");
                prefabDropItem = handle.Result;
                ExpandPool(poolSize); // 초기 풀 생성
            }
            else
            {
                GcLogger.LogError("Addressables에서 프리팹을 로드하지 못했습니다.");
            }
        }
        /// <summary>
        /// 풀에서 아이템을 가져오고, 부족하면 새로운 아이템을 생성한다.
        /// </summary>
        private Item GetOrCreateItem()
        {
            if (poolDropItem.Count == 0)
            {
                ExpandPool(poolSize); // 풀 사이즈만큼 추가 생성
            }
            return poolDropItem.Dequeue();
        }

        /// <summary>
        /// 특정 개수만큼 풀을 확장하여 아이템을 추가 생성.
        /// </summary>
        private void ExpandPool(int amount)
        {
            if (prefabDropItem == null || containerPoolDropItem == null) return;
            for (int i = 0; i < amount; i++)
            {
                GameObject gameObjectText = Object.Instantiate(prefabDropItem, containerPoolDropItem.transform);
                Item item = gameObjectText.GetComponent<Item>();
                item.gameObject.SetActive(false);
                poolDropItem.Enqueue(item);
            }
            // GcLogger.Log($"풀 확장: {amount}개 아이템 추가 (총 {poolDropItem.Count}개)");
        }
        /// <summary>
        /// 아이템 맵에 드랍하기 
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="itemUid"></param>
        private void ShowDropItem(Vector3 worldPosition, int itemUid)
        {
            var info = TableLoaderManager.Instance.TableItem.GetDataByUid(itemUid);
            if (info == null) return;
            Item item = GetOrCreateItem();
            item.itemUid = itemUid;
            item.startPos= worldPosition;
            item.gameObject.SetActive(true);
            
            // 풀을 일정 시간이 지나면 정리하도록 코루틴 시작
            reducePoolCoroutine ??= SceneGame.Instance.StartCoroutine(ReducePoolSize());
        }
        /// <summary>
        /// 일정 시간이 지나면 풀 크기를 다시 poolSize 값으로 줄인다.
        /// </summary>
        private IEnumerator ReducePoolSize()
        {
            yield return new WaitForSeconds(poolReduceTime);

            while (poolDropItem.Count > poolSize)
            {
                Item itemToDestroy = poolDropItem.Dequeue();
                if (itemToDestroy.itemUid > 0) continue;
                Object.Destroy(itemToDestroy.gameObject);
            }

            // GcLogger.Log($"풀 크기 정리 완료: {poolSize}개 유지");
            reducePoolCoroutine = null;
        }
        /// <summary>
        /// 드랍되는 아이템 확률 계산하기 
        /// </summary>
        /// <param name="monsterUid"></param>
        /// <returns></returns>
        private int GetDropItem(int monsterUid)
        {
            if (!monsterDropDictionary.ContainsKey(monsterUid)) return 0;

            Dictionary<MonsterDropRateType, int> dropRates = new Dictionary<MonsterDropRateType, int>();

            // 드롭 확률을 미리 정리
            foreach (StruckTableMonsterDropRate dropEntry in monsterDropDictionary[monsterUid])
            {
                dropRates[dropEntry.Type] = dropEntry.Rate;
            }

            int roll = Random.Range(0, 100);

            // ItemDropGroupUid 체크
            float cumulativePercent = 0f;
            int groupUid = 0;
            if (dropRates.ContainsKey(MonsterDropRateType.ItemDropGroupUid))
            {
                foreach (StruckTableMonsterDropRate dropEntry in monsterDropDictionary[monsterUid])
                {
                    cumulativePercent += dropEntry.Rate;
                    if (roll < cumulativePercent)
                    {
                        groupUid = dropEntry.Value;
                        break;
                    }
                }
            }

            if (groupUid <= 0)
            {
                return 0;
            }
            if (!dropGroupDictionary.ContainsKey(groupUid))
                return 0;
            
            roll = Random.Range(0, 100);
            cumulativePercent = 0f;
            foreach (StruckTableItemDropGroup group in dropGroupDictionary[groupUid])
            {
                cumulativePercent += group.Rate;
                if (roll < cumulativePercent) 
                {
                    StruckTableItem item = FindItemByGroup(group);
                    if (item is { Uid: > 0 })
                    {
                        // GcLogger.Log("item drop. uid: "+ item.Uid + " / Name: "+item.Name);
                        return item.Uid;
                    }
                }
            }
            return 0;
        }
        /// <summary>
        /// 몬스터 죽은 후 드랍 아이템 가져오기
        /// </summary>
        /// <param name="monsterVid"></param>
        /// <param name="monsterUid"></param>
        /// <param name="monsterObject"></param>
        public void OnMonsterDead(int monsterVid, int monsterUid, GameObject monsterObject)
        {
            int itemUid = GetDropItem(monsterUid);
            if (itemUid <= 0) return;
            ShowDropItem(monsterObject.transform.position, itemUid);
        }
        /// <summary>
        /// Item Drop Group 테이블에서 Type 별로 찾아보기 
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private StruckTableItem FindItemByGroup(StruckTableItemDropGroup group)
        {
            switch (group.Type)
            {
                case ItemDropGroup.ItemUid when int.TryParse(group.Value, out var itemUid):
                    return tableItem.GetDataByUid(itemUid);
                case ItemDropGroup.ItemCategory:
                    ItemConstants.Category category = (ItemConstants.Category)Enum.Parse(typeof(ItemConstants.Category), group.Value);
                    return dictionaryByCategory[category][Random.Range(0, dictionaryByCategory[category].Count)];
                case ItemDropGroup.ItemSubCategory:
                {
                    ItemConstants.SubCategory subCategory = (ItemConstants.SubCategory)Enum.Parse(typeof(ItemConstants.SubCategory), group.Value);
                    return dictionaryBySubCategory[subCategory][Random.Range(0, dictionaryBySubCategory[subCategory].Count)];
                }
                case ItemDropGroup.Nothing:
                case ItemDropGroup.ExcludeItemUid:
                case ItemDropGroup.None:
                default:
                    return null;
            }
        }
        /// <summary>
        /// 플레이어가 드랍 아이템을 먹었을때 처리 
        /// </summary>
        public void PlayerTaken(GameObject dropItem)
        {
            Item item = dropItem.GetComponent<Item>();
            if (item ==null || item.itemUid <= 0) return;
            SceneGame.Instance.saveDataManager.Inventory.AddItem(item.itemUid, 1);
            item.Reset();
            poolDropItem.Enqueue(item);
        }
#if UNITY_EDITOR
        /// <summary>
        /// 아이템 드랍 확률 테스트
        /// GGemCoTool 의 TestDropItemRate 에서 사용
        /// </summary>
        public class DropTestResult
        {
            public int MonsterUid;
            public int Iterations;
            public Dictionary<ItemManager.MonsterDropRateType, int> DropRateCounts;
            public Dictionary<ItemConstants.Category, int> CategoryCounts;
            public Dictionary<ItemConstants.SubCategory, int> SubCategoryCounts;
            public int TotalDrops;
        }
        private DropTestResult lastTestResult;
        public DropTestResult TestDropRates(int monsterUid, int iterations, Dictionary<ItemConstants.Category,List<StruckTableItem>> pdictionaryByCategory, Dictionary<ItemConstants.SubCategory,List<StruckTableItem>> pdictionaryBySubCategory, Dictionary<int,List<StruckTableItemDropGroup>> pdropGroupDictionary, Dictionary<int,List<StruckTableMonsterDropRate>> pmonsterDropDictionary, TableItem ptableItem)
        {
            lastTestResult = new DropTestResult
            {
                MonsterUid = monsterUid,
                Iterations = iterations,
                DropRateCounts = new Dictionary<MonsterDropRateType, int>(),
                CategoryCounts = new Dictionary<ItemConstants.Category, int>(),
                SubCategoryCounts = new Dictionary<ItemConstants.SubCategory, int>(),
                TotalDrops = 0
            };
            
            dictionaryByCategory = pdictionaryByCategory;
            dictionaryBySubCategory = pdictionaryBySubCategory;
            dropGroupDictionary = pdropGroupDictionary;
            monsterDropDictionary = pmonsterDropDictionary;
            tableItem = ptableItem;
            
            foreach (MonsterDropRateType type in System.Enum.GetValues(typeof(MonsterDropRateType)))
            {
                lastTestResult.DropRateCounts[type] = 0;
            }
            foreach (ItemConstants.Category category in System.Enum.GetValues(typeof(ItemConstants.Category)))
            {
                lastTestResult.CategoryCounts[category] = 0;
            }
            foreach (ItemConstants.SubCategory subCategory in System.Enum.GetValues(typeof(ItemConstants.SubCategory)))
            {
                lastTestResult.SubCategoryCounts[subCategory] = 0;
            }

            for (int i = 0; i < iterations; i++)
            {
                int itemUid = GetDropItem(monsterUid);

                if (itemUid <= 0)
                {
                    lastTestResult.DropRateCounts[MonsterDropRateType.Nothing]++;
                }
                else
                {
                    lastTestResult.DropRateCounts[MonsterDropRateType.ItemDropGroupUid]++;
                    var info = tableItem.GetDataByUid(itemUid);
                    if (info == null) continue;
                
                    if (System.Enum.IsDefined(typeof(ItemConstants.Category), info.Category))
                    {
                        lastTestResult.CategoryCounts[info.Category]++;
                    }
                    if (System.Enum.IsDefined(typeof(ItemConstants.SubCategory), info.SubCategory))
                    {
                        lastTestResult.SubCategoryCounts[info.SubCategory]++;
                    }
                    lastTestResult.TotalDrops++;
                }
            }
            // GcLogger.Log($"테스트 완료: 몬스터 UID {monsterUid}, {iterations}회 실행됨.");
            return lastTestResult;
        }
#endif
    }
}
