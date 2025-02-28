using System.Collections.Generic;
using GGemCo.Scripts.Utils;

namespace GGemCo.Scripts.SaveData
{
    public class QuestData : ISaveData
    {
        private Dictionary<int, bool> completedQuests = new Dictionary<int, bool>();

        public void Initialize()
        {
            completedQuests.Clear();
        }

        public void CompleteQuest(int questId)
        {
            if (!completedQuests.ContainsKey(questId))
            {
                completedQuests[questId] = true;
                GcLogger.Log("퀘스트 완료: " + questId);
            }
        }

        public bool IsQuestCompleted(int questId)
        {
            return completedQuests.TryGetValue(questId, out bool isCompleted) && isCompleted;
        }
    }
}