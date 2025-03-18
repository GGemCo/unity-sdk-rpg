using System.Collections.Generic;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI;
using GGemCo.Scripts.UI.Window;

namespace GGemCo.Scripts.SaveData
{
    /// <summary>
    /// 장비창 아이템 개수 관리
    /// </summary>
    public class EquipData : ItemStorageData
    {
        public void Initialize(TableLoaderManager loader, SaveDataContainer saveDataContainer = null)
        {
            ItemCounts.Clear();
            if (saveDataContainer?.EquipData != null)
            {
                ItemCounts = new Dictionary<int, StructInventoryIcon>(saveDataContainer.EquipData.ItemCounts);
            }

            UIWindowEquip uiWindowEquip = SceneGame.Instance.uIWindowManager
                .GetUIWindowByUid<UIWindowEquip>(UIWindowManager.WindowUid.Equip);

            if (uiWindowEquip == null) return;
            uiWindowEquip.OnSetIconEquip += SetItemCount;
            uiWindowEquip.OnDetachIconEquip += RemoveItemCount;
        }

        protected override int GetMaxSlotCount()
        {
            return SceneGame.Instance.uIWindowManager
                .GetUIWindowByUid<UIWindowEquip>(UIWindowManager.WindowUid.Equip)?.maxCountIcon ?? 0;
        }
    }
}