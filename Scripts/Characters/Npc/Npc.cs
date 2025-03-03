using GGemCo.Scripts.Configs;
using GGemCo.Scripts.TableLoader;
using UnityEngine;

namespace GGemCo.Scripts.Characters.Npc
{
    /// <summary>
    /// Npc 기본 클레스
    /// </summary>
    public class Npc : DefaultCharacter, INpc
    {
        public NpcData NpcData;
        
        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            NpcData = null;
            
#if GGEMCO_USE_SPINE
            DefaultCharacterBehavior = gameObject.AddComponent<BehaviorNpcSpine>();
#else
            DefaultCharacterBehavior = gameObject.AddComponent<BehaviorNpcSprite>();
#endif
        }
        /// <summary>
        /// tag, sorting layer, layer 셋팅하기
        /// </summary>
        public override void InitTagSortingLayer()
        {
            base.InitTagSortingLayer();
            tag = ConfigTags.GetNpc();
        }
        /// <summary>
        /// 캐릭터에 필요한 컴포넌트 추가하기
        /// </summary>
        protected override void InitComponents()
        {
        }
        /// <summary>
        /// 테이블에서 가져온 npc 정보 셋팅
        /// </summary>
        protected override void InitializeByTable()
        {
            base.InitializeByTable();
            if (TableLoaderManager.Instance == null) return;
            if (Uid <= 0) return;
            TableLoaderManager tableLoaderManager = TableLoaderManager.Instance;
            var info = tableLoaderManager.TableNpc.GetDataByUid(Uid);
            // FG_Logger.Log("InitializationStat uid: "+uid+" / info.uid: "+info.uid+" / StatMoveSpeed: "+info.statMoveSpeed);
            if (info.Uid > 0)
            {
                CurrentAtk = (long)StatAtk;
                StatMoveSpeed = info.StatMoveSpeed;
                CurrentMoveSpeed = StatMoveSpeed;
                CurrentMoveStep = StatMoveStep;
                CurrentHp = (long)StatHp;
                float scale = info.Scale;
                SetScale(scale);
                
                StruckTableAnimation struckTableAnimation = tableLoaderManager.TableAnimation.GetDataByUid(info.SpineUid);
                if (struckTableAnimation is { Uid: > 0 })
                {
                    CurrentMoveStep = struckTableAnimation.MoveStep;
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
            SetFlip(NpcData.Flip);
        }
        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(ConfigTags.GetPlayer()))
            {
            }
        }
        void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(ConfigTags.GetPlayer()))
            {
            }
        }
    }
}
