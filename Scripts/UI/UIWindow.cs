using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGemCo.Scripts
{
    /// <summary>
    /// 윈도우 
    /// </summary>
    public class UIWindow : MonoBehaviour, IDropHandler
    {
        // 공개(public) → 보호(protected) → 내부(internal) → 비공개(private) 
        // 상수(const), 정적(static) 필드 → 인스턴스 필드 → 속성(Properties) → 생성자(Constructors) → 메서드(Methods)
        // 윈도우 고유번호
        [HideInInspector] public UIWindowManager.WindowUid uid;
        [Header("기본속성")] 
        [Tooltip("아이콘 타입")] 
        public IconConstants.Type iconType;
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
        private StruckTableWindow struckTableWindow;
        private InteractionManager interactionManager;
        protected SceneGame SceneGame;

        protected virtual void Awake()
        {
            // 사용하지 않는 index 가 있을 수 있으므로 미리 만들어 두어야 건너 띄어도 문제가 없다.
            slots = new GameObject[maxCountIcon];
            icons = new GameObject[maxCountIcon];
            gameObject.AddComponent<CanvasGroup>();
            if (useFade)
            {
                uiWindowFade = gameObject.AddComponent<UIWindowFade>();
            }

            if (containerIcon != null && containerIcon.cellSize == Vector2.zero && slotSize != Vector2.zero)
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
            GameObject iconItem;
            if (iconType == IconConstants.Type.Skill)
            {
                iconItem = AddressablePrefabLoader.Instance.GetPreLoadGamePrefabByName(ConfigAddressables.KeyPrefabIconSkill);
            }
            else
            {
                iconItem = AddressablePrefabLoader.Instance.GetPreLoadGamePrefabByName(ConfigAddressables.KeyPrefabIconItem);
            }
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
            if (struckTableWindow != null && !struckTableWindow.DefaultActive)
            {
                gameObject.SetActive(false);
            }
            SceneGame = SceneGame.Instance;
            interactionManager = SceneGame.InteractionManager;
        }
        /// <summary>
        /// index 로 아이콘 가져오기
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public UIIcon GetIconByIndex(int index)
        {
            if (icons.Length == 0 || icons[index] == null)
            {
                GcLogger.LogError("아이콘이 없습니다. index: " +index);
                return null;
            }
        
            return icons[index].GetComponent<UIIcon>();
        }
        /// <summary>
        /// Uid 로 아이콘 가져오기
        /// </summary>
        /// <param name="iconUid"></param>
        /// <returns></returns>
        public UIIcon GetIconByUid(int iconUid)
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
                    return uiIcon;
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
        /// 빈 슬롯 찾기
        /// </summary>
        private int FindEmptySlot()
        {
            for (int i = 0; i < maxCountIcon; i++)
            {
                var icon = icons[i];
                if (icon == null) continue;
                UIIcon uiIcon = icon.GetComponent<UIIcon>();
                if (uiIcon == null) continue;
                if (uiIcon.uid <= 0 || uiIcon.GetCount() <= 0)
                    return i;
            }
            return -1;
        }
        public virtual void SetIconCount(int iconUid, int iconCount)
        {
            int emptySlot = FindEmptySlot();
            if (emptySlot == -1)
            {
                SceneGame.popupManager.ShowPopupError("윈도우에 빈 공간이 없습니다.");
                return;
            }
            SetIconCount(emptySlot, iconUid, iconCount);
        }

        public UIIcon SetIconCountReturnIcon(int slotIndex, int iconUid, int iconCount, int iconLevel = 0,
            bool iconLearn = false)
        {
            return SetIconCount(slotIndex, iconUid, iconCount, iconLevel, iconLearn);
        }
        public UIIcon SetIconCountReturnIcon(int iconUid, int iconCount)
        {
            int emptySlot = FindEmptySlot();
            if (emptySlot == -1)
            {
                SceneGame.popupManager.ShowPopupError("윈도우에 빈 공간이 없습니다.");
                return null;
            }
            return SetIconCount(emptySlot, iconUid, iconCount);
        }

        /// <summary>
        /// 아이콘 붙여주기 
        /// </summary>
        /// <param name="slotIndex"></param>
        /// <param name="iconUid"></param>
        /// <param name="iconCount"></param>
        /// <param name="iconLevel"></param>
        /// <param name="iconLearn"></param>
        public UIIcon SetIconCount(int slotIndex, int iconUid, int iconCount, int iconLevel = 0, bool iconLearn = false)
        {
            GameObject icon = icons[slotIndex];
            if (icon == null)
            {
                GcLogger.LogError("슬롯에 아이콘이 없습니다. slot index: " +slotIndex);
                return null;
            }
            UIIcon uiIcon = icon.GetComponent<UIIcon>();
            if (uiIcon == null)
            {
                GcLogger.LogError("슬롯에 UIIcon 이 없습니다. slot index: " +slotIndex);
                return null;
            }

            if (iconCount <= 0)
            {
                DetachIcon(slotIndex);
                return null;
            }
            uiIcon.window = this;
            uiIcon.windowUid = uid;
            uiIcon.ChangeInfoByUid(iconUid, iconCount, iconLevel, iconLearn);
            
            OnSetIcon(slotIndex, iconUid, iconCount, iconLevel, iconLearn);
            return uiIcon;
        }
        /// <summary>
        /// 아이콘 이동 후 슬롯별 uid, count 처리  
        /// </summary>
        /// <param name="result"></param>
        public void SetIcons(ResultCommon result)
        {
            if (!result.IsSuccess())
            {
                GcLogger.LogError(result.Message);
                SceneGame.systemMessageManager.ShowMessageWarning(result.Message);
            }
            if (result.ResultIcons == null || result.ResultIcons.Count <= 0) return;
            foreach (var icon in result.ResultIcons)
            {
                SetIconCount(icon.SlotIndex, icon.Uid, icon.Count, icon.Level, icon.IsLearned);
            }
        }
        protected virtual void OnSetIcon(int slotIndex, int iconUid, int iconCount, int iconLevel = 0, bool iconLearn = false)
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
        public static string GetIconGradePath(IconConstants.Grade valueGrade)
        {
            return $"Images/UI/{IconConstants.IconGradeImagePath[valueGrade]}";
        }
        /// <summary>
        /// window 테이블에 있는 OpenWindowUid, CloseWindowUid 컬럼 처리
        /// </summary>
        /// <param name="windowUids"></param>
        /// <param name="show"></param>
        private void ShowByTable(int[] windowUids, bool show)
        {
            foreach (var openWindowUid in windowUids)
            {
                UIWindowManager.WindowUid windowUid = (UIWindowManager.WindowUid)openWindowUid;
                UIWindow uiWindow = SceneGame.uIWindowManager.GetUIWindowByUid<UIWindow>(windowUid);
                    
                if (uiWindow.uiWindowFade == null)
                {
                    if (uiWindow.gameObject == null) continue;
                    uiWindow.gameObject.SetActive(show);
                    uiWindow.OnShow(show); 
                    continue;
                }

                if (show)
                {
                    uiWindow.uiWindowFade.ShowPanel();
                }
                else
                {
                    uiWindow.uiWindowFade.HidePanel();
                }
            }
        }
        /// <summary>
        /// 윈도우 open/close
        /// </summary>
        /// <param name="show"></param>
        public virtual bool Show(bool show)
        {
            if (uiWindowFade == null)
            {
                if (gameObject == null) return false;
                gameObject.SetActive(show);
                OnShow(show);
                return false;
            }
            if (show)
            {
                uiWindowFade.ShowPanel();
                ShowByTable(struckTableWindow.OpenWindowUid, true);
            }
            else
            {
                uiWindowFade.HidePanel();
                ShowByTable(struckTableWindow.CloseWindowUid, false);
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
            if (struckTableWindow.IsInteraction && interactionManager != null && interactionManager.IsInteractioning())
            {
                interactionManager.EndInteraction();
            }
            else
            {
                Show(false);
            }
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
        /// <summary>
        /// 각 윈도우에 table 정보 연결하기
        /// </summary>
        /// <param name="pstruckTableWindow"></param>
        public void SetTableWindow(StruckTableWindow pstruckTableWindow)
        {
            struckTableWindow = pstruckTableWindow;
        }

        private UIIcon selectedIcon;
        public virtual void SetSelectedIcon(int index)
        {
            if (selectedIcon != null)
            {
                selectedIcon.SetSelected(false);
            }
            if (icons.Length <= 0) return;
            var icon = icons[index];
            if (icon == null) return;
            selectedIcon = icon.GetComponent<UIIcon>();
            selectedIcon.SetSelected(true);
        }

        protected UIIcon GetSelectedIcon()
        {
            return selectedIcon;
        }
        public virtual void OnRightClick(UIIcon icon)
        {
            
        }
        public virtual void CopyIconCount(int toIndex, int fromIndex, int fromItemUid, int fromItemCount)
        {
            
        }

        public bool GetDefaultActive()
        {
            return struckTableWindow.DefaultActive;
        }

        public bool IsOpen()
        {
            return gameObject.activeSelf;
        }

        public virtual void ShowItemInfo(UIIcon icon)
        {
        }
        
    }
}
