using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.UI.Icon;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGemCo.Scripts.UI
{
    /// <summary>
    /// 윈도우 
    /// </summary>
    public class UIWindow : MonoBehaviour, IDropHandler
    {
        // 공개(public) → 보호(protected) → 내부(internal) → 비공개(private) 
        // 상수(const), 정적(static) 필드 → 인스턴스 필드 → 속성(Properties) → 생성자(Constructors) → 메서드(Methods)
        // 윈도우 고유번호
        public UIWindowManager.WindowUid uid;
        [Header("기본속성")] 
        [Tooltip("사용할 최대 아이콘 개수")]
        public int maxCountIcon;
        [Tooltip("윈도우 On/Off 시 fade in/Out 효과 사용 여부")]
        public bool useFade = true;
        [HideInInspector] public GameObject[] slots;
        [HideInInspector] public GameObject[] icons;
        [Tooltip("slot 사이즈. 보통 icon size 보다 크게 설정")]
        public Vector2 slotSize;
        [Tooltip("icon 사이즈")]
        public Vector2 iconSize;
        
        [Header("오브젝트")]
        [Tooltip("icon 이 들어갈 panel")]
        public GridLayoutGroup containerIcon;
        
        private UIWindowFade uiWindowFade;

        protected virtual void Awake()
        {
            slots = new GameObject[maxCountIcon];
            icons = new GameObject[maxCountIcon];
            gameObject.AddComponent<CanvasGroup>();
            if (useFade)
            {
                uiWindowFade = gameObject.AddComponent<UIWindowFade>();
            }

            if (containerIcon != null && slotSize != Vector2.zero)
            {
                containerIcon.cellSize = new Vector2(slotSize.x, slotSize.y);
            }

            InitializePool();
        }

        private void InitializePool()
        {
            ExpandPool(maxCountIcon);
        }
        /// <summary>
        /// 특정 개수만큼 풀을 확장하여 아이템을 추가 생성.
        /// </summary>
        protected virtual void ExpandPool(int amount)
        {
            if (AddressableSettingsLoader.Instance == null || containerIcon == null) return;
            if (amount <= 0) return;
            GameObject iconItem = AddressablePrefabLoader.Instance.GetPreLoadGamePrefabByName(ConfigAddressables.KeyPrefabIconItem);
            GameObject slot = AddressablePrefabLoader.Instance.GetPreLoadGamePrefabByName(ConfigAddressables.KeyPrefabSlot);
            if (iconItem == null) return;
            for (int i = 0; i < amount; i++)
            {
                GameObject slotObject = Instantiate(slot, containerIcon.gameObject.transform);
                UISlot uiSlot = slotObject.GetComponent<UISlot>();
                if (uiSlot == null) continue;
                uiSlot.Initialize(this, uid, i, slotSize);
                slots[i] = slotObject;
                
                GameObject icon = Instantiate(iconItem, slotObject.transform);
                UIIcon uiIcon = icon.GetComponent<UIIcon>();
                if (uiIcon == null) continue;
                uiIcon.Initialize(this, uid, i, i, iconSize, slotSize);
                icons[i] = icon;
            }
            // GcLogger.Log($"풀 확장: {amount}개 아이템 추가 (총 {poolDropItem.Count}개)");
        }
        protected virtual void Start()
        {
        }
        /// <summary>
        /// index 로 아이콘 가져오기
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
        /// Uid 로 아이콘 가져오기
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
        /// 아이콘 지우기 
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
            UIIcon uiIcon = icon.GetComponent<UIIcon>();
            uiIcon.ClearIconInfos();
            
            OnDetachIcon(slotIndex);
        }

        protected virtual void OnDetachIcon(int slotIndex)
        {
        }
        /// <summary>
        /// 아이콘 붙여주기 
        /// </summary>
        /// <param name="slotIndex"></param>
        /// <param name="icon"></param>
        public void SetIcon(int slotIndex, GameObject icon)
        {
            GameObject slot = slots[slotIndex];
            if (slot == null)
            {
                GcLogger.LogError("슬롯에 아이콘이 없습니다. slot index: " +slotIndex);
                return;
            }
            icon.transform.SetParent(slot.transform);
            icon.transform.position = slot.transform.position;
            UIIcon uiIcon = icon.GetComponent<UIIcon>();
            uiIcon.window = this;
            uiIcon.windowUid = uid;
            uiIcon.index = slotIndex;
            uiIcon.slotIndex = slotIndex;
            icons[slotIndex] = icon;
            
            OnSetIcon(slotIndex, icon);
        }
        protected virtual void OnSetIcon(int slotIndex, GameObject icon)
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
        /// <param name="targetIcon">드랍되는 곳에 있는 아이콘</param>
        public virtual void OnEndDragInIcon(GameObject droppedIcon, GameObject targetIcon)
        {
            // GcLogger.Log("OnEndDragInIcon");
        }
        /// <summary>
        ///  window 밖에다 드래그앤 드랍 했을때 처리 
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="droppedIcon"></param>
        /// <param name="targetIcon"></param>
        /// <param name="originalPosition"></param>
        public virtual void OnEndDragOutWindow(PointerEventData eventData, GameObject droppedIcon, GameObject targetIcon, Vector3 originalPosition)
        {
            // GcLogger.Log("OnEndDragInIcon");
        }
        /// <summary>
        /// 아이콘 등급 이미지 path 가져오기
        /// </summary>
        /// <param name="valueGrade"></param>
        /// <returns></returns>
        public static string GetIconGradePath(IIcon.Grade valueGrade)
        {
            return $"Images/UI/{IIcon.IconGradeImagePath[valueGrade]}";
        }
        /// <summary>
        /// 윈도우 open/close
        /// </summary>
        /// <param name="show"></param>
        public virtual bool Show(bool show)
        {
            if (uiWindowFade == null)
            {
                gameObject.SetActive(show);
                OnShow(show);
                return false;
            }
            if (gameObject.activeSelf == show) return false;
            if (show)
            {
                uiWindowFade.ShowPanel();
            }
            else
            {
                uiWindowFade.HidePanel();
            }

            return true;
        }
        /// <summary>
        /// 윈도우가 show 가 된 후 처리 
        /// </summary>
        public virtual void OnShow(bool show)
        {
            
        }

        public void OnClickClose()
        {
            if (uiWindowFade == null) return;
            Show(false);
        }
        /// <summary>
        /// 아이콘 원래 자리로 이동시키기
        /// </summary>
        /// <param name="droppedIcon"></param>
        protected void GoBackToSlot(GameObject droppedIcon)
        {
            if (droppedIcon == null) return;
            UIIcon icon = droppedIcon.GetComponent<UIIcon>();
            if (icon == null) return;
            GameObject targetSlot = icon.window.slots[icon.slotIndex];
            droppedIcon.transform.SetParent(targetSlot.transform);
            droppedIcon.transform.position = icon.GetDragOriginalPosition();
            droppedIcon.transform.SetSiblingIndex(1);
        }
    }
}
