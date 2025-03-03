using System.Collections.Generic;
using GGemCo.Scripts.Items;
using GGemCo.Scripts.Utils;

namespace GGemCo.Scripts.TableLoader
{
    public class StruckTableItemDropGroup
    {
        public int Uid;
        public string Memo;
        public ItemManager.ItemDropGroup Type;
        public string Value;
        public int Rate;
    }
    public class TableItemDropGroup : DefaultTable
    {
        private static readonly Dictionary<string, ItemManager.ItemDropGroup> MapType;

        static TableItemDropGroup()
        {
            MapType = new Dictionary<string, ItemManager.ItemDropGroup>
            {
                { "ItemCategory", ItemManager.ItemDropGroup.ItemCategory },
                { "ItemSubCategory", ItemManager.ItemDropGroup.ItemSubCategory },
                { "ItemUid", ItemManager.ItemDropGroup.ItemUid },
                { "ExcludeItemUid", ItemManager.ItemDropGroup.ExcludeItemUid },
                { "Nothing", ItemManager.ItemDropGroup.Nothing },
            };
        }
        private static ItemManager.ItemDropGroup ConvertType(string type) => MapType.GetValueOrDefault(type, ItemManager.ItemDropGroup.None);

        public readonly Dictionary<int, List<StruckTableItemDropGroup>> DropGroupDictionary = new Dictionary<int, List<StruckTableItemDropGroup>>();
        protected override void OnLoadedData(Dictionary<string, string> data)
        {
            int uid = int.Parse(data["Uid"]);

            if (!DropGroupDictionary.ContainsKey(uid))
            {
                DropGroupDictionary[uid] = new List<StruckTableItemDropGroup>();
            }
            StruckTableItemDropGroup struckTableItemDropGroup = new StruckTableItemDropGroup
            {
                Uid = int.Parse(data["Uid"]),
                Memo = data["Memo"],
                Type = ConvertType(data["Type"]),
                Value = data["Value"],
                Rate = int.Parse(data["Rate"]),
            };
            DropGroupDictionary[uid].Add(struckTableItemDropGroup);
        }
    }
}