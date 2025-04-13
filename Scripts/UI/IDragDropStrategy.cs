using UnityEngine;

namespace GGemCo.Scripts.UI
{
    public interface IDragDropStrategy
    {
        void HandleDragInIcon(UIWindow window, UIIcon dropped, UIIcon target);
        void HandleDragOut(UIWindow window, Vector3 worldPosition, GameObject droppedIcon, GameObject targetIcon, Vector3 originalPosition);
    }
}