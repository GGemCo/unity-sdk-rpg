using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GGemCo.Scripts
{
    /// <summary>
    /// Window 테이블 Structure
    /// </summary>
    public class StruckTableQuest : IUidName
    {
        public int Uid { get; set; }
        public string Name { get; set; }
        public QuestConstants.Type Type;
        public string Title;
        public string FileName;
        public int NeedQuestUid;
        public int NextQuestUid;
        public int DialogueUid;
        public int MapUid;
        public int NpcUid;
        public int DirectionUid;
        public bool Repeat;
    }

    /// <summary>
    /// Window 테이블
    /// </summary>
    public class TableQuest : DefaultTable
    {
        private static readonly Dictionary<string, QuestConstants.Type> MapType;
        private static readonly Dictionary<int, Dictionary<int, List<int>>> QuestUids = new Dictionary<int, Dictionary<int, List<int>>>();

        static TableQuest()
        {
            MapType = new Dictionary<string, QuestConstants.Type>
            {
                { "Main", QuestConstants.Type.Main },
                { "Sub", QuestConstants.Type.Sub },
            };
        }
        private QuestConstants.Type ConvertType(string grade) => MapType.GetValueOrDefault(grade, QuestConstants.Type.None);

        protected override void PreLoad()
        {
            QuestUids.Clear();
        }
        protected override void OnLoadedData(Dictionary<string, string> data)
        {
            int mapUid = int.Parse(data["MapUid"]);
            int npcUid = int.Parse(data["NpcUid"]);
            int questUid = int.Parse(data["Uid"]);
            if (QuestUids.ContainsKey(mapUid) != true)
            {
                Dictionary<int, List<int>> newData = new Dictionary<int, List<int>>();
                List<int> newData2 = new List<int> { questUid };
                newData.TryAdd(npcUid, newData2);
                QuestUids.TryAdd(mapUid, newData);
            }
            else
            {
                Dictionary<int, List<int>> newData = QuestUids[mapUid];
                if (newData.ContainsKey(npcUid) != true)
                {
                    List<int> newData2 = new List<int> { questUid };
                    newData.TryAdd(npcUid, newData2);
                }
                else
                {
                    List<int> newData2 = QuestUids[mapUid][npcUid];
                    newData2.Add(questUid);
                }
            }
        }
        public StruckTableQuest GetDataByUid(int uid)
        {
            if (uid <= 0)
            {
                GcLogger.LogError("uid is 0.");
                return null;
            }

            var data = GetData(uid);
            if (data == null) return null;
            return new StruckTableQuest
            {
                Uid = int.Parse(data["Uid"]),
                Type = ConvertType(data["Type"]),
                Title = data["Title"],
                FileName = data["FileName"],
                NeedQuestUid = int.Parse(data["NeedQuestUid"]),
                NextQuestUid = int.Parse(data["NextQuestUid"]),
                DialogueUid = int.Parse(data["DialogueUid"]),
                MapUid = int.Parse(data["MapUid"]),
                NpcUid = int.Parse(data["NpcUid"]),
                DirectionUid = int.Parse(data["DirectionUid"]),
                Repeat = ConvertBoolean(data["Repeat"]),
            };
        }
        public List<int> GetQuestsByNpcUnum(int mapUid, int npcUid)
        {
            List<int> empty = new List<int>();
            if (QuestUids.TryGetValue(mapUid, out var npcUids) != true || npcUids.ContainsKey(npcUid) != true)
                return empty;
            return QuestUids[mapUid][npcUid];
        }
        public override bool TryGetDataByUid(int uid, out object info)
        {
            info = GetDataByUid(uid);
            return info != null && ((StruckTableQuest)info).Uid > 0;
        }
    }
}