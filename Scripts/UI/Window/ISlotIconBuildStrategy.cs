using UnityEngine;
using UnityEngine.UI;

namespace GGemCo.Scripts
{
    public interface ISlotIconBuildStrategy
    {
        void BuildSlotsAndIcons(UIWindow window, GridLayoutGroup container, int maxCount, IconConstants.Type iconType, Vector2 slotSize, Vector2 iconSize, GameObject[] slots, GameObject[] icons);
    }
    
}