using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Maps.Objects;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using UnityEngine;

namespace GGemCo.Scripts.Characters.Player
{
    /// <summary>
    /// 플레이어 기본 클레스
    /// </summary>
    public class Player : DefaultCharacter, IPlayer
    {
        // 공격할 몬스터 
        private GameObject targetMonster;
        // 플레이어 주변에 얼마나 많은 몬스터에게 데미지를 입힐 것인지
        [HideInInspector] public Collider2D[] hits;
        // 주변에 npc 가 있는지 체크 
        protected bool IsNpcNearby;
        
        public float startScale;

        private GGemCoSettings gGemCoSettings;
        protected override void Awake()
        {
            base.Awake();
            IsNpcNearby = false;
#if GGEMCO_USE_SPINE
            DefaultCharacterBehavior = gameObject.AddComponent<BehaviorPlayerSpine>();
#else
            DefaultCharacterBehavior = gameObject.AddComponent<BehaviorPlayerSprite>();
#endif
        }

        protected override void Start()
        {
            base.Start();
            hits = new Collider2D[TableLoaderManager.instance.TableConfig.GetMaxEnemyValue()];
        }

        /// <summary>
        /// 테이블에서 가져온 몬스터 정보 셋팅
        /// </summary>
        protected override void InitializeByTable() 
        {
            if (TableLoaderManager.instance != null)
            {
                StatAtk = (long)TableLoaderManager.instance.TableConfig.GetPolyPlayerStatAtk();
                CurrentAtk = (long)StatAtk;
                CurrentHp = (long)StatHp;
                StatMoveStep = TableLoaderManager.instance.TableConfig.GetPolyPlayerStatMoveStep();
                StatMoveSpeed = TableLoaderManager.instance.TableConfig.GetPolyPlayerStatMoveSpeed();
                CurrentMoveSpeed = StatMoveSpeed;
                CurrentMoveStep = StatMoveStep;
                OriginalScaleX = transform.localScale.x;
            }

            if (startScale > 0)
            {
                SetScale(startScale);
            }
        }
        /// <summary>
        /// 공격 버튼 눌렀을때 처리 
        /// </summary>
        public bool Attack()
        {
            if (!IsPossibleAttack()) return false;
            if (!SearchAndAttackMonsters()) return false;

            SetFlipToTarget(targetMonster.transform);
            // int attackNameIndex = Random.Range(0, attackAniNames.Length);
            return true;
        }
        /// <summary>
        /// 내가 데미지 받았을때 처리 
        /// </summary>
        /// <param name="damage">받은 데미지</param>
        /// <param name="monster">누가 때렸는지</param>
        public bool OnDamage(long damage, GameObject monster)
        {
            if (damage <= 0) return false;
            
            if (CurrentStatus == ICharacter.CharacterStatus.Dead)
            {
                // FG_Logger.Log("monster dead");
                return false;
            }
                
            CurrentHp -= damage;
            // -1 이면 죽지 않는다
            if (StatHp < 0)
            {
                CurrentHp = 1;
            }

            Vector3 damageTextPosition = transform.position + new Vector3(0, DefaultCharacterBehavior.GetCharacterHeight() * Mathf.Abs(OriginalScaleX), 0);
            SceneGame.Instance.damageTextManager.ShowDamageText(damageTextPosition, damage, Color.red);
            
            if (CurrentHp <= 0)
            {
                CurrentStatus = ICharacter.CharacterStatus.Dead;
                float delay = 1.3f;
                Destroy(this.gameObject, delay);

                OnDead();
            }
            else
            {
                CurrentStatus = ICharacter.CharacterStatus.Damage;
            }

            return true;
        }
        /// <summary>
        /// 플레이어가 죽었을때 처리 
        /// </summary>
        private void OnDead()
        {
            DefaultCharacterBehavior.PlayDeadAnimation();
        }

        private bool SearchAndAttackMonsters()
        {
            // 캡슐 콜라이더 2D와 충돌 중인 모든 콜라이더를 검색
            CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
            Vector2 size = new Vector2(capsuleCollider.size.x * Mathf.Abs(transform.localScale.x), capsuleCollider.size.y * transform.localScale.y);
            Vector2 point = transform.position;
            int hitCount = Physics2D.OverlapCapsuleNonAlloc(point, size, capsuleCollider.direction, 0f, hits);
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = hits[i];
                if (hit.CompareTag(ConfigTags.Monster))
                {
                    Monster.Monster monster = hit.GetComponent<Monster.Monster>();
                    if (monster != null)
                    {
                        // FgLogger.Log("Player attacked the monster after animation!");
                        targetMonster = monster.gameObject;
                        return true;
                    }
                }
            }

            return false;
        }
        protected void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(ConfigTags.Monster))
            {
                IsAttacking = true;
                Monster.Monster monster = collision.gameObject.GetComponent<Monster.Monster>();
                if (monster.gameObject.GetComponent<Monster.Monster>().IsStatusDead())
                {
                    // FG_Logger.Log("player / update / monster dead");
                    targetMonster = null;
                }
                else {
                    targetMonster = monster.gameObject;
                    Attack();
                }
            }
            else if (collision.gameObject.CompareTag(ConfigTags.Npc))
            {
                IsNpcNearby = true;
            }
            else if (collision.gameObject.CompareTag(ConfigTags.MapObjectWarp))
            {
                ObjectWarp objectWarp = collision.gameObject.GetComponent<ObjectWarp>();
                if (objectWarp != null && objectWarp.toMapUid > 0)
                {
                    SceneGame.Instance.mapManager.SetPlaySpawnPosition(objectWarp.toMapPlayerSpawnPosition);
                    SceneGame.Instance.mapManager.LoadMap(objectWarp.toMapUid);
                }
            }
        }
        protected void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(ConfigTags.Monster))
            {
                IsAttacking = false;
            }
            else if (collision.gameObject.CompareTag(ConfigTags.Npc))
            {
                IsNpcNearby = false;
            }
        }
    }
}
