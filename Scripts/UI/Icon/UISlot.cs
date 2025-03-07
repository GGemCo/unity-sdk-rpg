using UnityEditor;
using UnityEngine;

namespace GGemCo.Scripts.UI.Icon
{
    public class UISlot : MonoBehaviour
    {
        private UIWindow window;
        private UIWindowManager.WindowUid windowUid;
        private int index;

        private RectTransform rectTransform;
        protected void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void Initialize(UIWindow pwindow, UIWindowManager.WindowUid pwindowUid, int pindex, Vector2 slotSize)
        {
            window = pwindow;
            windowUid = pwindowUid;
            index = pindex;
            
            ChangeSlotImageSize(slotSize);
        }
        /// <summary>
        /// 슬롯 이미지 사이즈 변경하기
        /// </summary>
        /// <param name="size"></param>
        public void ChangeSlotImageSize(Vector2 size)
        {
            rectTransform.sizeDelta = size;
        }
    }
}
