using System.Collections.Generic;
using GGemCo.Scripts.Items;
using GGemCo.Scripts.Utils;

namespace GGemCo.Scripts.TableLoader
{
    /// <summary>
    /// 아이템 테이블 Structure
    /// </summary>
    public class StruckTableItem
    {
        public int Uid;
        public string Name;
        public ItemConstants.Type Type;
        public ItemConstants.Category Category;
        public ItemConstants.SubCategory SubCategory;
        public ItemConstants.Class Class;
        public string ImagePath;
        public string StatusID1;
        public int StatusValue1;
        public string OptionType1;
        public int OptionValue1;
    }
    /// <summary>
    /// 아이템 테이블
    /// </summary>
    public class TableItem : DefaultTable
    {
        private static readonly Dictionary<string, ItemConstants.Type> MapType;
        private static readonly Dictionary<string, ItemConstants.Category> MapCategory;
        private static readonly Dictionary<string, ItemConstants.SubCategory> MapSubCategory;
        private static readonly Dictionary<string, ItemConstants.Class> MapClass;

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
                { "Armor", ItemConstants.Category.Armor },
                { "Potion", ItemConstants.Category.Potion },
            };
            MapSubCategory = new Dictionary<string, ItemConstants.SubCategory>
            {
                { "Sword", ItemConstants.SubCategory.Sword },
                { "Top", ItemConstants.SubCategory.Top },
                { "Leg", ItemConstants.SubCategory.Leg },
                { "RecoverHp", ItemConstants.SubCategory.RecoverHp },
                { "RecoverMp", ItemConstants.SubCategory.RecoverMp },
                { "IncreaseAttackSpeed", ItemConstants.SubCategory.IncreaseAttackSpeed },
                { "IncreaseMoveSpeed", ItemConstants.SubCategory.IncreaseMoveSpeed },
            };
            MapClass = new Dictionary<string, ItemConstants.Class>
            {
                { "Normal", ItemConstants.Class.Normal },
            };
        }
        private static ItemConstants.Type ConvertType(string type) => MapType.GetValueOrDefault(type, ItemConstants.Type.None);
        private static ItemConstants.Category ConvertCategory(string type) => MapCategory.GetValueOrDefault(type, ItemConstants.Category.None);
        private static ItemConstants.SubCategory ConvertSubCategory(string type) => MapSubCategory.GetValueOrDefault(type, ItemConstants.SubCategory.None);
        private static ItemConstants.Class ConvertClass(string type) => MapClass.GetValueOrDefault(type, ItemConstants.Class.None);
        
        private readonly Dictionary<ItemConstants.Category, List<StruckTableItem>> dictionaryByCategory = new Dictionary<ItemConstants.Category, List<StruckTableItem>>();
        private readonly Dictionary<ItemConstants.SubCategory, List<StruckTableItem>> dictionaryBySubCategory = new Dictionary<ItemConstants.SubCategory, List<StruckTableItem>>();
        protected override void OnLoadedData(Dictionary<string, string> data)
        {
            int uid = int.Parse(data["Uid"]);
            ItemConstants.Category category = ConvertCategory(data["Category"]);
            ItemConstants.SubCategory subCategory = ConvertSubCategory(data["SubCategory"]);
            
            StruckTableItem struckTableItemDropGroup = GetDataByUid(uid);
            {
                if (!dictionaryByCategory.ContainsKey(category))
                {
                    dictionaryByCategory[category] = new List<StruckTableItem>();
                }

                dictionaryByCategory[category].Add(struckTableItemDropGroup);
            }
            {
                if (!dictionaryBySubCategory.ContainsKey(subCategory))
                {
                    dictionaryBySubCategory[subCategory] = new List<StruckTableItem>();
                }

                dictionaryBySubCategory[subCategory].Add(struckTableItemDropGroup);
            }
        }

        public Dictionary<ItemConstants.Category, List<StruckTableItem>> GetDictionaryByCategory()
        {
            return dictionaryByCategory;
        }
        public Dictionary<ItemConstants.SubCategory, List<StruckTableItem>> GetDictionaryBySubCategory()
        {
            return dictionaryBySubCategory;
        }
        
        public StruckTableItem GetDataByUid(int uid)
        {
            if (uid <= 0)
            {
                GcLogger.LogError("uid is 0.");
                return null;
            }
            var data = GetData(uid);
            if (data == null) return null;
            return new StruckTableItem
            {
                Uid = int.Parse(data["Uid"]),
                Name = data["Name"],
                Type = ConvertType(data["Type"]),
                Category = ConvertCategory(data["Category"]),
                SubCategory = ConvertSubCategory(data["SubCategory"]),
                Class = ConvertClass(data["Class"]),
                ImagePath = data["ImagePath"],
                StatusID1 = data["StatusID1"],
                StatusValue1 = int.Parse(data["StatusValue1"]),
                OptionType1 = data["OptionType1"],
                OptionValue1 = int.Parse(data["OptionValue1"]),
            };
        }
    }
}