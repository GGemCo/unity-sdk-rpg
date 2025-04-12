﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GGemCo.Scripts
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
        public ItemConstants.PartsType PartsID;
        public ItemConstants.Class Class;
        public string ImagePath;
        public int MaxOverlayCount;
        public float CoolTime;
        public CurrencyConstants.Type SaleCurrencyType;
        public int SaleCurrencyValue;
        public ItemConstants.AntiFlag[] AntiFlag;
        public string Description;
        
        public string StatusID1;
        public ConfigCommon.SuffixType StatusSuffix1;
        public int StatusValue1;
        public string StatusID2;
        public ConfigCommon.SuffixType StatusSuffix2;
        public int StatusValue2;
        
        public string OptionType1;
        public ConfigCommon.SuffixType OptionSuffix1;
        public int OptionValue1;
        public string OptionType2;
        public ConfigCommon.SuffixType OptionSuffix2;
        public int OptionValue2;
        public string OptionType3;
        public ConfigCommon.SuffixType OptionSuffix3;
        public int OptionValue3;
        public string OptionType4;
        public ConfigCommon.SuffixType OptionSuffix4;
        public int OptionValue4;
        public string OptionType5;
        public ConfigCommon.SuffixType OptionSuffix5;
        public int OptionValue5;
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
        private static readonly Dictionary<string, ItemConstants.PartsType> MapPartsID;
        private static readonly Dictionary<string, ItemConstants.AntiFlag> MapAntiFlag;

        static TableItem()
        {
            MapType = new Dictionary<string, ItemConstants.Type>
            {
                { "Equip", ItemConstants.Type.Equip },
                { "Consumable", ItemConstants.Type.Consumable },
                { "Currency", ItemConstants.Type.Currency },
            };
            MapCategory = new Dictionary<string, ItemConstants.Category>
            {
                { "Weapon", ItemConstants.Category.Weapon },
                { "Armor", ItemConstants.Category.Armor },
                { "Potion", ItemConstants.Category.Potion },
                { "Gold", ItemConstants.Category.Gold },
                { "Silver", ItemConstants.Category.Silver },
            };
            MapSubCategory = new Dictionary<string, ItemConstants.SubCategory>
            {
                { "Sword", ItemConstants.SubCategory.Sword },
                { "Chest", ItemConstants.SubCategory.Chest },
                { "Boots", ItemConstants.SubCategory.Boots },
                { "RecoverHp", ItemConstants.SubCategory.RecoverHp },
                { "RecoverMp", ItemConstants.SubCategory.RecoverMp },
                { "IncreaseAttackSpeed", ItemConstants.SubCategory.IncreaseAttackSpeed },
                { "IncreaseMoveSpeed", ItemConstants.SubCategory.IncreaseMoveSpeed },
            };
            MapClass = new Dictionary<string, ItemConstants.Class>
            {
                { "Normal", ItemConstants.Class.Normal },
            };
            MapPartsID = new Dictionary<string, ItemConstants.PartsType>
            {
                { "Helmet", ItemConstants.PartsType.Helmet },
                { "Chest", ItemConstants.PartsType.Chest },
                { "Boots", ItemConstants.PartsType.Boots },
                { "Weapon", ItemConstants.PartsType.Weapon },
            };
            MapAntiFlag = new Dictionary<string, ItemConstants.AntiFlag>
            {
                { "Shop", ItemConstants.AntiFlag.ShopSale },
                { "Stash", ItemConstants.AntiFlag.Stash },
            };
        }
        private static ItemConstants.Type ConvertType(string type) => MapType.GetValueOrDefault(type, ItemConstants.Type.None);
        public static ItemConstants.Category ConvertCategory(string type) => MapCategory.GetValueOrDefault(type, ItemConstants.Category.None);
        public static ItemConstants.SubCategory ConvertSubCategory(string type) => MapSubCategory.GetValueOrDefault(type, ItemConstants.SubCategory.None);
        private static ItemConstants.Class ConvertClass(string type) => MapClass.GetValueOrDefault(type, ItemConstants.Class.None);
        private static ItemConstants.PartsType ConvertPartsID(string type) => MapPartsID.GetValueOrDefault(type, ItemConstants.PartsType.None);
        private static ItemConstants.AntiFlag[] ConvertAntiFlag(string type)
        {
            string[] flags = type.Split(',');
            ItemConstants.AntiFlag[] antiFlags = new ItemConstants.AntiFlag[flags.Length];
            for (int i = 0; i < antiFlags.Length; i++)
            {
                antiFlags[i] = MapAntiFlag.GetValueOrDefault(flags[i], ItemConstants.AntiFlag.None);
            }
            return antiFlags;
        }

        /// <summary>
        /// Description 문자열 내 {컬럼명} 형태의 플레이스홀더를 실제 값으로 치환합니다.
        /// </summary>
        /// <param name="template">설명 문자열</param>
        /// <param name="values">컬럼 이름과 값이 들어 있는 딕셔너리</param>
        /// <returns>치환된 문자열</returns>
        private static string ParsePlaceholders(string template, Dictionary<string, string> values)
        {
            // 정규식: 중괄호 {} 안의 내용을 캡처
            Regex regex = new Regex(@"\{(.*?)\}");

            return regex.Replace(template, match =>
            {
                string key = match.Groups[1].Value;

                if (values.TryGetValue(key, out var value))
                {
                    return value ?? "";
                }

                // 해당 키가 없으면 원래 문자열 유지
                return match.Value;
            });
        }
        
        private readonly Dictionary<ItemConstants.Category, List<StruckTableItem>> dictionaryByCategory = new Dictionary<ItemConstants.Category, List<StruckTableItem>>();
        private readonly Dictionary<ItemConstants.SubCategory, List<StruckTableItem>> dictionaryBySubCategory = new Dictionary<ItemConstants.SubCategory, List<StruckTableItem>>();
        protected override void OnLoadedData(Dictionary<string, string> data)
        {
            int uid = int.Parse(data["Uid"]);
            ItemConstants.Category category = ConvertCategory(data["Category"]);
            ItemConstants.SubCategory subCategory = ConvertSubCategory(data["SubCategory"]);
            data["Description"] = ParsePlaceholders(data["Description"], data);
            
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
                PartsID = ConvertPartsID(data["PartsID"]),
                SubCategory = ConvertSubCategory(data["SubCategory"]),
                Class = ConvertClass(data["Class"]),
                ImagePath = data["ImagePath"],
                AntiFlag = ConvertAntiFlag(data["AntiFlag"]),
                MaxOverlayCount = int.Parse(data["MaxOverlayCount"]),
                CoolTime = float.Parse(data["CoolTime"]),
                Description = data["Description"],
                SaleCurrencyType = ConvertCurrencyType(data["SaleCurrencyType"]),
                SaleCurrencyValue = int.Parse(data["SaleCurrencyValue"]),
                
                StatusID1 = data["StatusID1"],
                StatusSuffix1 = ConvertSuffixType(data["StatusSuffix1"]),
                StatusValue1 = int.Parse(data["StatusValue1"]),
                StatusID2 = data["StatusID2"],
                StatusSuffix2 = ConvertSuffixType(data["StatusSuffix2"]),
                StatusValue2 = int.Parse(data["StatusValue2"]),
                
                OptionType1 = data["OptionType1"],
                OptionSuffix1 = ConvertSuffixType(data["OptionSuffix1"]),
                OptionValue1 = int.Parse(data["OptionValue1"]),
                OptionType2 = data["OptionType2"],
                OptionSuffix2 = ConvertSuffixType(data["OptionSuffix2"]),
                OptionValue2 = int.Parse(data["OptionValue2"]),
                OptionType3 = data["OptionType3"],
                OptionSuffix3 = ConvertSuffixType(data["OptionSuffix3"]),
                OptionValue3 = int.Parse(data["OptionValue3"]),
                OptionType4 = data["OptionType4"],
                OptionSuffix4 = ConvertSuffixType(data["OptionSuffix4"]),
                OptionValue4 = int.Parse(data["OptionValue4"]),
                OptionType5 = data["OptionType5"],
                OptionSuffix5 = ConvertSuffixType(data["OptionSuffix5"]),
                OptionValue5 = int.Parse(data["OptionValue5"]),
            };
        }
    }
}