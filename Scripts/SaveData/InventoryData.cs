using System.Collections.Generic;

namespace GGemCo.Scripts.SaveData
{
    public class InventoryData : ISaveData
    {
        private readonly Dictionary<int, int> itemCounts = new Dictionary<int, int>();

        public void Initialize()
        {
            itemCounts.Clear();
        }

        public (int itemUid, int count) AddItem(int itemUid, int value)
        {
            if (itemUid <= 0) return (0, 0);
            itemCounts.TryAdd(itemUid, 0);
            itemCounts[itemUid] += value;
            return (itemUid, itemCounts[itemUid]);
        }

        public int GetItemCount(int itemUid)
        {
            return itemCounts.GetValueOrDefault(itemUid, 0);
        }
    }
}