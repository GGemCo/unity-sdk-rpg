using System.Collections.Generic;
using GGemCo.Scripts.Attributes;
using GGemCo.Scripts.Items;
using GGemCo.Scripts.Utils;

namespace GGemCo.Scripts.TableLoader
{
    public class StruckTableItem
    {
        public int Uid;
        public string Name;
        public ItemConstants.Type Type;
        public ItemConstants.Category Category;
        public ItemConstants.SubCategory SubCategory;
        public ItemConstants.Class Class;
        public AttributesConstants.Type OptionType1;
        public float OptionValue1;
    }
    public class TableItem : DefaultTable
    {
        private static readonly Dictionary<string, ItemConstants.Type> MapType;
        private static readonly Dictionary<string, ItemConstants.Category> MapCategory;
        private static readonly Dictionary<string, ItemConstants.SubCategory> MapSubCategory;
        private static readonly Dictionary<string, ItemConstants.Class> MapClass;
        private static readonly Dictionary<string, AttributesConstants.Type> MapAttributesType;

        static TableItem()
        {
            MapType = new Dictionary<string, ItemConstants.Type>
            {
                { "Equip", ItemConstants.Type.Equip },
                { "Consumable", ItemConstants.Type.Consumable },
            };
            MapCategory = new Dictionary<string, ItemConstants.Category>
            {
                { "Weapon", ItemConstants.Category.Weapon },
                { "Potion", ItemConstants.Category.Potion },
            };
            MapSubCategory = new Dictionary<string, ItemConstants.SubCategory>
            {
                { "Sword", ItemConstants.SubCategory.Sword },
                { "RecoverHp", ItemConstants.SubCategory.RecoverHp },
                { "RecoverMp", ItemConstants.SubCategory.RecoverMp },
            };
            MapClass = new Dictionary<string, ItemConstants.Class>
            {
                { "Normal", ItemConstants.Class.Normal },
            };
            MapAttributesType = new Dictionary<string, AttributesConstants.Type>
            {
                { "Atk", AttributesConstants.Type.Atk },
            };
        }
        private static ItemConstants.Type ConvertType(string type) => MapType.GetValueOrDefault(type, ItemConstants.Type.None);
        private static ItemConstants.Category ConvertCategory(string type) => MapCategory.GetValueOrDefault(type, ItemConstants.Category.None);
        private static ItemConstants.SubCategory ConvertSubCategory(string type) => MapSubCategory.GetValueOrDefault(type, ItemConstants.SubCategory.None);
        private static ItemConstants.Class ConvertClass(string type) => MapClass.GetValueOrDefault(type, ItemConstants.Class.None);
        private static AttributesConstants.Type ConvertAttributesType(string type) => MapAttributesType.GetValueOrDefault(type, AttributesConstants.Type.None);
        
        public StruckTableItem GetDataByUid(int uid)
        {
            if (uid <= 0)
            {
                GcLogger.LogError("uid is 0.");
                return new StruckTableItem();
            }
            var data = GetData(uid);
            return new StruckTableItem
            {
                Uid = int.Parse(data["Uid"]),
                Name = data["Name"],
                Type = ConvertType(data["Type"]),
                Category = ConvertCategory(data["Category"]),
                SubCategory = ConvertSubCategory(data["SubCategory"]),
                Class = ConvertClass(data["Class"]),
                OptionType1 = ConvertAttributesType(data["OptionType1"]),
                OptionValue1 = float.Parse(data["OptionValue1"]),
            };
        }
    }
}