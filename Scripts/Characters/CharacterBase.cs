using System.Collections;
using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Scenes;
using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    /// <summary>
    /// 캐릭터 공용 
    /// </summary>
    public class CharacterBase : CharacterStat
    {
        private const int SortingOrderTop = 32767;
        private const int SortingOrderBottom = -32768;
        /// <summary>
        /// 캐릭터 상태
        /// </summary>
        public enum CharacterStatus
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
            /// 죽음
            /// </summary>
            Dead,
            /// <summary>
            /// 움직이지 못함
            /// </summary>
            DontMove,
            /// <summary>
            /// 움직이지 못함
            /// </summary>
            CastingSkill,
            UseSkill,
        }

        /// <summary>
        /// 캐릭터 등급
        /// </summary>
        public enum Grade
        {
            None,
            Common,
            Boss
        }
        /// <summary>
        /// 캐릭터 정렬
        /// </summary>
        private enum CharacterSortingOrder
        {
            Normal,
            AlwaysOnTop,
            AlwaysOnBottom,
            Fixed
        }

        [Header("캐릭터 정보")]
        // 캐릭터 타입
        public CharacterManager.Type type;
        // 캐릭터 테이블 Uid
        public int uid;
        // 스폰될때 부여되는 가상번호 vid
        public int vid;
        // 현재 이동 스텝
        public float currentMoveStep;
        // 어그로
        public bool isAggro;
        
        [Header("캐릭터 방향 관련")]
        // 좌우 flip 여부
        public bool isFlip;
        // 방향
        public Vector3 direction;
        // 좌우 방향 체크에 사용
        public Vector3 directionPrev;
        // 좌우 flip 가능 여부
        private bool isPossibleFlip = true;
        // 초기 scale x 값
        public float originalScaleX;
        
        [Header("애니메이션 및 렌더링 관련")]
        // 애니메이션 컨트롤러
        public ICharacterAnimationController CharacterAnimationController;
        private Renderer characterRenderer;
        private CharacterSortingOrder sortingOrder;
        
        [Header("상태 및 스탯")]
        // 현재 hp
        protected long CurrentHp;
        // 현재 상태
        protected CharacterStatus CurrentStatus;
        // 몬스터 죽은 후 맵에서 지우기까지에 시간
        private float delayDestroyMonster;
        // fade in, out 효과 시작 여부. 맵에서 컬링 될때 사용
        private bool isStartFade;
        private float characterHeight;
        // 공격한 GameObject 의 Transform
        public Transform attackerTransform;
        
        protected override void Awake()
        {
            base.Awake();
            isAggro = false;
            SetStatusIdle();
            InitComponents();
            InitTagSortingLayer();
            delayDestroyMonster = AddressableSettingsLoader.Instance.settings.delayDestroyMonster;
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
            originalScaleX = transform.localScale.x;
            
            InitializeByTable();
            InitializeByRegenData();
        }
        /// <summary>
        /// 테이블에서 가져온 몬스터 정보 셋팅
        /// </summary>
        protected virtual void InitializeByTable()
        {
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
            return Mathf.Approximately(transform.localScale.x, (originalScaleX * -1f));
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

            transform.localScale =
                value
                    ? new Vector3(originalScaleX * -1f, transform.localScale.y, transform.localScale.z)
                    : new Vector3(originalScaleX, transform.localScale.y, transform.localScale.z);
            isFlip = value;
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
        /// 캐릭터 순서. sorting order 처리 
        /// </summary>
        private void UpdatePosition()
        {
            if (sortingOrder == CharacterSortingOrder.Fixed) return;

            int baseSortingOrder = sortingOrder switch
            {
                CharacterSortingOrder.AlwaysOnTop => SortingOrderTop,
                CharacterSortingOrder.AlwaysOnBottom => SortingOrderBottom,
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
        public bool IsStatusAttack() => CurrentStatus == CharacterStatus.Attack;
        public bool IsStatusRun() => CurrentStatus == CharacterStatus.Run;
        public bool IsStatusIdle() => CurrentStatus == CharacterStatus.Idle;
        public bool IsStatusNone() => CurrentStatus == CharacterStatus.None;

        public CharacterStatus GetCurrentStatus() => CurrentStatus;
        
        private void SetStatus(CharacterStatus value) => CurrentStatus = value;
        public void SetStatusDead() => SetStatus(CharacterStatus.Dead);
        public void SetStatusIdle() => SetStatus(CharacterStatus.Idle);
        public void SetStatusRun() => SetStatus(CharacterStatus.Run);
        public void SetStatusAttack() => SetStatus(CharacterStatus.Attack);

        public void SetScale(float scale)
        {
            transform.localScale = new Vector3(scale, scale, 0);
            originalScaleX = scale;
        }
        /// <summary>
        /// fade in 효과 시작. 맵 컬링시 사용
        /// </summary>
        public void StartFadeIn()
        {
            if (isStartFade) return;
            isStartFade = true;
            gameObject.SetActive(true);
            StartCoroutine(FadeIn(ConfigCommon.CharacterFadeSec));
        }
        /// <summary>
        /// fade out 효과 시작. 맵 컬링시 사용
        /// </summary>
        public void StartFadeOut()
        {
            if (isStartFade) return;
            isStartFade = true;
            StartCoroutine(FadeOut(ConfigCommon.CharacterFadeSec));
        }
        
        private IEnumerator FadeIn(float duration)
        {
            yield return CharacterAnimationController.FadeEffect(duration, true);
        }

        private IEnumerator FadeOut(float duration)
        {
            yield return CharacterAnimationController.FadeEffect(duration, false);
            gameObject.SetActive(false);
        }
        public void SetIsStartFade(bool value)
        {
            isStartFade = value;
        }

        protected virtual float GetCharacterHeight()
        {
            return characterHeight;
        }
        public virtual void SetHeight(float height)
        {
            characterHeight = height;
        }
        public virtual float GetCurrentMoveStep()
        {
            return currentMoveStep;
        }
        /// <summary>
        /// attack 이벤트 처리 
        /// </summary>
        public virtual void OnEventAttack()
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
        /// 캐릭터가 죽었을때 처리 
        /// </summary>
        protected virtual void OnDead()
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

            Vector3 damageTextPosition = transform.position + new Vector3(0, GetCharacterHeight() * Mathf.Abs(originalScaleX), 0);
            SceneGame.Instance.damageTextManager.ShowDamageText(damageTextPosition, damage, Color.red);
            
            if (CurrentHp <= 0)
            {
                CurrentStatus = CharacterStatus.Dead;
                Destroy(this.gameObject, delayDestroyMonster);

                OnDead();
            }
            else
            {
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
            attackerTransform = attacker;
        }
    }
}