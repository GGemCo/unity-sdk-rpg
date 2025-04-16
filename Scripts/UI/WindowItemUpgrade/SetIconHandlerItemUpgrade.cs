﻿namespace GGemCo.Scripts
{
    public class SetIconHandlerItemUpgrade : ISetIconHandler
    {
        public void OnSetIcon(UIWindow window, int slotIndex, int iconUid, int iconCount, int iconLevel, bool isLearned)
        {
            UIIcon icon = window.GetIconByIndex(slotIndex);
            if (icon == null) return;
            UIWindowItemUpgrade uiWindowItemUpgrade = window as UIWindowItemUpgrade;
            if (uiWindowItemUpgrade == null) return;
            uiWindowItemUpgrade.SetInfo(icon);
        }
        public void OnDetachIcon(UIWindow window, int slotIndex)
        {
            UIIcon icon = window.GetIconByIndex(slotIndex);
            if (icon == null) return;
            UIWindowItemUpgrade uiWindowItemUpgrade = window as UIWindowItemUpgrade;
            if (uiWindowItemUpgrade == null) return;
            uiWindowItemUpgrade.ClearMaterials();
        }
    }
}