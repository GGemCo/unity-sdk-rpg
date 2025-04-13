namespace GGemCo.Scripts
{
    public class SetIconHandlerItemBuy : ISetIconHandler
    {
        public void OnSetIcon(UIWindow window, int slotIndex, int iconUid, int iconCount, int iconLevel, bool isLearned)
        {
            UIWindowItemBuy uiWindowItemBuy = window as UIWindowItemBuy;
            if (uiWindowItemBuy == null) return;
            uiWindowItemBuy.UpdateInfo(iconUid, iconCount);
        }
        public void OnDetachIcon(UIWindow window, int slotIndex)
        {
        }
    }
}