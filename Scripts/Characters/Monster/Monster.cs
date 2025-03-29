using System;
using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI;
using GGemCo.Scripts.Utils;
using UnityEngine;
using R3;

namespace GGemCo.Scripts.Characters.Monster
{
    /// <summary>
    /// 몬스터 기본 클레스
    /// </summary>
    public class Monster : CharacterBase
    {
        // 몬스터 데이터
        [HideInInspector] public MonsterData MonsterData;
        // 선공/후공
        private AttackType attackType;
        public delegate void DelegateMonsterDead(int monsterVid, int monsterUid, GameObject monsterObject);
        public event DelegateMonsterDead OnMonsterDead;
        
        // 몬스터 행동 처리
        private ControllerMonster controllerMonster;
        // 생명력 slier
        [HideInInspector] public GameObject sliderHpBar;
        private GameObject prefabSliderHpBar;
        private Transform containerMonsterHpBar;
        
        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            MonsterData = null;
            attackType = AttackType.PassiveDefense;
            
            OnMonsterDead += SceneGame.Instance.ItemManager.OnMonsterDead;
            OnMonsterDead += SceneGame.Instance.saveDataManager.Player.AddExp;
            OnMonsterDead += SceneGame.Instance.mapManager.OnDeadMonster;
            
            CurrentHp
                .Subscribe(SetSliderHp)
                .AddTo(this);
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
            // AddComponent 순서 중요
            base.InitComponents();
            Vector2 offset = Vector2.zero;
            Vector2 size = new Vector2(0,0);
            ComponentController.AddCapsuleCollider2D(gameObject,false, offset, size);
            controllerMonster = gameObject.AddComponent<ControllerMonster>();
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
            if (uid <= 0) return;
            TableLoaderManager tableLoaderManager = TableLoaderManager.Instance;
            var info = tableLoaderManager.TableMonster.GetDataByUid(uid);
            // GcLogger.Log("InitializationStat uid: "+uid+" / info.uid: "+info.uid+" / StatMoveSpeed: "+info.statMoveSpeed);
            if (info.Uid <= 0) return;
            characterName = info.Name;
            SetBaseInfos(info.StatAtk, info.StatDef, info.StatHp, 0, info.StatMoveSpeed, info.StatAttackSpeed);
            CurrentHp.OnNext(info.StatHp);
            float scale = info.Scale;
            SetScale(scale);

            StruckTableAnimation struckTableAnimation = tableLoaderManager.TableAnimation.GetDataByUid(info.SpineUid);
            if (struckTableAnimation is { Uid: > 0 })
            {
                currentMoveStep = struckTableAnimation.MoveStep;
                CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
                if (capsule != null)
                {
                    capsule.size = struckTableAnimation.ColliderSize;
                }
                height = struckTableAnimation.Height;
            }

        }
        public void CreateHpBar()
        {
            if (SceneGame.Instance.containerMonsterHpBar == null)
            {
                GcLogger.LogError("SceneGame 에 containerMonsterHpBar 가 설정되지 않았습니다.");
                return;
            }
            prefabSliderHpBar = AddressablePrefabLoader.Instance.GetPreLoadGamePrefabByName(ConfigAddressables.KeyPrefabSliderMonsterHp);
            if (prefabSliderHpBar == null)
            {
                GcLogger.LogError($"Addressable group 에 {ConfigAddressables.KeyPrefabSliderMonsterHp} 가 없습니다.");
                return;
            }
            containerMonsterHpBar = SceneGame.Instance.containerMonsterHpBar.transform;
            sliderHpBar = Instantiate(prefabSliderHpBar, containerMonsterHpBar);
            MonsterHpBar monsterHpBar = sliderHpBar.GetComponent<MonsterHpBar>();
            monsterHpBar.Initialize(this);
        }

        /// <summary>
        /// 데미지 받으면 어그로 on. 공격자 등록하기
        /// </summary>
        /// <param name="attacker"></param>
        protected override void OnDamage(GameObject attacker)
        {
            if (IsAggro() == false)
            {
                SetAggro(true);
            }
            SetAttackerTarget(attacker.transform);
        }
        /// <summary>
        /// 몬스터가 죽었을때 처리 
        /// </summary>
        protected override void OnDead()
        {
            base.OnDead();
            if (sliderHpBar != null)
            {
                Destroy(sliderHpBar);
            }
            controllerMonster.StopAttackCoroutine();
            OnMonsterDead?.Invoke(vid, uid, gameObject);
        }
        protected void OnDestroy()
        {
            OnMonsterDead -= SceneGame.Instance.ItemManager.OnMonsterDead;
            OnMonsterDead -= SceneGame.Instance.saveDataManager.Player.AddExp;
            OnMonsterDead -= SceneGame.Instance.mapManager.OnDeadMonster;
            if (sliderHpBar != null)
            {
                Destroy(sliderHpBar);
            }
        }
        /// <summary>
        /// attack 이벤트 처리 
        /// </summary>
        public override void OnEventAttack()
        {
            if (IsStatusDead()) return;
            // GcLogger.Log(@event);
            long totalDamage = SceneGame.Instance.calculateManager.GetMonsterTotalAtk(uid);
        
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

        public float GetHeight()
        {
            return height * Math.Abs(transform.localScale.x);
        }

        private void SetSliderHp(long value)
        {
            if (sliderHpBar == null) return;
            sliderHpBar.GetComponent<MonsterHpBar>().SetValue(value);
        }
        protected override void OnStartFadeIn()
        {
            if (sliderHpBar == null) return;
            sliderHpBar.GetComponent<MonsterHpBar>().StartFadeIn();
        }
        protected override void OnStartFadeOut()
        {
            if (sliderHpBar == null) return;
            sliderHpBar.GetComponent<MonsterHpBar>().StartFadeOut();
        }
    }
}
