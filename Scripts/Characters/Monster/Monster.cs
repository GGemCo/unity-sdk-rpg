using GGemCo.Scripts.Characters.Monster.Behavior;
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
    public class Monster : DefaultCharacter, IMonster
    {
        // 어그로
        [HideInInspector] public bool isAggro;
        // 몬스터 데이터
        public MonsterData MonsterData;
        
        // 선공/후공
        private AttackType attackType;
        // 맵에서 지우기까지에 시간
        private float delayDestroy;
        // 공격 속도
        [HideInInspector] public float currentAttackSpeed;
        
        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            currentAttackSpeed = 1f;
            isAggro = false;
            MonsterData = null;
            PossibleAttack = true;

            attackType = AttackType.PassiveDefense;
            delayDestroy = TableLoaderManager.instance.TableConfig.GetDelayDestroyMonster();
            
#if GGEMCO_USE_SPINE
            DefaultCharacterBehavior = gameObject.AddComponent<BehaviorPassiveDefense>();
#else
            DefaultCharacterBehavior = gameObject.AddComponent<BehaviorMonsterSprite>();
#endif
        }
        /// <summary>
        /// tag, sorting layer, layer 셋팅하기
        /// </summary>
        public override void InitTagSortingLayer()
        {
            base.InitTagSortingLayer();
            tag = ConfigTags.GetMonster();
        }
        /// <summary>
        /// 캐릭터에 필요한 컴포넌트 추가하기
        /// </summary>
        protected override void InitComponents()
        {
            base.InitComponents();
            Vector2 offset = Vector2.zero;
            Vector2 size = new Vector2(340,170);
            ComponentController.AddCapsuleCollider2D(gameObject,false, offset, size);
        }
        /// <summary>
        /// regen_data 의 정보 셋팅
        /// </summary>
        protected override void InitializeByRegenData()
        {
            // 맵 배치툴로 저장한 정보가 있을 경우 
            if (MonsterData == null) return;
            SetFlip(MonsterData.IsFlip);
        }
        /// <summary>
        /// 테이블에서 가져온 몬스터 정보 셋팅
        /// </summary>
        protected override void InitializeByTable()
        {
            base.InitializeByTable();
            if (TableLoaderManager.instance == null) return;
            if (Uid <= 0) return;
            TableLoaderManager tableLoaderManager = TableLoaderManager.instance;
            var info = tableLoaderManager.TableMonster.GetMonsterData(Uid);
            // FG_Logger.Log("InitializationStat uid: "+uid+" / info.uid: "+info.uid+" / StatMoveSpeed: "+info.statMoveSpeed);
            if (info.Uid <= 0) return;
            StatAtk = info.StatAtk;
            CurrentAtk = (long)StatAtk;
            currentAttackSpeed = info.StatAttackSpeed;
            StatMoveSpeed = info.StatMoveSpeed;
            CurrentMoveSpeed = StatMoveSpeed;
            CurrentMoveStep = StatMoveStep;
            StatHp = info.StatHp;
            CurrentHp = (long)StatHp;
            float scale = info.Scale;
            SetScale(scale);

            StruckTableAnimation struckTableAnimation = tableLoaderManager.TableAnimation.GetSpineData(info.SpineUid);
            if (struckTableAnimation is { Uid: > 0 })
            {
                CurrentMoveStep = struckTableAnimation.MoveStep;
            }
        }
        /// <summary>
        /// 몬스터에게 데미지 주기 
        /// </summary>
        /// <param name="damage">데미지 수치</param>
        /// <param name="attacker">공격한 대상</param>
        public bool OnDamage(long damage, GameObject attacker)
        {
            if (IsStatusDead())
            {
                // FG_Logger.Log("monster dead");
                return false;
            }
            if (damage <= 0) return false;
                
            CurrentHp -= damage;
            // -1 이면 죽지 않는다
            if (StatHp < 0)
            {
                CurrentHp = 1;
            }

            Vector3 damageTextPosition = transform.position + new Vector3(0, DefaultCharacterBehavior.GetCharacterHeight() * Mathf.Abs(OriginalScaleX), 0);
            SceneGame.Instance.damageTextManager.ShowDamageText(damageTextPosition, damage, Color.white);
            
            if (CurrentHp <= 0)
            {
                SceneGame.Instance.mapManager.OnDeadMonster(Vid);
                
                //FG_Logger.Log("dead vid : " + this.vid);
                CurrentStatus = ICharacter.CharacterStatus.Dead;
                Destroy(gameObject, delayDestroy);

                OnDead();
            }
            else
            {
                if (isAggro == false)
                {
                    isAggro = true;
                }
                CurrentStatus = ICharacter.CharacterStatus.Damage;
                // 후공
                if (attackType == AttackType.PassiveDefense)
                {
                    DefaultCharacterBehavior.SetAttackerTarget(attacker.transform);
                }
            }

            return true;
        }
        /// <summary>
        /// 몬스터가 죽었을때 처리 
        /// </summary>
        private void OnDead()
        {
            DefaultCharacterBehavior.PlayDeadAnimation();
        }
        /// <summary>
        /// 선공 몬스터 처리
        /// </summary>
        /// <param name="collision"></param>
        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(ConfigTags.GetPlayer()))
            {
                CurrentStatus = ICharacter.CharacterStatus.Idle;
                // 선공
                if (attackType == AttackType.AggroFirst && isAggro == false)
                {
                    isAggro = true;
                    DefaultCharacterBehavior.SetAttackerTarget(collision.gameObject.transform);
                }
            }
        }
    }
}
