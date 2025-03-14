using GGemCo.Scripts.TableLoader;
using UnityEngine;

namespace GGemCo.Scripts.Characters.Monster
{
    public class MonsterStat : DefaultCharacterStat
    {
        private Monster monster;
        public override void Initialize(GameObject pcharacter)
        {
            if (pcharacter == null) return;
            base.Initialize(pcharacter);
            monster = pcharacter.GetComponent<Monster>();
        }
        /// <summary>
        /// GGemCoPlayerSettings 스크립터블 오브젝트에 설정된 base 값 셋팅 
        /// </summary>
        /// <param name="struckTableMonster"></param>
        public void SetBaseInfos(StruckTableMonster struckTableMonster)
        {
            BaseAtk = struckTableMonster.StatAtk;
            BaseDef = struckTableMonster.StatDef;
            BaseHp = struckTableMonster.StatHp;
            BaseMp = 0;
            BaseMoveSpeed = struckTableMonster.StatMoveSpeed;
            BaseAttackSpeed = struckTableMonster.StatAttackSpeed;
            RecalculateStats();
        }

        public float GetTotalAttackSpeed()
        {
            return TotalAttackSpeed / 100f;
        }
        
    }
}