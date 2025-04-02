﻿using System.Collections.Generic;
using GGemCo.Scripts.Configs;

namespace GGemCo.Scripts.Items
{
    public static class ItemConstants
    {
        public enum Type
        {
            None,
            Equip, // 장비
            Consumable, //소모품
        }

        public enum Category
        {
            None,
            Weapon, // 무기
            Armor, // 방어구
            Potion, // 물약
        }

        public enum SubCategory
        {
            None,
            Sword, // 칼
            Chest, // 상의
            Boots, // 신발
            RecoverHp, // hp 물약
            RecoverMp, // mp 물약
            IncreaseAttackSpeed, // 공격속도 증가
            IncreaseMoveSpeed, // 이동속도 증가
        }

        public enum Class
        {
            None,
            Normal, // 일반
        }
        public enum PartsType
        {
            None,
            Helmet,
            Chest,
            Shoulder,
            Forearm,
            Gloves,
            Belt,
            Pants,
            Boots,
            Weapon,
            Necklace,
            Ring,
            Shield,
        }
        /// <summary>
        /// 부위별 리소스 폴더 이름
        /// </summary>
        public static readonly Dictionary<PartsType, string> FolderNameByPartsType = new Dictionary<PartsType, string>
        {
            { PartsType.Chest, "Chest" },
            { PartsType.Boots, "Boots" },
            { PartsType.Weapon, "Weapon" },
        };
        /// <summary>
        /// 부위별 스파인 슬롯 이름
        /// </summary>
        public static readonly Dictionary<PartsType, List<string>> SlotNameByPartsType = new Dictionary<PartsType, List<string>>
        {
            { PartsType.Chest, new List<string> { "body" } },
            { PartsType.Boots, new List<string> { "leg_l", "leg_r" } },
            { PartsType.Weapon, new List<string> { "knife" } },
        };
        /// <summary>
        /// 슬롯별 어태치먼트 이름
        /// </summary>
        public static readonly Dictionary<string, string> AttachmentNameBySlotName = new Dictionary<string, string>
        {
            { "body", "body" },
            { "leg_l", "leg_l" },
            { "leg_r", "leg_r" },
            { "knife", "knife" },
            { "knife2", "knife" },
        };

        public static readonly Dictionary<ConfigCommon.SuffixType, string> StatusSuffixFormats = new Dictionary<ConfigCommon.SuffixType, string>
        {
            { ConfigCommon.SuffixType.Plus, "+{0}" },
            { ConfigCommon.SuffixType.Minus, "-{0}" },
            { ConfigCommon.SuffixType.Increase, "+{0}%" },
            { ConfigCommon.SuffixType.Decrease, "-{0}%" }
        };

    }
}