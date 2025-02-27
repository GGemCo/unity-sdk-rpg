using System.Collections.Generic;
using GGemCo.Scripts.Items;
using GGemCo.Scripts.Utils;

namespace GGemCo.Scripts.TableLoader
{
    public class StruckTableMonsterDropRate
    {
        public int Uid;
        public string Memo;
        public int MonsterUid;
        public ItemManager.MonsterDropRateType Type;
        public int Value;
        public int Rate;
    }
    public class TableMonsterDropRate : DefaultTable
    {
        private static readonly Dictionary<string, ItemManager.MonsterDropRateType> MapType;

        static TableMonsterDropRate()
        {
            MapType = new Dictionary<string, ItemManager.MonsterDropRateType>
            {
                { "ItemDropGroupUid", ItemManager.MonsterDropRateType.ItemDropGroupUid },
                { "Nothing", ItemManager.MonsterDropRateType.Nothing },
            };
        }
        private static ItemManager.MonsterDropRateType ConvertType(string type) => MapType.GetValueOrDefault(type, ItemManager.MonsterDropRateType.None);

        public readonly Dictionary<int, List<StruckTableMonsterDropRate>> MonsterDropDictionary = new Dictionary<int, List<StruckTableMonsterDropRate>>();
        protected override void OnLoadedData(Dictionary<string, string> data)
        {
            int uid = int.Parse(data["Uid"]);
            int monsterUid = int.Parse(data["MonsterUid"]);

            if (!MonsterDropDictionary.ContainsKey(monsterUid))
            {
                MonsterDropDictionary[monsterUid] = new List<StruckTableMonsterDropRate>();
            }

            StruckTableMonsterDropRate struckTableMonsterDropRate = GetDataByUid(uid);
            MonsterDropDictionary[monsterUid].Add(struckTableMonsterDropRate);
        }
        public StruckTableMonsterDropRate GetDataByUid(int uid)
        {
            if (uid <= 0)
            {
                GcLogger.LogError("uid is 0.");
                return new StruckTableMonsterDropRate();
            }
            var data = GetData(uid);
            return new StruckTableMonsterDropRate
            {
                Uid = int.Parse(data["Uid"]),
                Memo = data["Memo"],
                MonsterUid = int.Parse(data["MonsterUid"]),
                Type = ConvertType(data["Type"]),
                Value = int.Parse(data["Value"]),
                Rate = int.Parse(data["Rate"]),
            };
        }
    }
}