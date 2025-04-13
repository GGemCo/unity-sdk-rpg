namespace GGemCo.Scripts
{
    public class SetIconHandlerInventory : ISetIconHandler
    {
        public void OnSetIcon(UIWindow window, int slotIndex, int iconUid, int iconCount, int iconLevel, bool isLearned)
        {
            var inventoryData = SceneGame.Instance.saveDataManager.Inventory;
            UIIcon icon = window.GetIconByIndex(slotIndex);
            if (icon != null)
            {
                inventoryData.SetItemCount(slotIndex, icon.uid, icon.GetCount());
            }
        }
        public void OnDetachIcon(UIWindow window, int slotIndex)
        {
            var inventoryData = SceneGame.Instance.saveDataManager.Inventory;
            UIIcon icon = window.GetIconByIndex(slotIndex);
            if (icon != null)
            {
                inventoryData.RemoveItemCount(slotIndex);
            }
        }
    }
}