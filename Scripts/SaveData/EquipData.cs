using System.Collections.Generic;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI;
using GGemCo.Scripts.UI.Window;

namespace GGemCo.Scripts.SaveData
{
    /// <summary>
    /// 세이브 데이터 - 장착 아이템 정보
    /// </summary>
    public class EquipData : ItemStorageData
    {
        public void Initialize(TableLoaderManager loader, SaveDataContainer saveDataContainer = null)
        {
            ItemCounts.Clear();
            if (saveDataContainer?.EquipData != null)
            {
                ItemCounts = new Dictionary<int, SaveDataIcon>(saveDataContainer.EquipData.ItemCounts);
            }
        }

        protected override int GetMaxSlotCount()
        {
            return SceneGame.Instance.uIWindowManager
                .GetUIWindowByUid<UIWindowEquip>(UIWindowManager.WindowUid.Equip)?.maxCountIcon ?? 0;
        }
    }
}