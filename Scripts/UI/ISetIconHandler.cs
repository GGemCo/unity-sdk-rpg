namespace GGemCo.Scripts
{
    public interface ISetIconHandler
    {
        void OnSetIcon(UIWindow window, int slotIndex, int iconUid, int iconCount, int iconLevel, bool isLearned);
        void OnDetachIcon(UIWindow window, int slotIndex);
    }
}