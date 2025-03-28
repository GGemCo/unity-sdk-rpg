using GGemCo.Scripts.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo.Scripts.UI.Icon
{
    /// <summary>
    /// 아이콘 공용
    /// </summary>
    public class UIIcon : MonoBehaviour
    {
        [Header("오브젝트")]
        [Tooltip("개수를 표현할 텍스트")]
        public TextMeshProUGUI textCount;
        [Tooltip("쿨타임 게이지")]
        public Image imageCoolTimeGauge;
        [Tooltip("선택되었을때 보여줄 이미지")]
        [HideInInspector] public Image imageSelected;

        private bool isSelected;
        
        // 윈도우 
        [HideInInspector] public UIWindow window;
        // 윈도우 고유번호
        [HideInInspector] public UIWindowManager.WindowUid windowUid;
        // 번호
        [HideInInspector] public int index;
        // 슬롯 번호
        [HideInInspector] public int slotIndex;
        // 고유번호 (아이템일때는 아이템 고유번호)
        [HideInInspector] public int uid;

        protected IconConstants.Type IconType;
        private IconConstants.Status iconStatus;

        // 아이콘 이미지
        protected Image ImageIcon;
        // 개수
        private int count;
        // 레벨
        private int level;
        // 배웠는지
        private bool isLearn;
        // 등급
        private IconConstants.Grade grade;
        // 등급 아이콘
        [HideInInspector] public Image imageGrade;
        // 잠금 아이콘
        [HideInInspector] public GameObject imageLock;
            
        // 드래그 핸들러
        private UIDragHandler dragHandler;
        // 쿨타임 핸들러
        protected UICoolTimeHandler CoolTimeHandler;
        private RectTransform rectTransform;

        protected virtual void Awake()
        {
            uid = 0;
            count = 0;
            level = 0;
            index = 0;
            slotIndex = 0;
            isLearn = false;
            window = null;
            windowUid = UIWindowManager.WindowUid.None;
            iconStatus = IconConstants.Status.Normal;
            IconType = IconConstants.Type.None;
            
            ImageIcon = GetComponent<Image>();
            dragHandler = gameObject.AddComponent<UIDragHandler>();
            CoolTimeHandler = gameObject.AddComponent<UICoolTimeHandler>();
            rectTransform = GetComponent<RectTransform>();

            SetSelected(false);
        }

        protected virtual void Start()
        {
            if (imageLock != null)
            {
                SetIconLock(count <= 0);
            }
            // 선택되었을때 보여줄 이미지 크기를 slot size 로 변경
            if (imageSelected == null) return;
            imageSelected.rectTransform.sizeDelta = window.slotSize;
        }

        public void Initialize(UIWindow pwindow, UIWindowManager.WindowUid pwindowUid, int pindex, int pslotIndex, 
            Vector2 iconSize, Vector2 slotSize)
        {
            window = pwindow;
            windowUid = pwindowUid;
            index = pindex;
            slotIndex = pslotIndex;
            SetCount(0);
            ChangeIconImageSize(iconSize, slotSize);
        }

        public bool IsItem() => IconType == IconConstants.Type.Item;
        public bool IsSkill() => IconType == IconConstants.Type.Skill;
        public bool IsBuff() => IconType == IconConstants.Type.Buff;

        /// <summary>
        /// 장비 타입인지
        /// </summary>
        /// <returns></returns>
        public virtual bool IsEquipType()
        {
            return false;
        }
        /// <summary>
        /// 물약 타입인지
        /// </summary>
        /// <returns></returns>
        public virtual bool IsPotionType()
        {
            return false;
        }
        /// <summary>
        /// 생명력 물약인지
        /// </summary>
        /// <returns></returns>
        public virtual bool IsHpPotionType()
        {
            return false;
        }
        /// <summary>
        /// 마력 물약인지
        /// </summary>
        /// <returns></returns>
        public virtual bool IsMpPotionType()
        {
            return false;
        }
        /// <summary>
        /// 이동속도 증가 물약인지
        /// </summary>
        /// <returns></returns>
        public virtual bool IsIncreaseMoveSpeedPotionType()
        {
            return false;
        }
        /// <summary>
        /// 공격속도 증가 물약인지
        /// </summary>
        /// <returns></returns>
        public virtual bool IsIncreaseAttackSpeedPotionType()
        {
            return false;
        }
        public IconConstants.Type GetIconType() => IconType;
        public IconConstants.Grade GetGrade() => grade;
        public void SetStatus(IconConstants.Status status) => this.iconStatus = status;

        protected virtual bool UpdateInfo()
        {
            if (uid <= 0) return false;
            return true;
        }
        /// <summary>
        /// 다른 uid 로 변경하기
        /// </summary>
        /// <param name="iconUid"></param>
        /// <param name="iconCount"></param>
        /// <param name="iconLevel"></param>
        /// <param name="iconIsLearn"></param>
        /// <param name="remainCoolTime"></param>
        public virtual bool ChangeInfoByUid(int iconUid, int iconCount = 0, int iconLevel = 0, bool iconIsLearn = false, int remainCoolTime = 0)
        {
            CoolTimeHandler?.SetRemainCoolTime(remainCoolTime);
            if (iconUid == 0 && iconCount == 0)
            {
                ClearIconInfos();
                return false;
            }
            uid = iconUid;
            SetCount(iconCount);
            SetLevel(iconLevel);
            SetIsLearn(iconIsLearn);
            return true;
        }
        /// <summary>
        /// 개수 추가하기
        /// </summary>
        /// <param name="value"></param>
        public void AddCount(int value) => SetCount(count + value);
        /// <summary>
        /// 총 개수 가져오기
        /// </summary>
        /// <param name="value"></param>
        public void SetCount(int value)
        {
            SetIconLock(value <= 0);
            count = value;
            if (textCount != null)
            {
                textCount.text = count <= 0 ? "" : count.ToString();
            }
        }
        /// <summary>
        /// 아이템 잠금
        /// </summary>
        /// <param name="set"></param>
        private void SetIconLock(bool set)
        {
            if (imageLock == null) return;
            imageLock.SetActive(set);
        }
        /// <summary>
        /// 아이템 정보 지우기
        /// </summary>
        public virtual void ClearIconInfos()
        {
            CoolTimeHandler?.InitializeCoolTime();
            uid = 0;
            Sprite newSprite = Resources.Load<Sprite>($"Images/UI/blank");
            if (ImageIcon != null)
            {
                ImageIcon.sprite = newSprite;
            }
            if (imageGrade != null)
            {
                imageGrade.sprite = newSprite;
            }
            SetCount(0);
        }
        /// <summary>
        /// 아이콘 이미지 경로 가져오기 
        /// </summary>
        /// <returns></returns>
        protected virtual string GetIconImagePath()
        {
            return "";
        }
        /// <summary>
        /// 아이콘 이미지 업데이트 하기
        /// </summary>
        protected virtual void UpdateIconImage()
        {
            if (ImageIcon == null)
            {
                ImageIcon.sprite = null;
                return;
            }
            string path = GetIconImagePath();
            if (path == null || path == "")
            {
                ImageIcon.sprite = null;
                return;
            }
            ImageIcon.sprite = Resources.Load<Sprite>(path);
        }
        /// <summary>
        /// 이미지 사이즈 변경하기
        /// ray cast 사이즈를 슬롯 사이즈와 같게 변경하기 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="slotSize"></param>
        public void ChangeIconImageSize(Vector2 size, Vector2 slotSize)
        {
            rectTransform.sizeDelta = size;
            var diff = (slotSize.x - size.x)/2;
            ImageIcon.raycastPadding = new Vector4(-diff, -diff, -diff, -diff);
        }
        /// <summary>
        /// 아이콘의 원래 위치 가져오기
        /// </summary>
        /// <returns></returns>
        public Vector3 GetDragOriginalPosition()
        {
            return dragHandler.GetOriginalPosition();
        }
        public void SetSelected(bool selected)
        {
            if (imageSelected == null) return;
            isSelected = selected;
            ShowSelected(selected);
        }
        public void ShowSelected(bool selected)
        {
            if (imageSelected == null) return;
            // 선택된 아이콘이면 끄지 않는다.
            if (isSelected && !selected) return;
            imageSelected.gameObject.SetActive(selected);
        }
        public virtual ItemConstants.PartsType GetPartsType()
        {
            return ItemConstants.PartsType.None;
        }
        /// <summary>
        /// item 테이블에 StatusValue1 컬럼값 가져오기
        /// </summary>
        /// <returns></returns>
        public virtual int GetStatusValue1()
        {
            return 0;
        }
        /// <summary>
        /// item 테이블에 StatusID1 컬럼값 가져오기
        /// </summary>
        /// <returns></returns>
        public virtual string GetStatusId1()
        {
            return "";
        }
        /// <summary>
        /// item 테이블에 duration 컬럼값 가져오기
        /// </summary>
        /// <returns></returns>
        public virtual float GetDuration()
        {
            return 0;
        }

        public void SetPosition(Vector3 position)
        {
            transform.localPosition = position;
        }

        public virtual bool CheckRequireLevel()
        {
            return false;
        }

        private void SetLevel(int value)
        {
            level = value;
        }
        public void SetIsLearn(bool value)
        {
            isLearn = value;
        }
        public int GetLevel() => level;
        public int GetCount() => count;

        public bool IsLearn() => isLearn;
    }
}
