using UnityEngine;

namespace GGemCo.Scripts.UI.Icon
{
    public class UISlot : MonoBehaviour
    {
        public UIWindow window;
        public UIWindowManager.WindowUid windowUid;
        public int index;

        private RectTransform rectTransform;
        protected void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
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
