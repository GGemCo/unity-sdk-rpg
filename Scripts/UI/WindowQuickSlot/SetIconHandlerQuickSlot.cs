namespace GGemCo.Scripts
{
    public class SetIconHandlerQuickSlot : ISetIconHandler
    {
        public void OnSetIcon(UIWindow window, int slotIndex, int iconUid, int iconCount, int iconLevel, bool isLearned)
        {
            UIIcon icon = window.GetIconByIndex(slotIndex);
            if (icon == null) return;
            SceneGame.Instance.saveDataManager.QuickSlot.SetSkill(slotIndex, iconUid, iconCount, iconLevel, isLearned);
        }
        public void OnDetachIcon(UIWindow window, int slotIndex)
        {
            UIIcon icon = window.GetIconByIndex(slotIndex);
            if (icon == null) return;
            SceneGame.Instance.saveDataManager.QuickSlot.RemoveSkill(slotIndex);
        }
    }
}