using System.Collections.Generic;

namespace GGemCo.Scripts
{
    /// <summary>
    /// 세이브 데이터 - 퀘스트 정보
    /// </summary>
    public class QuestData : ISaveData
    {
        private Dictionary<int, bool> completedQuests = new Dictionary<int, bool>();

        public void Initialize(TableLoaderManager loader, SaveDataContainer saveDataContainer = null)
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