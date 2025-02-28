using GGemCo.Scripts.TableLoader;
using UnityEngine;

namespace GGemCo.Scripts.SaveData
{
    public class SaveDataManager : MonoBehaviour
    {
        public PlayerData Player { get; private set; }
        public InventoryData Inventory { get; private set; }
        public QuestData Quest { get; private set; }

        private TableLoaderManager tableLoaderManager;

        private void Awake()
        {
            tableLoaderManager = TableLoaderManager.Instance;
            if (tableLoaderManager == null) return;

            // 각 데이터 클래스 초기화
            Player = new PlayerData(tableLoaderManager);
            Inventory = new InventoryData();
            Quest = new QuestData();

            // 초기화 실행
            Player.Initialize();
            Inventory.Initialize();
            Quest.Initialize();
        }
    }
}