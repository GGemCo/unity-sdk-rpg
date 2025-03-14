using System.Linq;
using GGemCo.Scripts.ScriptableSettings;
using UnityEngine;

namespace GGemCo.Scripts.Characters.Player
{
    public class PlayerStat : DefaultCharacterStat
    {
        private Player player;
        public override void Initialize(GameObject pcharacter)
        {
            if (pcharacter == null) return;
            base.Initialize(pcharacter);
            player = pcharacter.GetComponent<Player>();
        }
        /// <summary>
        /// GGemCoPlayerSettings 스크립터블 오브젝트에 설정된 base 값 셋팅 
        /// </summary>
        /// <param name="playerSettings"></param>
        public void SetBaseInfos(GGemCoPlayerSettings playerSettings)
        {
            BaseAtk = playerSettings.statAtk;
            BaseDef = playerSettings.statDef;
            BaseHp = playerSettings.statHp;
            BaseMp = playerSettings.statMp;
            BaseMoveSpeed = playerSettings.statMoveSpeed;
            RecalculateStats();
        }
        // 값 업데이트
        public override void UpdateStatCache()
        {
            PlusValues.Clear();
            MinusValues.Clear();
            IncreaseValues.Clear();
            DecreaseValues.Clear();

            // 아이템 효과 적용
            foreach (var item in player.GetEquippedItems().Select(items => items.Value))
            {
                if (item == null) continue;
                ApplyStatEffect(item.StatusID1, item.StatusValue1);
                ApplyStatEffect(item.OptionType1, item.OptionValue1);
            }

            // 버프 효과 적용
            // foreach (var buff in ActiveBuffs)
            // {
            //     ApplyStatEffect(buff.OptionType, buff.OptionValue);
            // }

            RecalculateStats();
        }
        
    }
}