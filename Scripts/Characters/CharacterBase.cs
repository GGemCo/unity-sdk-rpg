using System.Collections;
using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Characters.Player;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    public class CharacterBase : CharacterStat
    {
        /// <summary>
        /// 캐릭터 상태
        /// </summary>
        protected enum CharacterStatus
        {
            None,
            /// <summary>
            /// 기본 상태
            /// </summary>
            Idle,
            /// <summary>
            /// 움직이는 중
            /// </summary>
            Run,
            /// <summary>
            /// 공격 중
            /// </summary>
            Attack,
            /// <summary>
            /// 데미지 입는 중
            /// </summary>
            Damage,
            /// <summary>
            /// 죽음
            /// </summary>
            Dead,
            /// <summary>
            /// 움직이지 못함
            /// </summary>
            DontMove
        }
        /// <summary>
        /// 캐릭터 등급
        /// </summary>
        public enum Grade
        {
            None,
            Common,
            Boss,
        }
        /// <summary>
        /// 캐릭터 정렬
        /// </summary>
        public enum CharacterSortingOrder
        {
            Normal,
            AlwaysOnTop,
            AlwaysOnBottom,
            Fixed
        }
        public int Uid { get; set; }
        // 스폰될때 vid
        public int Vid { get; set; }
        public long CurrentHp { get; set; }
        public float CurrentMoveStep { get; set; }
        protected CharacterStatus CurrentStatus { get; set; }

        private CharacterSortingOrder SortingOrder { get; set; }
        
        public float OriginalScaleX { get; set; }
        public bool IsAttacking { get; set; }
        public bool PossibleAttack { get; set; }
        public string MyTag { get; set; }
        public string TargetTag { get; set; }

        public bool flip;
        
        private Renderer characterRenderer;
        private bool isPossibleFlip = true;
        private bool isStartFade;
        
        private EquipController equipController;

        public ICharacterAnimationController CharacterAnimationController;
        // 맵에서 지우기까지에 시간
        private float delayDestroy;
        public Transform AttackerTransform;
        
        public Vector3 direction;
        public Vector3 directionPrev;
        public CharacterManager.Type type;
        
        protected override void Awake()
        {
            base.Awake();
            IsAttacking = false;
            CurrentStatus = CharacterStatus.None;
            delayDestroy = AddressableSettingsLoader.Instance.settings.delayDestroyMonster;
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
            characterRenderer.sortingLayerName = ConfigSortingLayer.GetValue(ConfigSortingLayer.Keys.Character);
        }
        /// <summary>
        /// 캐릭터에 필요한 컴포넌트 추가하기
        /// </summary>
        protected virtual void InitComponents()
        {
            
        }
        protected override void Start()
        {
            base.Start();
            OriginalScaleX = transform.localScale.x;
            
            InitializeByTable();
            InitializeByRegenData();
            equipController = GetComponent<EquipController>();
        }
        /// <summary>
        /// 테이블에서 가져온 몬스터 정보 셋팅
        /// </summary>
        protected virtual void InitializeByTable()
        {
            if (TableLoaderManager.Instance == null) return;
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
        /// <param name="value"></param>
        public void SetFlip(bool value)
        {
            if (IsPossibleFlip() != true) return;

            transform.localScale = value ? new Vector3(OriginalScaleX * -1f, transform.localScale.y, transform.localScale.z) : new Vector3(OriginalScaleX, transform.localScale.y, transform.localScale.z);
            flip = value;
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
            return CurrentStatus != CharacterStatus.Attack &&
                   CurrentStatus != CharacterStatus.Dead &&
                   PossibleAttack;
        }
        /// <summary>
        /// 캐릭터가 이동할 수 있는 상태인지 
        /// </summary>
        /// <returns></returns>
        private bool IsPossibleRun()
        {
            return !IsAttacking && GetCurrentMoveSpeed() > 0 &&
                   (CurrentStatus == CharacterStatus.Idle || CurrentStatus == CharacterStatus.None);
        }
        private void UpdatePosition()
        {
            if (SortingOrder == CharacterSortingOrder.Fixed) return;

            int baseSortingOrder = SortingOrder switch
            {
                CharacterSortingOrder.AlwaysOnTop => 32767,
                CharacterSortingOrder.AlwaysOnBottom => -32768,
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
        public bool IsStatusDead() => CurrentStatus == CharacterStatus.Dead;
        protected bool IsStatusAttack() => CurrentStatus == CharacterStatus.Attack;
        protected bool IsStatusRun() => CurrentStatus == CharacterStatus.Run;
        protected bool IsStatusIdle() => CurrentStatus == CharacterStatus.Idle;
        protected bool IsStatusNone() => CurrentStatus == CharacterStatus.None;

        private void SetStatus(CharacterStatus value) => CurrentStatus = value;
        protected void SetStatusDead() => SetStatus(CharacterStatus.Dead);
        protected void SetStatusIdle() => SetStatus(CharacterStatus.Idle);
        protected void SetStatusRun() => SetStatus(CharacterStatus.Run);

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
            // yield return DefaultCharacterBehavior.FadeEffect(duration, fadeIn);
            yield break;
        }
        public void SetIsStartFade(bool value)
        {
            isStartFade = value;
        }
        private float characterHeight;
        public virtual float GetCharacterHeight()
        {
            return characterHeight;
        }
        public virtual void SetHeight(float height)
        {
            characterHeight = height;
        }
        public virtual float GetCurrentMoveStep()
        {
            return CurrentMoveStep;
        }

        public virtual void OnSpineEventAttack()
        {
        }
        /// <summary>
        /// 플레이어와 몬스터가 마주보고 있는지 체크 
        /// </summary>
        /// <param name="monster"></param>
        /// <returns></returns>
        protected bool AreFacingEachOther(Transform monster)
        {
            float playerDir = Mathf.Sign(transform.localScale.x);  // 플레이어 방향 (오른쪽: -1, 왼쪽: 1)
            float monsterDir = Mathf.Sign(monster.localScale.x); // 몬스터 방향 (오른쪽: -1, 왼쪽: 1)

            float directionToMonster = Mathf.Sign(monster.position.x - transform.position.x);

            return Mathf.Approximately(playerDir, -directionToMonster) && Mathf.Approximately(monsterDir, directionToMonster);
        }
        /// <summary>
        /// 플레이어가 죽었을때 처리 
        /// </summary>
        protected virtual void OnDead()
        {
            PlayDeadAnimation();
        }

        private void PlayDeadAnimation()
        {
            CharacterAnimationController.PlayDeadAnimation();
        }
        /// <summary>
        /// 내가 데미지 받았을때 처리 
        /// </summary>
        /// <param name="damage">받은 데미지</param>
        /// <param name="attacker">누가 때렸는지</param>
        public bool TakeDamage(long damage, GameObject attacker)
        {
            if (IsStatusDead())
            {
                // GcLogger.Log("monster dead");
                return false;
            }
            if (damage <= 0) return false;

            CurrentHp -= damage;
            // -1 이면 죽지 않는다
            if (BaseHp < 0)
            {
                CurrentHp = 1;
            }

            Vector3 damageTextPosition = transform.position + new Vector3(0, GetCharacterHeight() * Mathf.Abs(OriginalScaleX), 0);
            SceneGame.Instance.damageTextManager.ShowDamageText(damageTextPosition, damage, Color.red);
            
            if (CurrentHp <= 0)
            {
                CurrentStatus = CharacterStatus.Dead;
                Destroy(this.gameObject, delayDestroy);

                OnDead();
            }
            else
            {
                CurrentStatus = CharacterStatus.Damage;
                OnDamage(attacker);
            }

            return true;
        }

        protected virtual void OnDamage(GameObject attacker)
        {
            
        }
        /// <summary>
        /// 공격한 오브젝트 설정하기 
        /// </summary>
        /// <param name="attacker"></param>
        protected void SetAttackerTarget(Transform attacker)
        {
            AttackerTransform = attacker;
        }
    }
}