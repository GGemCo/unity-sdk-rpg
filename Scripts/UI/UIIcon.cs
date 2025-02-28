using System.Collections.Generic;
using GGemCo.Scripts.Core;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGemCo.Scripts.UI
{
    public class UIIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public enum Grade { None, B, A, S, SS, SSR }
        
        const string GradeNone = "None";
        const string GradeB = "B";
        const string GradeA = "A";
        const string GradeS = "S";
        const string GradeSS = "SS";
        const string GradeSSR = "SSR";
        
        public static readonly Dictionary<Grade, string> IconGradeImagePath = new Dictionary<Grade, string>
        {
            { Grade.B, "rank_b" },
            { Grade.A, "rank_a" },
            { Grade.S, "rank_s" },
            { Grade.SS, "rank_ss" },
            { Grade.SSR, "rank_ssr" }
        };
        public static readonly Dictionary<string, Grade> IconGradeEnum = new Dictionary<string, Grade>
        {
            { GradeNone, Grade.None },
            { GradeB, Grade.B },
            { GradeA, Grade.A },
            { GradeS, Grade.S },
            { GradeSS, Grade.SS },
            { GradeSSR, Grade.SSR }
        };
        public enum Type
        {
            None,
            Item,
            Skill,
            Skin,
            CashShop
        }
        public enum Status
        {
            None,
            Normal,
            Lock,
            Disable
        }
        public UIWindow window;
        public int iconIndex; // 아이콘에 할당된 인덱스
        public int iconSlotIndex; // 슬롯 이동을 위한 인덱스
        public int uid; // 테이블 uid
        protected Type IconType;
        protected bool IsPossibleDrag;
        [SerializeField] private bool isPossibleClick;
    
        private Status iconStatus;
        public Image imageIcon; // 아이템 아이콘
    
        private float currentCoolTime;
        public bool isReverseFillAmount; // true 일경우, 0에서 1로 
        [HideInInspector] public bool isPlayingCoolTime;
        [HideInInspector] public float coolTimeDuration;
        [HideInInspector] public Image coolTimeGauge; // 발동 대기시간 게이지 이미지

        public Vector3 OriginalPosition { get; private set; }

        public int level;
        public int levelMax;
        private Grade grade;
        private int gradeLevel;

        public int count;
        public int composeCount;
        public Image imageGrade;
        public GameObject imageLock;

        public virtual float GetCoolTimeDuration()
        {
            return coolTimeDuration;
        }

        protected void SetRaycastTargetRecursive(Transform target, bool value)
        {
            // Get the Graphic component and set raycastTarget
            Graphic graphic = target.GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.raycastTarget = value;
            }
        }
        public virtual void Awake() {
            Transform textCoolTime = transform.Find("TextCoolTime");
            Transform imageCoolTimeGauge = transform.Find("ImageCoolTimeGauge");
            if (imageCoolTimeGauge != null) {
                coolTimeGauge = imageCoolTimeGauge.GetComponent<Image>();
                coolTimeGauge.gameObject.SetActive(false);
            }

            iconStatus = Status.Normal;
            IconType = Type.None;
            IsPossibleDrag = true;
            levelMax = 0;
        }
        /// <summary>
        /// 아이콘 상태 셋팅
        /// </summary>
        /// <param name="status"></param>
        public void SetStatus(Status status)
        {
            iconStatus = status;
        }
        public virtual void Start()
        {
            // Recursively set raycastTarget for all children
            // 모든 하위 object 의 raycastTarget 값을 false 로 해야 ondrop 이 작동한다 
            foreach (Transform child in transform)
            {
                SetRaycastTargetRecursive(child, false);
            }

            if (isPossibleClick)
            {
                Button button = GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(OnIconClicked);
                }
            }
        }
        /// <summary>
        /// 아이콘 클릭했을때 처리 
        /// </summary>
        protected virtual void OnIconClicked()
        {
            if (window != null)
            {
                window.OnClickIcon(this);
            }
        }
        /// <summary>
        /// 아이콘 드래그 시작할때 처리
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (IsPossibleDrag != true) return;

            transform.SetParent(transform.root); // 드래그하는 동안 최상위로 이동
            GetComponent<Image>().raycastTarget = false; // 드래그 중인 아이콘이 클릭되지 않도록 설정
        
            GameObject draggedIcon = eventData.pointerDrag;
            OriginalPosition = draggedIcon.transform.position;
        }
        /// <summary>
        /// 아이콘 드래그 중일때 처리
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnDrag(PointerEventData eventData)
        {
            if (IsPossibleDrag != true) return;
        
            transform.position = Input.mousePosition; // 마우스 위치로 아이템 이동
        }
        /// <summary>
        /// 아이콘 드래그 종료되었을때 처리 
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (IsPossibleDrag != true) return;
            
            // 드래그 종료 후 아이콘 클릭 가능하게 설정
            // 적용 시점을 밑으로 내릴 경우, 처리되지 않는 경우가 있다
            GetComponent<Image>().raycastTarget = true;
        
            GameObject droppedIcon = eventData.pointerDrag; // 드래그 중인 아이콘
            GameObject originalIcon = eventData.pointerEnter; // 드랍된 위치에 있는 아이콘 또는 슬롯

            if (droppedIcon != null && droppedIcon.GetComponent<UIIcon>() != null)
            {
                window.OnEndDragInIcon(droppedIcon, originalIcon);
            }
            // 슬롯이 아닌 곳 또는 제자리에 드롭되었을 경우 원래 위치로 되돌리기
            else
            {
                GameObject targetSlot = window.slots[this.iconSlotIndex];

                droppedIcon.transform.SetParent(targetSlot.transform);
                droppedIcon.transform.position = OriginalPosition;
            }
        }
        public void Update() {
            UpdateCoolTime();
        }
        /// <summary>
        /// 발동 대기시간 시작하기
        /// </summary>
        /// <returns></returns>
        public virtual bool PlayCoolTime() {
            if (IsLock()) return false;
            if (isPlayingCoolTime) return false;
            if (GetCoolTimeDuration() <= 0) return false;

            isPlayingCoolTime = true;
            currentCoolTime = GetCoolTimeDuration();
            if (coolTimeGauge != null)
            {
                coolTimeGauge.fillAmount = isReverseFillAmount ? 0 : 1; // 아이콘이 완전히 채워짐
                coolTimeGauge.gameObject.SetActive(true);
            }

            return true;
            // ImageIcon.color = new Color(ImageIcon.color.r, ImageIcon.color.g, ImageIcon.color.b, 0.5f); // 흐려짐
        }
        /// <summary>
        /// 발동 대기시간 업데이트
        /// </summary>
        void UpdateCoolTime() {
            if (isPlayingCoolTime != true) return;
            currentCoolTime -= Time.deltaTime;
            if (currentCoolTime <= 0)
            {
                EndCoolTime();
                return;
            }
            // iconImage.fillAmount = currentCooldown / cooldownTime; // 아이콘 채우기
            if (coolTimeGauge != null) {
                if (isReverseFillAmount)
                {
                    coolTimeGauge.fillAmount = 1 - currentCoolTime / GetCoolTimeDuration();
                }
                else
                {
                    coolTimeGauge.fillAmount = currentCoolTime / GetCoolTimeDuration(); // 테두리 줄이기
                }
            }
        }
        /// <summary>
        /// 쿨타임 초기화 하기 
        /// </summary>
        public virtual void InitializeCoolTime()
        {
            isPlayingCoolTime = false;
            currentCoolTime = 0;
            if (coolTimeGauge != null) {
                coolTimeGauge.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 발동 대기시간 종료 되었을때 처리
        /// UIWindowSkill 에서 스킬을 쓰레기통에 버렸을 경우 
        /// </summary>
        public virtual void EndCoolTime() {
            isPlayingCoolTime = false;
            currentCoolTime = 0;
            if (coolTimeGauge != null) {
                coolTimeGauge.gameObject.SetActive(false);
                // coolTimeGauge.fillAmount = 1; // 아이콘이 완전히 채워짐
            }
            // ImageIcon.color = new Color(ImageIcon.color.r, ImageIcon.color.g, ImageIcon.color.b, 1f); // 원래 색상
        }
        /// <summary>
        /// 발동 대기시간 설정하기
        /// </summary>
        /// <param name="time"></param>
        protected void SetCoolTime(float time)
        {
            coolTimeDuration = time;
        }
        /// <summary>
        /// 아이콘 이미지 바꾸기
        /// </summary>
        /// <param name="iconPath"></param>
        protected void ChangeIconImage(string iconPath)
        {
            if (iconPath != "")
            {
                if (imageIcon != null)
                {
                    Sprite newSprite = Resources.Load<Sprite>(iconPath);

                    if (newSprite == null)
                    {
                        GcLogger.LogError("icon 이미지 리소스가 없습니다. uid: "+uid+" / icon path: "+iconPath);
                        return;
                    }
                    imageIcon.sprite = newSprite;
                    // imageIcon.SetNativeSize();
                }
                else
                {
                    GcLogger.LogError("Image 프로퍼티에 값이 없습니다. uid:"+uid);
                }
            }
            else
            {
                GcLogger.LogError("icon 이미지 경로가 없습니다. uid:"+uid);
            }
        }
        /// <summary>
        /// 현재 발동 대기시간 가져오기
        /// </summary>
        /// <returns></returns>
        public float GetCurrentCoolTime()
        {
            return currentCoolTime;
        }
        /// <summary>
        /// 쿨타임이 진행중일때 진행중인 발동 대기시간 적용해주기 
        /// </summary>
        /// <param name="remainCoolTime"></param>
        protected void SetRemainCoolTime(float remainCoolTime)
        {
            if (remainCoolTime <= 0) return;
            isPlayingCoolTime = true;
            currentCoolTime = remainCoolTime;
            if (coolTimeGauge != null)
            {
                coolTimeGauge.gameObject.SetActive(true);
            }
        }
        /// <summary>
        /// 아이콘 드래그 가능 여부 셋팅
        /// </summary>
        /// <param name="set"></param>
        public void SetIsPossibleDrag(bool set)
        {
            IsPossibleDrag = set;
        }
        /// <summary>
        /// 아이콘 드래그 가능 여부 가져오기 
        /// </summary>
        /// <returns></returns>
        public bool GetIsPossibleDrag()
        {
            return IsPossibleDrag;
        }
        /// <summary>
        /// 아이콘 정보 바꾸기
        /// </summary>
        /// <param name="iconUid"></param>
        /// <param name="remainCoolTime"></param>
        public virtual void ChangeInfoByUid(int iconUid, int remainCoolTime = 0)
        {
        
        }
        /// <summary>
        /// Start 함수 호출되기전에 정보 셋팅
        /// </summary>
        public void InitializePreStart(Dictionary<string, string> innerDictionary, int slotIndex, GameObject parentWindow)
        {
            if (innerDictionary == null)
            {
                GcLogger.LogError("아이템 테이블 정보가 없습니다.");
                return;
            }

            if (slotIndex < 0)
            {
                GcLogger.LogError("슬롯 정보가 잘 못 되었습니다. slotIndex: "+slotIndex);
                return;
            }

            if (parentWindow == null)
            {
                GcLogger.LogError("아이콘이 들어갈 윈도우가 없습니다.");
                return;
            }

            window = parentWindow.GetComponent<UIWindow>();
            uid = int.Parse(innerDictionary["Uid"]);
            iconIndex = slotIndex;
            iconSlotIndex = slotIndex;
            ChangeInfoByUid(uid);
        }
        /// <summary>
        /// 클릭 가능 여부 
        /// </summary>
        /// <param name="set"></param>
        public void SetIsPossibleClick(bool set)
        {
            Button button = GetComponent<Button>();
            if (button == null) return;
            if (set)
            {
                button.onClick.AddListener(OnIconClicked);
            }
            else
            {
                button.onClick.RemoveListener(OnIconClicked);
            }
        }
        /// <summary>
        /// 이름 셋팅하기
        /// </summary>
        /// <param name="iconName"></param>
        protected void SetName(string iconName)
        {
            if (iconName == "")
            {
                GcLogger.LogError("아이템 이름이 없습니다.");
                return;
            }
        }
        /// <summary>
        /// 레벨 셋팅하기
        /// </summary>
        /// <param name="iconLevel"></param>
        protected void SetLevel(int iconLevel)
        {
            this.level = iconLevel;
        }

        protected void SetLevelUpCount(int value)
        {
            if (value == 0) return;
            this.composeCount = value;
        }
        /// <summary>
        /// 등급 셋팅하기
        /// </summary>
        /// <param name="valueGrade"></param>
        /// <param name="valueGradeLevel"></param>
        protected void SetGrade(Grade valueGrade, int valueGradeLevel)
        {
            if (valueGrade == UIIcon.Grade.None && imageGrade == null)
            {
                GcLogger.LogError("등급 이미지 오브젝트가 없습니다.");
                return;
            }

            grade = valueGrade;
            gradeLevel = valueGradeLevel;
            Sprite newSprite = Resources.Load<Sprite>(UIWindow.GetIconGradePath(valueGrade, valueGradeLevel));
            imageGrade.sprite = newSprite;
            imageGrade.SetNativeSize();
        }

        public Grade GetGrade()
        {
            return grade;
        }

        public int GetGradeLevel()
        {
            return gradeLevel;
        }
        /// <summary>
        /// 합성에 필요한 개수 설정
        /// </summary>
        /// <param name="value"></param>
        protected void SetComposeCount(int value)
        {
            if (value == 0) return;
            this.composeCount = value;
        }
        /// <summary>
        /// 가지고 있는 개수
        /// </summary>
        /// <param name="value"></param>
        public void SetCount(int value)
        {
            if (value <= 0)
            {
                SetIconLock(true);
            }
            else
            {
                SetIconLock(false);
            }

            this.count = value;
            OnSetCount();
        }
        protected virtual void OnSetCount()
        {
            
        }

        public void AddCount(int value)
        {
            SetCount(count + value);
        }
        /// <summary>
        /// 아이콘 잠금 이미지 보임/안보임
        /// </summary>
        /// <param name="set"></param>
        public void SetIconLock(bool set)
        {
            if (imageLock == null) return;
            if (imageLock.activeSelf == set) return;
            imageLock.SetActive(set);
        }
        /// <summary>
        /// 아이콘이 잠금 상태인지 체크
        /// </summary>
        /// <returns></returns>
        public bool IsLock()
        {
            return imageLock.activeSelf;
        }
        /// <summary>
        /// 아이콘 정보 지우기
        /// </summary>
        public virtual void ClearIconInfos()
        {
            // 초기화 순서 지키기 
            EndCoolTime();
            
            uid = 0;
            if (imageGrade != null)
            {
                Sprite newSprite = Resources.Load<Sprite>($"Images/UI/rank_none");
                imageGrade.sprite = newSprite;
                imageGrade.SetNativeSize();
            }
        }
        public virtual void ChangeTextByEquip()
        {
        }
        public virtual void UpdateInfos()
        {
        }
    }
}
