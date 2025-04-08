using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.Characters.Npc
{
    /// <summary>
    /// Npc 기본 클레스
    /// </summary>
    public class Npc : CharacterBase
    {
        public NpcData NpcData;
        
        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            NpcData = null;
        }

        protected override void Start()
        {
            base.Start();
            CharacterAnimationController.SetCharacterColor(ColorHelper.HexToColor("#DAAADB"));
        }

        /// <summary>
        /// tag, sorting layer, layer 셋팅하기
        /// </summary>
        public override void InitTagSortingLayer()
        {
            base.InitTagSortingLayer();
            tag = ConfigTags.GetValue(ConfigTags.Keys.Npc);
        }
        /// <summary>
        /// 캐릭터에 필요한 컴포넌트 추가하기
        /// </summary>
        protected override void InitComponents()
        {
            base.InitComponents();
            ComponentController.AddRigidbody2D(gameObject);
            
            // attack range
            GameObject attackRange = new GameObject("AttackRange");
            CharacterAttackRange characterAttackRange = attackRange.AddComponent<CharacterAttackRange>();
            characterAttackRange.Initialize(this);
            
            Vector2 offset = Vector2.zero;
            Vector2 size = new Vector2(0,0);
            colliderCheckCharacter = ComponentController.AddCapsuleCollider2D(attackRange, true, offset, size);
            
            gameObject.AddComponent<ControllerNpc>();
            
        }
        /// <summary>
        /// 테이블에서 가져온 npc 정보 셋팅
        /// </summary>
        protected override void InitializeByTable()
        {
            base.InitializeByTable();
            if (TableLoaderManager.Instance == null) return;
            if (uid <= 0) return;
            TableLoaderManager tableLoaderManager = TableLoaderManager.Instance;
            var info = tableLoaderManager.TableNpc.GetDataByUid(uid);
            // GcLogger.Log("InitializationStat uid: "+uid+" / info.uid: "+info.uid+" / StatMoveSpeed: "+info.statMoveSpeed);
            if (info.Uid > 0)
            {
                const int statAtk = 0;
                const int statDef = 0;
                const int statHp = 0;
                const int statMp = 0;
                const int statMoveSpeed = 100;
                const int statAttackSpeed = 0;
                const int statRegistFire = 0;
                const int statRegistCold = 0;
                const int statRegistLightning = 0;
                SetBaseInfos(statAtk, statDef, statHp, statMp, statMoveSpeed, statAttackSpeed, statRegistFire,
                    statRegistCold, statRegistLightning);
                float scale = info.Scale;
                SetScale(scale);
                
                StruckTableAnimation struckTableAnimation = tableLoaderManager.TableAnimation.GetDataByUid(info.SpineUid);
                if (struckTableAnimation is { Uid: > 0 })
                {
                    currentMoveStep = struckTableAnimation.MoveStep;
                    if (colliderCheckCharacter != null)
                    {
                        colliderCheckCharacter.size = new Vector2(struckTableAnimation.AttackRange, struckTableAnimation.AttackRange/2f);
                    }
                }
            }
        }
        /// <summary>
        /// regen_data 의 정보 셋팅
        /// </summary>
        protected override void InitializeByRegenData()
        {
            // 맵 배치툴로 저장한 정보가 있을 경우 
            if (NpcData == null) return;
            // UpdateDirection() 에서 초기 방향 처리를 위해 추가
            direction = new Vector3(NpcData.Flip?1:-1, 0, 0);
            directionPrev = new Vector3(NpcData.Flip?1:-1, 0, 0);
            SetFlip(NpcData.Flip);
        }
        
        protected void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Player)))
            {
                SceneGame.Instance.InteractionManager.SetInfo(this);
            }
        }

        protected void OnTriggerExit2D(Collider2D collision)
        {
            if (!Application.isPlaying || !this.enabled || !gameObject.activeInHierarchy) return;
            
            if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Player)))
            {
                SceneGame.Instance.InteractionManager.EndInteraction();
            }
        }
    }
}
