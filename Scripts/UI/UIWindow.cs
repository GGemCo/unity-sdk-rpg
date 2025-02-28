using GGemCo.Scripts.Core;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GGemCo.Scripts.UI
{
    public class UIWindow : MonoBehaviour, IDropHandler
    {
        public UIWindowManager.WindowUid uid;
        public string id;

        public GameObject[] slots;
        public GameObject[] icons;

        private UIWindowFade uiWindowFade;

        protected virtual void Awake()
        {
            this.gameObject.AddComponent<CanvasGroup>();
            uiWindowFade = gameObject.AddComponent<UIWindowFade>();
        }

        // Start is called before the first frame update
        protected virtual void Start()
        {
        
        }
        /// <summary>
        /// 아이콘 가져오기
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GameObject GetIconByIndex(int index)
        {
            if (icons.Length == 0 || icons[index] == null)
            {
                GcLogger.LogError("아이콘이 없습니다. index: " +index);
                return null;
            }
        
            return icons[index];
        }
        /// <summary>
        /// 아이콘 가져오기
        /// </summary>
        /// <param name="iconUid"></param>
        /// <returns></returns>
        public GameObject GetIconByUid(int iconUid)
        {
            if (icons.Length == 0)
            {
                GcLogger.LogError("아이콘이 없습니다.");
                return null;
            }

            foreach (var icon in icons)
            {
                if (icon == null) continue;
                UIIcon uiIcon = icon.GetComponent<UIIcon>();
                if (uiIcon == null) continue;
                if (uiIcon.uid == iconUid)
                {
                    return icon;
                }
            }

            return null;
        }
        /// <summary>
        /// 아이콘 정보 지워주기 
        /// </summary>
        /// <param name="slotIndex"></param>
        public void DetachIcon(int slotIndex)
        {
            if (icons.Length <= 0) return;
            GameObject icon = icons[slotIndex];
            if (icon == null || icon.GetComponent<UIIcon>() == null)
            {
                GcLogger.LogError("슬롯에 아이콘이 없습니다. slot index: " +slotIndex);
                return;
            }
            icon.GetComponent<UIIcon>().ClearIconInfos();
            OnDetachIcon(slotIndex);
        }

        protected virtual void OnDetachIcon(int slotIndex)
        {
            
        }
        /// <summary>
        /// 아이콘 붙여주기 
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="slotIndex"></param>
        public void SetIcon(GameObject icon, int slotIndex)
        {
            GameObject slot = slots[slotIndex];
            if (slot == null)
            {
                GcLogger.LogError("슬롯에 아이콘이 없습니다. slot index: " +slotIndex);
                return;
            }
            icon.transform.SetParent(slot.transform);
            icon.transform.position = slot.transform.position;
            icon.GetComponent<UIIcon>().iconIndex = slotIndex;
            icon.GetComponent<UIIcon>().iconSlotIndex = slotIndex;
            icons[slotIndex] = icon;
            
            OnSetIcon(icon, slotIndex);
        }
        protected virtual void OnSetIcon(GameObject icon, int slotIndex)
        {
            
        }
        public virtual void OnClickIcon(UIIcon icon)
        {
        
        }
        public virtual void OnDrop(PointerEventData eventData)
        {
        }
        /// <summary>
        /// 아이콘 위에서 드래그가 끝났을때 처리 
        /// </summary>
        /// <param name="droppedIcon">드랍한 한 아이콘</param>
        /// <param name="originalIcon">드랍되는 곳에 있는 아이콘</param>
        public virtual void OnEndDragInIcon(GameObject droppedIcon, GameObject originalIcon)
        {
            // GcLogger.Log("OnEndDragInIcon");
        }
        /// <summary>
        /// 아이템 구매
        /// </summary>
        /// <param name="item"></param>
        public virtual void PurchaseItem(GameObject item)
        {
        }
        /// <summary>
        /// 아이콘 등급 이미지 path 가져오기
        /// </summary>
        /// <param name="valueGrade"></param>
        /// <param name="valueGradeLevel"></param>
        /// <returns></returns>
        public static string GetIconGradePath(UIIcon.Grade valueGrade, int valueGradeLevel)
        {
            return $"Images/UI/{UIIcon.IconGradeImagePath[valueGrade]}{valueGradeLevel}";
        }
        /// <summary>
        /// 윈도우가 show 가 된 후 처리 
        /// </summary>
        public virtual void OnShow(bool show)
        {
            
        }

        public void Show(bool show)
        {
            if (uiWindowFade == null) return;
            if (gameObject.activeSelf == show) return;
            if (show)
            {
                uiWindowFade.ShowPanel();
            }
            else
            {
                uiWindowFade.HidePanel();
            }
        }
    }
}
