using System.Collections;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.TableLoader;
#if GGEMCO_USE_SPINE
using Spine.Unity;
#endif
using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    /// <summary>
    /// 기본 캐릭터 클레스, 스탯 관리, 상태 관리, order
    /// </summary>
    ///<inheritdoc cref="ICharacter"/>
    public class DefaultCharacter : MonoBehaviour, ICharacter
    {
        public int Uid { get; set; }
        // 스폰될때 vid
        public int Vid { get; set; }
        public float StatHp { get; set; }
        public float StatAtk { get; set; }
        public float StatMoveStep { get; set; }
        public float StatMoveSpeed { get; set; }
        
        public long CurrentHp { get; set; }
        public long CurrentAtk { get; set; }
        public float CurrentMoveStep { get; set; }
        public float CurrentMoveSpeed { get; set; }
        public ICharacter.CharacterStatus CurrentStatus { get; set; }
        
        public ICharacter.CharacterSortingOrder SortingOrder { get; set; }
        
        public float OriginalScaleX { get; set; }
        public bool IsAttacking { get; set; }
        public bool PossibleAttack { get; set; }
        public string MyTag { get; set; }
        public string TargetTag { get; set; }

        public bool flip;
        
        private Renderer characterRenderer;

        protected DefaultCharacterBehavior DefaultCharacterBehavior;

        private bool isPossibleFlip = true;
        private ICharacter characterImplementation;
        private bool isStartFade;
        
#if GGEMCO_USE_SPINE
        private SkeletonAnimation skeletonAnimation;
#else
        private SpriteRenderer spriteRenderer;
#endif
        
        protected virtual void Awake()
        {
            StatAtk = 100;
            StatHp = 100;
            StatMoveStep = 10f;
            StatMoveSpeed = 1f;
            
            IsAttacking = false;
            CurrentStatus = ICharacter.CharacterStatus.None;
            
            SetScale(1f);
            characterRenderer = GetComponent<Renderer>();
#if GGEMCO_USE_SPINE
            skeletonAnimation = GetComponent<SkeletonAnimation>();
#else
            spriteRenderer = GetComponent<SpriteRenderer>();
#endif
            InitComponents();
            InitTagSortingLayer();
        }
        /// <summary>
        /// tag, sorting layer, layer 셋팅하기
        /// </summary>
        public virtual void InitTagSortingLayer()
        {
            if (characterRenderer == null)
            {
                characterRenderer = GetComponent<Renderer>();
            }
            characterRenderer.sortingLayerName = ConfigSortingLayer.Character;
        }
        /// <summary>
        /// 캐릭터에 필요한 컴포넌트 추가하기
        /// </summary>
        protected virtual void InitComponents()
        {
            
        }
        protected virtual void Start()
        {
            // statatk 값들은 table 에서 불러올 수 있기 때문에 Start 에서 처리한다.
            CurrentAtk = (long)StatAtk;
            CurrentHp = (long)StatHp;
            CurrentMoveStep = StatMoveStep;
            CurrentMoveSpeed = StatMoveSpeed;
            OriginalScaleX = transform.localScale.x;
            InitializeByTable();
            InitializeByRegenData();
        }
        /// <summary>
        /// 테이블에서 가져온 몬스터 정보 셋팅
        /// </summary>
        protected virtual void InitializeByTable()
        {
            if (TableLoaderManager.instance == null) return;
            if (Uid <= 0) return;
        }
        /// <summary>
        /// regen_data 의 정보 셋팅
        /// </summary>
        protected virtual void InitializeByRegenData()
        {
            
        }
        /// <summary>
        /// 캐릭터가 flip 되었는지 체크
        /// <para>
        /// 디폴트는 왼쪽을 바라봄
        /// </para>
        /// </summary>
        /// <returns></returns>
        public bool IsFlipped() {
            return Mathf.Approximately(transform.localScale.x, (OriginalScaleX * -1f));
        }

        public void SetIsPossibleFlip(bool set) => isPossibleFlip = set;

        private bool IsPossibleFlip() => isPossibleFlip;
        /// <summary>
        /// 캐릭터 방향 셋팅하기
        /// </summary>
        /// <param name="flip"></param>
        public void SetFlip(bool flip)
        {
            if (IsPossibleFlip() != true) return;

            transform.localScale = flip ? new Vector3(OriginalScaleX * -1f, transform.localScale.y, transform.localScale.z) : new Vector3(OriginalScaleX, transform.localScale.y, transform.localScale.z);
            this.flip = flip;
        }
        /// <summary>
        /// 타겟 오브젝트가 있을경우 방향 셋팅하기
        /// </summary>
        /// <param name="targetTransform"></param>
        protected void SetFlipToTarget(Transform targetTransform)
        {
            SetFlip(transform.position.x <= targetTransform.position.x);
        }
        /// <summary>
        /// 공격 버튼 눌렀을때 처리 
        /// </summary>
        protected virtual bool IsPossibleAttack()
        {
            return CurrentStatus != ICharacter.CharacterStatus.Attack &&
                   CurrentStatus != ICharacter.CharacterStatus.Dead &&
                   PossibleAttack;
        }
        /// <summary>
        /// 캐릭터가 이동할 수 있는 상태인지 
        /// </summary>
        /// <returns></returns>
        private bool IsPossibleRun()
        {
            return !IsAttacking && CurrentMoveSpeed > 0 &&
                   (CurrentStatus == ICharacter.CharacterStatus.Idle || CurrentStatus == ICharacter.CharacterStatus.None);
        }
        /// <summary>
        /// 플레이어 이동 시작 
        /// </summary>
        public void Run() 
        {
            if (!IsPossibleRun() || CurrentStatus == ICharacter.CharacterStatus.Run) return;
            SetStatus(ICharacter.CharacterStatus.Run);
        }
        /// <summary>
        ///  플레이어 움직임 멈춤 
        /// </summary>
        public void Stop() {
            // FG_Logger.Log("player Stop");
            SetStatus(ICharacter.CharacterStatus.Idle);
        }
        public void SetSortingOrder(ICharacter.CharacterSortingOrder value)
        {
            SortingOrder = value;
        }
        private void UpdatePosition()
        {
            if (SortingOrder == ICharacter.CharacterSortingOrder.Fixed) return;

            int baseSortingOrder = SortingOrder switch
            {
                ICharacter.CharacterSortingOrder.AlwaysOnTop => 32767,
                ICharacter.CharacterSortingOrder.AlwaysOnBottom => -32768,
                _ => -(int)(transform.position.y * 100)
            };

            characterRenderer.sortingOrder = baseSortingOrder;
        }
        protected virtual void Update()
        {
            if (IsStatusDead()) return;
            UpdatePosition();
        }
        /// <summary>
        /// 강제로 이동시키기
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MoveForce(float x, float y)
        {
            transform.position = new Vector3(x, y, transform.position.z);
        }
        public void SetMoveSpeed(float speed)
        {
            CurrentMoveSpeed = Mathf.Approximately(speed, -1) ? StatMoveSpeed : speed;
        }
        
        public bool IsStatusDead() => CurrentStatus == ICharacter.CharacterStatus.Dead;
        protected bool IsStatusAttack() => CurrentStatus == ICharacter.CharacterStatus.Attack;
        protected bool IsStatusRun() => CurrentStatus == ICharacter.CharacterStatus.Run;
        protected bool IsStatusIdle() => CurrentStatus == ICharacter.CharacterStatus.Idle;
        protected bool IsStatusNone() => CurrentStatus == ICharacter.CharacterStatus.None;

        private void SetStatus(ICharacter.CharacterStatus value) => CurrentStatus = value;
        protected void SetStatusDead() => SetStatus(ICharacter.CharacterStatus.Dead);
        protected void SetStatusIdle() => SetStatus(ICharacter.CharacterStatus.Idle);
        protected void SetStatusRun() => SetStatus(ICharacter.CharacterStatus.Run);

        public void SetScale(float scale)
        {
            transform.localScale = new Vector3(scale, scale, 0);
            OriginalScaleX = scale;
        }
        public void StartFadeIn()
        {
            if (isStartFade) return;
            isStartFade = true;
            gameObject.SetActive(true);
            StartCoroutine(FadeIn(ConfigCommon.CharacterFadeSec));
        }
        public void StartFadeOut()
        {
            if (isStartFade) return;
            isStartFade = true;
            StartCoroutine(FadeOut(ConfigCommon.CharacterFadeSec));
        }
        
        private IEnumerator FadeIn(float duration)
        {
            yield return FadeEffect(duration, true);
        }

        private IEnumerator FadeOut(float duration)
        {
            yield return FadeEffect(duration, false);
            gameObject.SetActive(false);
        }
        private IEnumerator FadeEffect(float duration, bool fadeIn)
        {
            yield return DefaultCharacterBehavior.FadeEffect(duration, fadeIn);
            
//             float elapsedTime = 0f;
//             float startAlpha = fadeIn ? 0 : 1;
//             float endAlpha = fadeIn ? 1 : 0;
//
// #if GGEMCO_USE_SPINE
//             Color color = skeletonAnimation.Skeleton.GetColor();
// #else
//             Color color = spriteRenderer.color;
// #endif
//
//             while (elapsedTime < duration)
//             {
//                 elapsedTime += Time.deltaTime;
//                 color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
// #if GGEMCO_USE_SPINE
//                 skeletonAnimation.Skeleton.SetColor(color);
// #else
//                 spriteRenderer.color = color;
// #endif
//                 yield return null;
//             }
//
//             isStartFade = false;
        }
        public void Move(Vector3 direction)
        {
            throw new System.NotImplementedException();
        }

        public void TakeDamage(int damage)
        {
            throw new System.NotImplementedException();
        }

        public void SetIsStartFade(bool value)
        {
            isStartFade = value;
        }
    }
}