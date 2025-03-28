using System.Collections.Generic;
using GGemCo.Scripts.Skill;
using GGemCo.Scripts.Utils;

namespace GGemCo.Scripts.TableLoader
{
    /// <summary>
    /// 맵 테이블 Structure
    /// </summary>
    public class StruckTableSkill
    {
        public int Uid;
        public string Name;
        public string ImagePath;
        public int Level;
        public int Maxlevel;
        public int NeedPlayerLevel;
        public SkillConstants.Target Target;
        public string DamageStatusID;
        public int DamageValue;
        public int Distance;
        public int NeedMp;
        public float CoolTime;
        public string OptionID1;
        public int OptionValue1;
        public float OptionDuration1;
        public int OptionRate1;
    }
    /// <summary>
    /// 맵 테이블
    /// </summary>
    public class TableSkill : DefaultTable
    {
        // 레벨 1인 것만 모아놓은 dictionary
        private readonly Dictionary<int, StruckTableSkill> skills = new Dictionary<int, StruckTableSkill>();
        // 레벨 별로 모아놓은 dictionary
        private readonly Dictionary<int, Dictionary<int, StruckTableSkill>> skillsByLevel = new Dictionary<int, Dictionary<int, StruckTableSkill>>();
        
        private static readonly Dictionary<string, SkillConstants.Target> MapTarget;
        static TableSkill()
        {
            MapTarget = new Dictionary<string, SkillConstants.Target>
            {
                { "Player", SkillConstants.Target.Player },
                { "Monster", SkillConstants.Target.Monster },
            };
        }
        private static SkillConstants.Target ConvertTarget(string type) => MapTarget.GetValueOrDefault(type, SkillConstants.Target.None);

        public Dictionary<int, StruckTableSkill> GetSkills()
        {
            return skills;
        }
        protected override void OnLoadedData(Dictionary<string, string> data)
        {
            int uid = int.Parse(data["Uid"]);
            int level = int.Parse(data["Level"]);

            if (!skillsByLevel.ContainsKey(uid))
            {
                skillsByLevel.TryAdd(uid, new Dictionary<int, StruckTableSkill>());
            }
            if (!skillsByLevel[uid].ContainsKey(level))
            {
                skillsByLevel[uid].TryAdd(level, new StruckTableSkill());
            }

            StruckTableSkill struckTableItemDropGroup = new StruckTableSkill
            {
                Uid = int.Parse(data["Uid"]),
                Name = data["Name"],
                ImagePath = data["ImagePath"],
                Level = int.Parse(data["Level"]),
                Maxlevel = int.Parse(data["Maxlevel"]),
                NeedPlayerLevel = int.Parse(data["NeedPlayerLevel"]),
                Target = ConvertTarget(data["Target"]),
                DamageStatusID = data["DamageStatusID"],
                DamageValue = int.Parse(data["DamageValue"]),
                Distance = int.Parse(data["Distance"]),
                NeedMp = int.Parse(data["NeedMp"]),
                CoolTime = float.Parse(data["CoolTime"]),
                OptionID1 = data["OptionID1"],
                OptionValue1 = int.Parse(data["OptionValue1"]),
                OptionDuration1 = float.Parse(data["OptionDuration1"]),
                OptionRate1 = int.Parse(data["OptionRate1"]),
            };
            skillsByLevel[uid][level] = struckTableItemDropGroup;
            if (!skills.ContainsKey(uid))
            {
                skills.TryAdd(uid, struckTableItemDropGroup);
            }

        }

        public StruckTableSkill GetDataByUidLevel(int uid, int level)
        {
            if (uid > 0 && level > 0)
            {
                Dictionary<int, StruckTableSkill> struckTableSkill = skillsByLevel.GetValueOrDefault(uid);
                if (struckTableSkill != null)
                {
                    return struckTableSkill.GetValueOrDefault(level);
                }
            }
            GcLogger.LogError("고유번호가 없거나 레벨 값이 없습니다.");
            return null;
        }

        public StruckTableSkill GetDataByUid(int uid)
        {
            GcLogger.LogError("사용할 수 없습니다.");
            return null;
        }
    }
}