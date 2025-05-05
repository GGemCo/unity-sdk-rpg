using System;

namespace GGemCo.Scripts
{
    public static class GameEventManager
    {
        public static event Action<int> OnMonsterKilled = null;
        public static event Action<int, int> OnItemCollected = null;
        public static event Action<int> OnDialogStart = null;
        public static event Action<int> OnDialogEnd = null;

        public static void MonsterKilled(int monsterUid)
        {
            OnMonsterKilled?.Invoke(monsterUid);
        }

        public static void ItemCollected(int itemUid, int count)
        {
            OnItemCollected?.Invoke(itemUid, count);
        }
        public static void DialogStart(int npcUid)
        {
            OnDialogStart?.Invoke(npcUid);
        }
        public static void DialogEnd(int npcUid)
        {
            OnDialogEnd?.Invoke(npcUid);
        }
    }
}