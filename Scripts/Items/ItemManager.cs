using System;
using System.Collections.Generic;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

namespace GGemCo.Scripts.Items
{
    public class ItemManager : MonoBehaviour
    {
        [SerializeField] private int poolSize = 20;
        private readonly Queue<SpriteRenderer> poolSprite = new Queue<SpriteRenderer>();
        
        public enum MonsterDropRateType
        {
            None,
            ItemDropGroupUid,
            Nothing
        }
        public enum ItemDropGroup
        {
            None,
            ItemCategory,
            ItemSubCategory,
            ItemUid,
            ExcludeItemUid,
            Nothing,
        }

        private TableItem tableItem;
        private Dictionary<ItemConstants.Category, List<StruckTableItem>> dictionaryByCategory;
        private Dictionary<ItemConstants.SubCategory, List<StruckTableItem>> dictionaryBySubCategory;
        private Dictionary<int, List<StruckTableItemDropGroup>> dropGroupDictionary = new Dictionary<int, List<StruckTableItemDropGroup>>();
        private Dictionary<int, List<StruckTableMonsterDropRate>> monsterDropDictionary = new Dictionary<int, List<StruckTableMonsterDropRate>>();
        private void Awake()
        {
            poolSprite.Clear();
            InitializePool();
        }
        void Start()
        {
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
                for (int i = 0; i < poolSize; i++)
                {
                    GameObject gameObjectText = Instantiate(handle.Result, SceneGame.Instance.itemManager.gameObject.transform);
                    SpriteRenderer spriteRenderer = gameObjectText.GetComponent<SpriteRenderer>();
                    spriteRenderer.gameObject.SetActive(false);
                    poolSprite.Enqueue(spriteRenderer);
                }
            }
            else
            {
                Debug.LogError("Addressables에서 프리팹을 로드하지 못했습니다.");
            }
        }

        private void ShowDropItem(Vector3 worldPosition, int itemUid)
        {
            if (poolSprite.Count == 0)
                return;
            var info = TableLoaderManager.Instance.TableItem.GetDataByUid(itemUid);
            if (info == null) return;
            SpriteRenderer spriteRenderer = poolSprite.Dequeue();
            spriteRenderer.sprite = Resources.Load<Sprite>($"Images/Item/{info.Type.ToString()}/{info.Category.ToString()}/{info.SubCategory.ToString()}/{info.ImagePath}");
            spriteRenderer.transform.position = worldPosition;
            spriteRenderer.gameObject.SetActive(true);
        }

        public void GetDroppedItems(int monsterVid, int monsterUid, GameObject monsterObject)
        {
            if (!monsterDropDictionary.ContainsKey(monsterUid)) return;

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
                return;
            }
            if (!dropGroupDictionary.ContainsKey(groupUid))
                return;
            
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
                        GcLogger.Log("item drop. uid: "+ item.Uid + " / Name: "+item.Name);
                        ShowDropItem(monsterObject.transform.position, item.Uid);
                        return;
                    }
                }
            }
            return;
        }
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
#if UNITY_EDITOR
        public void TestDropRates(int monsterUid, int iterations)
        {
            Dictionary<MonsterDropRateType, int> dropRateCounts = new Dictionary<MonsterDropRateType, int>();
            Dictionary<ItemConstants.Category, int> categoryCounts = new Dictionary<ItemConstants.Category, int>();
            Dictionary<ItemConstants.SubCategory, int> subCategoryCounts = new Dictionary<ItemConstants.SubCategory, int>();
            int totalDrops = 0;

            foreach (MonsterDropRateType type in Enum.GetValues(typeof(MonsterDropRateType)))
            {
                dropRateCounts[type] = 0;
            }
            foreach (ItemConstants.Category category in Enum.GetValues(typeof(ItemConstants.Category)))
            {
                categoryCounts[category] = 0;
            }
            foreach (ItemConstants.SubCategory subCategory in Enum.GetValues(typeof(ItemConstants.SubCategory)))
            {
                subCategoryCounts[subCategory] = 0;
            }

            for (int i = 0; i < iterations; i++)
            {
                // StruckTableItem item = GetDroppedItems(monsterUid);
                //
                // if (item == null)
                // {
                //     dropRateCounts[MonsterDropRateType.Nothing]++;
                // }
                // else
                // {
                //     dropRateCounts[MonsterDropRateType.ItemDropGroupUid]++;
                //     // foreach (var item in droppedItems)
                //     // {
                //         if (Enum.IsDefined(typeof(ItemConstants.Category), item.Category))
                //         {
                //             categoryCounts[item.Category]++;
                //         }
                //         if (Enum.IsDefined(typeof(ItemConstants.SubCategory), item.SubCategory))
                //         {
                //             subCategoryCounts[item.SubCategory]++;
                //         }
                //         totalDrops++;
                //     // }
                // }
            }

            Debug.Log($"Test Results for Monster UID: {monsterUid} ({iterations} iterations)");
            Debug.Log("🔹 MonsterDropRateType Drop Rates:");
            foreach (var entry in dropRateCounts)
            {
                float percentage = (entry.Value / (float)iterations) * 100;
                Debug.Log($"{entry.Key}: {entry.Value} times ({percentage:F2}%)");
            }

            Debug.Log("🔹 Item Category Drop Rates:");
            foreach (var entry in categoryCounts)
            {
                float percentage = (entry.Value / (float)totalDrops) * 100;
                Debug.Log($"{entry.Key}: {entry.Value} times ({percentage:F2}%)");
            }
            Debug.Log("🔹 Item Sub Category Drop Rates:");
            foreach (var entry in subCategoryCounts)
            {
                float percentage = (entry.Value / (float)totalDrops) * 100;
                Debug.Log($"{entry.Key}: {entry.Value} times ({percentage:F2}%)");
            }
        }
#endif
    }
}
