using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.Characters.Monster
{
    public enum AttackType
    {
        PassiveDefense, // 후공
        AggroFirst // 선공
    }
    /// <summary>
    /// 몬스터 기본 클레스
    /// </summary>
    public class Monster : CharacterBase
    {
        // 어그로
        public bool isAggro;
        // 몬스터 데이터
        public MonsterData MonsterData;
        
        // 선공/후공
        private AttackType attackType;
        
        // 1. 델리게이트 선언
        public delegate void DelegateMonsterDead(int monsterVid, int monsterUid, GameObject monsterObject);
        // 2. 델리게이트 이벤트 정의
        public event DelegateMonsterDead OnMonsterDead;
        private ControllerMonster controllerMonster;
        
        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            isAggro = false;
            MonsterData = null;
            PossibleAttack = true;

            attackType = AttackType.PassiveDefense;
            
            OnMonsterDead += SceneGame.Instance.itemManager.OnMonsterDead;
            OnMonsterDead += SceneGame.Instance.saveDataManager.Player.AddExp;
        }

        protected override void Start()
        {
            base.Start();
            controllerMonster = GetComponent<ControllerMonster>();
        }

        /// <summary>
        /// tag, sorting layer, layer 셋팅하기
        /// </summary>
        public override void InitTagSortingLayer()
        {
            base.InitTagSortingLayer();
            tag = ConfigTags.GetValue(ConfigTags.Keys.Monster);
        }
        /// <summary>
        /// 캐릭터에 필요한 컴포넌트 추가하기
        /// </summary>
        protected override void InitComponents()
        {
            base.InitComponents();
            Vector2 offset = Vector2.zero;
            Vector2 size = new Vector2(0,0);
            ComponentController.AddCapsuleCollider2D(gameObject,false, offset, size);
        }
        /// <summary>
        /// regen_data 의 정보 셋팅
        /// </summary>
        protected override void InitializeByRegenData()
        {
            // 맵 배치툴로 저장한 정보가 있을 경우 
            if (MonsterData == null) return;
            // UpdateDirection() 에서 초기 방향 처리를 위해 추가
            direction = new Vector3(MonsterData.IsFlip?1:-1, 0, 0);
            directionPrev = new Vector3(MonsterData.IsFlip?1:-1, 0, 0);
            SetFlip(MonsterData.IsFlip);
        }
        /// <summary>
        /// 테이블에서 가져온 몬스터 정보 셋팅
        /// </summary>
        protected override void InitializeByTable()
        {
            base.InitializeByTable();
            if (TableLoaderManager.Instance == null) return;
            if (Uid <= 0) return;
            TableLoaderManager tableLoaderManager = TableLoaderManager.Instance;
            var info = tableLoaderManager.TableMonster.GetDataByUid(Uid);
            // GcLogger.Log("InitializationStat uid: "+uid+" / info.uid: "+info.uid+" / StatMoveSpeed: "+info.statMoveSpeed);
            if (info.Uid <= 0) return;
            SetBaseInfos(info.StatAtk, info.StatDef, info.StatHp, 0, info.StatMoveSpeed, info.StatAttackSpeed);
            CurrentHp = info.StatHp;
            float scale = info.Scale;
            SetScale(scale);

            StruckTableAnimation struckTableAnimation = tableLoaderManager.TableAnimation.GetDataByUid(info.SpineUid);
            if (struckTableAnimation is { Uid: > 0 })
            {
                CurrentMoveStep = struckTableAnimation.MoveStep;
                CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
                if (capsule != null)
                {
                    capsule.size = struckTableAnimation.ColliderSize;
                }
            }
        }
        protected override void OnDamage(GameObject attacker)
        {
            if (isAggro == false)
            {
                isAggro = true;
            }
            // 후공
            if (attackType == AttackType.PassiveDefense)
            {
                SetAttackerTarget(attacker.transform);
            }
        }
        /// <summary>
        /// 몬스터가 죽었을때 처리 
        /// </summary>
        protected override void OnDead()
        {
            base.OnDead();
            controllerMonster.StopAttackCoroutine();
            OnMonsterDead?.Invoke(Vid, Uid, gameObject);
        }
        /// <summary>
        /// 선공 몬스터 처리
        /// </summary>
        /// <param name="collision"></param>
        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Player)))
            {
                if (IsStatusDead()) return;
                
                CurrentStatus = CharacterStatus.Idle;
                // 선공
                if (attackType == AttackType.AggroFirst && isAggro == false)
                {
                    isAggro = true;
                    SetAttackerTarget(collision.gameObject.transform);
                }
            }
        }

        protected void OnDestroy()
        {
            OnMonsterDead -= SceneGame.Instance.itemManager.OnMonsterDead;
            OnMonsterDead -= SceneGame.Instance.saveDataManager.Player.AddExp;
        }
        public override void OnSpineEventAttack()
        {
            if (IsStatusDead()) return;
            // GcLogger.Log(@event);
            long totalDamage = SceneGame.Instance.calculateManager.GetMonsterTotalAtk(Uid);
        
            // 캡슐 콜라이더 2D와 충돌 중인 모든 콜라이더를 검색
            CapsuleCollider2D capsuleCollider2D = GetComponent<CapsuleCollider2D>();
            Vector2 size = new Vector2(capsuleCollider2D.size.x * Mathf.Abs(transform.localScale.x), capsuleCollider2D.size.y * transform.localScale.y);
            Vector2 point = (Vector2)transform.position + capsuleCollider2D.offset * transform.localScale;
            Collider2D[] collider2Ds = Physics2D.OverlapCapsuleAll(point, size, capsuleCollider2D.direction, 0f);

            foreach (var hit in collider2Ds)
            {
                if (hit.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Player)))
                {
                    Player.Player player = hit.GetComponent<Player.Player>();
                    if (player != null)
                    {
                        // GcLogger.Log("Player attacked the monster after animation!");
                        player.TakeDamage(totalDamage, gameObject);
                        break;
                    }
                }
            }
        }
    }
}
