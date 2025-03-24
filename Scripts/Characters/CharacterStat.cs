using System.Collections.Generic;
using System.Linq;
using GGemCo.Scripts.TableLoader;
using R3;
using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    /// <summary>
    /// 캐릭터 스탯 관리
    /// </summary>
    public class CharacterStat : MonoBehaviour
    {
        // 기본 스탯
        private int BaseAtk { get; set; }
        private int BaseDef { get; set; }
        protected int BaseHp { get; set; }
        private int BaseMp { get; set; }
        private int BaseMoveSpeed { get; set; }
        private int BaseAttackSpeed { get; set; }
        private int BaseCriticalDamage { get; set; }
        private int BaseCriticalProbability { get; set; }

        private readonly Dictionary<string, int> flatModifiers = new();
        private readonly Dictionary<string, float> percentModifiers = new();

        private long totalAtk, totalDef, totalHp, totalMp, totalMoveSpeed, totalAttackSpeed, totalCriticalDamage, totalCriticalProbability;
        // 최종 적용된 스탯 (캐싱)
        public readonly BehaviorSubject<long> TotalAtk = new(1);
        public readonly BehaviorSubject<long> TotalDef = new(1);
        public readonly BehaviorSubject<long> TotalHp = new(100);
        public readonly BehaviorSubject<long> TotalMp = new(100);
        public readonly BehaviorSubject<long> TotalMoveSpeed = new(100);
        public readonly BehaviorSubject<long> TotalAttackSpeed = new(100);
        public readonly BehaviorSubject<long> TotalCriticalDamage = new(100);
        public readonly BehaviorSubject<long> TotalCriticalProbability = new(100);

        private CharacterBuffManager buffManager;

        protected virtual void Awake()
        {
            buffManager = new CharacterBuffManager(this);
        }
        protected virtual void Start()
        {
        }

        /// <summary>
        /// 스크립터블 오브젝트에 설정된 base 값 셋팅 
        /// </summary>
        /// <param name="statAtk"></param>
        /// <param name="statDef"></param>
        /// <param name="statHp"></param>
        /// <param name="statMp"></param>
        /// <param name="statMoveSpeed"></param>
        /// <param name="statAttackSpeed"></param>
        protected void SetBaseInfos(int statAtk, int statDef, int statHp, int statMp, int statMoveSpeed,
            int statAttackSpeed)
        {
            BaseAtk = statAtk;
            BaseDef = statDef;
            BaseHp = statHp;
            BaseMp = statMp;
            BaseMoveSpeed = statMoveSpeed;
            BaseAttackSpeed = statAttackSpeed;
            RecalculateStats();
        }

        /// <summary>
        /// 값 업데이트
        /// </summary>
        /// <param name="equippedItems"></param>
        public void UpdateStatCache(Dictionary<int, StruckTableItem> equippedItems)
        {
            flatModifiers.Clear();
            percentModifiers.Clear();

            Dictionary<string, float> modifiers = new Dictionary<string, float>();
            // 아이템 효과 적용
            foreach (var item in equippedItems.Select(items => items.Value))
            {
                if (item == null) continue;
                modifiers.TryAdd(item.StatusID1, item.StatusValue1);
                modifiers.TryAdd(item.StatusID2, item.StatusValue2);
                modifiers.TryAdd(item.OptionType1, item.OptionValue1);
                modifiers.TryAdd(item.OptionType2, item.OptionValue2);
                modifiers.TryAdd(item.OptionType3, item.OptionValue3);
                modifiers.TryAdd(item.OptionType4, item.OptionValue4);
                modifiers.TryAdd(item.OptionType5, item.OptionValue5);
            }

            ApplyStatModifiers(modifiers);
            RecalculateStats();
        }
        /// <summary>
        /// 버프 적용하기
        /// </summary>
        /// <param name="buff"></param>
        protected void ApplyBuff(StruckBuff buff) => buffManager.ApplyBuff(buff);
        /// <summary>
        /// 스탯 변경값 적용하기
        /// </summary>
        /// <param name="modifiers"></param>
        public void ApplyStatModifiers(Dictionary<string, float> modifiers)
        {
            foreach (var kvp in modifiers)
            {
                ModifyStat(kvp.Key, kvp.Value, true);
            }
        }

        public void RemoveStatModifiers(Dictionary<string, float> modifiers)
        {
            foreach (var kvp in modifiers)
            {
                ModifyStat(kvp.Key, kvp.Value, false);
            }
        }
        /// <summary>
        /// 접미사에 따라 적용할 값 배열에 넣기
        /// </summary>
        /// <param name="statType"></param>
        /// <param name="value"></param>
        /// <param name="isAdding"></param>
        private void ModifyStat(string statType, float value, bool isAdding)
        {
            if (string.IsNullOrEmpty(statType)) return;

            string baseStat = statType.Replace("_PLUS", "").Replace("_MINUS", "").Replace("_INCREASE", "").Replace("_DECREASE", "");

            if (statType.EndsWith("_PLUS"))
            {
                flatModifiers[baseStat] = flatModifiers.GetValueOrDefault(baseStat, 0) + (isAdding ? (int)value : -(int)value);
                if (flatModifiers[baseStat] == 0) flatModifiers.Remove(baseStat);
            }
            else if (statType.EndsWith("_MINUS"))
            {
                flatModifiers[baseStat] = flatModifiers.GetValueOrDefault(baseStat, 0) - (isAdding ? (int)value : -(int)value);
                if (flatModifiers[baseStat] == 0) flatModifiers.Remove(baseStat);
            }
            else if (statType.EndsWith("_INCREASE"))
            {
                percentModifiers[baseStat] = percentModifiers.GetValueOrDefault(baseStat, 0) + (isAdding ? value : -value);
                if (Mathf.Approximately(percentModifiers[baseStat], 0)) percentModifiers.Remove(baseStat);
            }
            else if (statType.EndsWith("_DECREASE"))
            {
                percentModifiers[baseStat] = percentModifiers.GetValueOrDefault(baseStat, 0) - (isAdding ? value : -value);
                if (Mathf.Approximately(percentModifiers[baseStat], 0)) percentModifiers.Remove(baseStat);
            }
        }
        /// <summary>
        /// 스탯별 최종 계산하기
        /// </summary>
        /// <param name="statKey"></param>
        /// <param name="baseValue"></param>
        /// <returns></returns>
        private long CalculateFinalStat(string statKey, int baseValue)
        {
            int flatBonus = flatModifiers.GetValueOrDefault(statKey, 0);
            float percentBonus = percentModifiers.GetValueOrDefault(statKey, 0);

            float finalMultiplier = 1 + (percentBonus / 100f);
            if (finalMultiplier < 0) finalMultiplier = 0; // 최소 0으로 제한

            return (long)((baseValue + flatBonus) * finalMultiplier);
        }
        /// <summary>
        /// 최종 계산하기
        /// </summary>
        public void RecalculateStats()
        {
            totalAtk = CalculateFinalStat("STAT_ATK", BaseAtk);
            totalDef = CalculateFinalStat("STAT_DEF", BaseDef);
            totalHp = CalculateFinalStat("STAT_HP", BaseHp);
            totalMp = CalculateFinalStat("STAT_MP", BaseMp);
            totalMoveSpeed = CalculateFinalStat("STAT_MOVE_SPEED", BaseMoveSpeed);
            totalAttackSpeed = CalculateFinalStat("STAT_ATTACK_SPEED", BaseAttackSpeed);
            totalCriticalDamage = CalculateFinalStat("STAT_CRITIAL_DAMAGE", BaseCriticalDamage);
            totalCriticalProbability = CalculateFinalStat("STAT_CRITIAL_PROBABILITY", BaseCriticalProbability);

            ApplyStatChanges();
        }
        /// <summary>
        /// 변경한 스탯 r3 에 적용하기
        /// </summary>
        private void ApplyStatChanges()
        {
            TotalAtk.OnNext(totalAtk);
            TotalDef.OnNext(totalDef);
            TotalHp.OnNext(totalHp);
            TotalMp.OnNext(totalMp);
            TotalMoveSpeed.OnNext(totalMoveSpeed);
            TotalAttackSpeed.OnNext(totalAttackSpeed);
            TotalCriticalDamage.OnNext(totalCriticalDamage);
            TotalCriticalProbability.OnNext(totalCriticalProbability);
        }

        public float GetCurrentMoveSpeed(bool isPercent = true) => isPercent ? TotalMoveSpeed.Value / 100f : TotalMoveSpeed.Value;
        public float GetCurrentAttackSpeed() => TotalAttackSpeed.Value / 100f;
    }
}
