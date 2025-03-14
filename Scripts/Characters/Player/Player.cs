using System.Collections.Generic;
using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Maps.Objects;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.ScriptableSettings;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
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
        // 주변에 npc 가 있는지 체크 
        private bool isNpcNearby;
        private GGemCoSettings gGemCoSettings; 
        private EquipController equipController;
        public PlayerStat PlayerStat;
        protected override void Awake()
        {
            base.Awake();
            isNpcNearby = false;
#if GGEMCO_USE_SPINE
            DefaultCharacterBehavior = gameObject.AddComponent<BehaviorPlayerSpine>();
#else
            DefaultCharacterBehavior = gameObject.AddComponent<BehaviorPlayerSprite>();
#endif
            equipController = new EquipController();
            equipController.Initialize(this);
        }
        /// <summary>
        /// tag, sorting layer, layer 셋팅하기
        /// </summary>
        public override void InitTagSortingLayer()
        {
            base.InitTagSortingLayer();
            tag = ConfigTags.GetValue(ConfigTags.Keys.Player);
        }
        /// <summary>
        /// 캐릭터에 필요한 컴포넌트 추가하기
        /// </summary>
        protected override void InitComponents()
        {
            base.InitComponents();
            ComponentController.AddRigidbody2D(gameObject);
            Vector2 offset = Vector2.zero;
            Vector2 size = new Vector2(500, 250);
            ComponentController.AddCapsuleCollider2D(gameObject, true, offset, size);
            offset = Vector2.zero;
            size = new Vector2(264,132);
            ComponentController.AddCapsuleCollider2D(gameObject, false, offset, size,
                LayerMask.GetMask(ConfigLayer.GetValue(ConfigLayer.Keys.TileMapWall)),
                ~ (1 << LayerMask.NameToLayer(ConfigLayer.GetValue(ConfigLayer.Keys.TileMapWall))));
        }
        /// <summary>
        /// GGemCoPlayerSettings 에서 가져온 정보 셋팅
        /// </summary>
        protected override void InitializeByTable()
        {
            if (AddressableSettingsLoader.Instance == null) return;
            {
                GGemCoPlayerSettings playerSettings = AddressableSettingsLoader.Instance.playerSettings;
                PlayerStat.SetBaseInfos(playerSettings);
                CurrentHp = PlayerStat.TotalHp;
                CurrentMoveStep = playerSettings.statMoveStep;
                OriginalScaleX = transform.localScale.x;
                SetScale(playerSettings.startScale);
            }
        }
        /// <summary>
        /// 공격 버튼 눌렀을때 처리 
        /// </summary>
        private bool Attack()
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
                // GcLogger.Log("monster dead");
                return false;
            }
                
            CurrentHp -= damage;
            // -1 이면 죽지 않는다
            if (PlayerStat.BaseAtk < 0)
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
            Collider2D[] collider2Ds = Physics2D.OverlapCapsuleAll(point, size, capsuleCollider.direction, 0f);
            foreach (var hit in collider2Ds)
            {
                if (hit.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Monster)))
                {
                    Monster.Monster monster = hit.GetComponent<Monster.Monster>();
                    if (monster != null)
                    {
                        // GcLogger.Log("Player attacked the monster after animation!");
                        targetMonster = monster.gameObject;
                        return true;
                    }
                }
            }

            return false;
        }
        protected void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Monster)))
            {
                IsAttacking = true;
                Monster.Monster monster = collision.gameObject.GetComponent<Monster.Monster>();
                if (monster.gameObject.GetComponent<Monster.Monster>().IsStatusDead())
                {
                    // GcLogger.Log("player / update / monster dead");
                    targetMonster = null;
                }
                else {
                    targetMonster = monster.gameObject;
                    Attack();
                }
            }
            else if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Npc)))
            {
                isNpcNearby = true;
            }
            else if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.MapObjectWarp)))
            {
                ObjectWarp objectWarp = collision.gameObject.GetComponent<ObjectWarp>();
                SceneGame.Instance.mapManager.LoadMapByWarp(objectWarp);
            }
            else if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.DropItem)))
            {
                SceneGame.Instance.itemManager.PlayerTaken(collision.gameObject);
            }
        }
        protected void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Monster)))
            {
                IsAttacking = false;
            }
            else if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Npc)))
            {
                isNpcNearby = false;
            }
        }
        public void EquipItem(int partIndex, int itemUid)
        {
            bool result = equipController.EquipItem(partIndex, itemUid);
            if (!result) return;
            DefaultCharacterBehavior.ChangeImageByEquiped(partIndex, itemUid);
        }
        public void UnEquipItem(int partIndex)
        {
            bool result = equipController.UnEquipItem(partIndex);
        }
        protected override void InitCharacterStat()
        {
            PlayerStat = new PlayerStat();
            PlayerStat.Initialize(gameObject);
        }
        public override Dictionary<int, StruckTableItem> GetEquippedItems()
        {
            return equipController.GetEquippedItems();
        }
        public override float GetCurrentMoveSpeed()
        {
            return PlayerStat.TotalMoveSpeed / 100f;
        }
        public float GetCurrentMoveStep()
        {
            return CurrentMoveStep;
        }
    }
}
