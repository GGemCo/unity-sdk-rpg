using GGemCo.Scripts.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo.Scripts.UI.Icon
{
    public class UIIcon : MonoBehaviour
    {
        [Header("오브젝트")]
        [Tooltip("개수를 표현할 텍스트")]
        public TextMeshProUGUI textCount;
        [Tooltip("선택되었을때 보여줄 이미지")]
        public Image imageSelected;

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

        protected IIcon.Type IconType;
        private IIcon.Status iconStatus;

        // 아이콘 이미지
        private Image imageIcon;
        // 개수
        [HideInInspector] public int count;
        // 등급
        private IIcon.Grade grade;
        // 등급 아이콘
        [HideInInspector] public Image imageGrade;
        // 잠금 아이콘
        [HideInInspector] public GameObject imageLock;
            
        // 드래그 핸들러
        private UIDragHandler dragHandler;
        // 쿨타임 핸들러
        private UICoolTimeHandler coolTimeHandler;
        private RectTransform rectTransform;

        protected virtual void Awake()
        {
            imageIcon = GetComponent<Image>();
            dragHandler = gameObject.AddComponent<UIDragHandler>();
            coolTimeHandler = gameObject.AddComponent<UICoolTimeHandler>();
            rectTransform = GetComponent<RectTransform>();

            iconStatus = IIcon.Status.Normal;
            IconType = IIcon.Type.None;
            SetSelected(false);
        }

        protected virtual void Start()
        {
            if (imageLock != null)
            {
                SetIconLock(count <= 0);
            }
            // 선택되었을때 보여줄 이미지 크기를 slot size 로 변경
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

        public bool IsItem() => IconType == IIcon.Type.Item;

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
        public IIcon.Type GetIconType() => IconType;
        public IIcon.Grade GetGrade() => grade;
        public void SetStatus(IIcon.Status status) => this.iconStatus = status;

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
        /// <param name="remainCoolTime"></param>
        public virtual void ChangeInfoByUid(int iconUid, int iconCount = 0, int remainCoolTime = 0)
        {
            coolTimeHandler?.SetRemainCoolTime(remainCoolTime);
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
        public void ClearIconInfos()
        {
            coolTimeHandler?.InitializeCoolTime();
            uid = 0;
            Sprite newSprite = Resources.Load<Sprite>($"Images/UI/blank");
            if (imageIcon != null)
            {
                imageIcon.sprite = newSprite;
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
        protected void UpdateIconImage()
        {
            if (imageIcon == null)
            {
                imageIcon.sprite = null;
                return;
            }
            string path = GetIconImagePath();
            if (path == null || path == "")
            {
                imageIcon.sprite = null;
                return;
            }
            imageIcon.sprite = Resources.Load<Sprite>(path);
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
            imageIcon.raycastPadding = new Vector4(-diff, -diff, -diff, -diff);
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
    }
}
