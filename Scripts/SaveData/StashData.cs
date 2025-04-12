using System;
using System.Collections.Generic;

namespace GGemCo.Scripts
{
    /// <summary>
    /// 세이브 데이터 - 창고 아이템 정보
    /// </summary>
    public class StashData : ItemStorageData
    {
        public void Initialize(TableLoaderManager loader, SaveDataContainer saveDataContainer = null)
        {
            ItemCounts.Clear();
            if (saveDataContainer?.StashData != null)
            {
                ItemCounts = new Dictionary<int, SaveDataIcon>(saveDataContainer.StashData.ItemCounts);
            }
        }

        protected override int GetMaxSlotCount()
        {
            return SceneGame.Instance.uIWindowManager
                .GetUIWindowByUid<UIWindowStash>(UIWindowManager.WindowUid.Stash)?.maxCountIcon ?? 0;
        }
    }
}