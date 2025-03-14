using System.Collections.Generic;
using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    // public class Buff
    // {
    //     public string Name;
    //     public string OptionType;
    //     public int OptionValue;
    // }
    public class DefaultCharacterStat
    {
        private GameObject character;
        // 기본 스탯
        public int BaseAtk { get; protected set; }
        protected int BaseDef { get; set; }
        public int BaseHp { get; protected set; }
        protected int BaseMp { get; set; }
        protected int BaseMoveSpeed { get; set; }
        
        protected int BaseAttackSpeed { get; set; }
        protected int BaseCriticalDamage { get; set; }
        protected int BaseCriticalProbability { get; set; }

        // 최종 적용된 스탯 (캐싱)
        public long TotalAtk { get; private set; }
        public long TotalDef { get; private set; }
        public long TotalHp { get; private set; }
        public long TotalMp { get; private set; }
        public long TotalMoveSpeed { get; private set; }
        public long TotalAttackSpeed { get; private set; }
        private long TotalCriticalDamage { get; set; }
        private long TotalCriticalProbability { get; set; }

        // 현재 활성화된 버프
        // protected readonly List<Buff> ActiveBuffs = new List<Buff>();
        
        // 스탯 캐싱을 위한 Dictionary
        protected readonly Dictionary<string, int> PlusValues = new Dictionary<string, int>();  // 고정 추가
        protected readonly Dictionary<string, int> MinusValues = new Dictionary<string, int>(); // 고정 감소
        protected readonly Dictionary<string, float> IncreaseValues = new Dictionary<string, float>(); // % 증가
        protected readonly Dictionary<string, float> DecreaseValues = new Dictionary<string, float>(); // % 감소

        public virtual void Initialize(GameObject pcharacter)
        {
            if (pcharacter == null) return;
            character = pcharacter;
        }
        // // 버프 적용
        // private void ApplyBuff(Buff buff, float duration)
        // {
        //     ActiveBuffs.Add(buff);
        //     RecalculateStats();
        //     // StartCoroutine(RemoveBuffAfterDuration(buff, duration));
        // }
        //
        // // 버프 자동 제거
        // private IEnumerator RemoveBuffAfterDuration(Buff buff, float duration)
        // {
        //     yield return new WaitForSeconds(duration);
        //     ActiveBuffs.Remove(buff);
        //     RecalculateStats();
        // }
        // 값 업데이트
        public virtual void UpdateStatCache()
        {
        }
        // 스탯을 적절한 Dictionary에 추가하는 함수
        protected void ApplyStatEffect(string statType, float value)
        {
            if (string.IsNullOrEmpty(statType)) return;

            if (statType.EndsWith("_PLUS"))
            {
                string baseStat = statType.Replace("_PLUS", "");
                PlusValues.TryAdd(baseStat, 0);
                PlusValues[baseStat] += (int)value;
            }
            else if (statType.EndsWith("_MINUS"))
            {
                string baseStat = statType.Replace("_MINUS", "");
                MinusValues.TryAdd(baseStat, 0);
                MinusValues[baseStat] += (int)value;
            }
            else if (statType.EndsWith("_INCREASE"))
            {
                string baseStat = statType.Replace("_INCREASE", "");
                IncreaseValues.TryAdd(baseStat, 0);
                IncreaseValues[baseStat] += value;
            }
            else if (statType.EndsWith("_DECREASE"))
            {
                string baseStat = statType.Replace("_DECREASE", "");
                DecreaseValues.TryAdd(baseStat, 0);
                DecreaseValues[baseStat] += value;
            }
        }

        // 최종 스탯 재계산
        protected void RecalculateStats()
        {
            if (character == null) return;
            // 기본값 + 추가값 적용
            TotalAtk = BaseAtk + GetTotalPlusValue("STAT_ATK") - GetTotalMinusValue("STAT_ATK");
            TotalDef = BaseDef + GetTotalPlusValue("STAT_DEF") - GetTotalMinusValue("STAT_DEF");
            TotalHp = BaseHp + GetTotalPlusValue("STAT_HP") - GetTotalMinusValue("STAT_HP");
            TotalMp = BaseMp + GetTotalPlusValue("STAT_MP") - GetTotalMinusValue("STAT_MP");
            TotalMoveSpeed = BaseMoveSpeed + GetTotalPlusValue("STAT_MOVE_SPEED") - GetTotalMinusValue("STAT_MOVE_SPEED");
            TotalAttackSpeed = BaseAttackSpeed + GetTotalPlusValue("STAT_ATTACK_SPEED") - GetTotalMinusValue("STAT_ATTACK_SPEED");
            TotalCriticalDamage = BaseCriticalDamage + GetTotalPlusValue("STAT_CRITIAL_DAMAGE") - GetTotalMinusValue("STAT_CRITIAL_DAMAGE");
            TotalCriticalProbability = BaseCriticalProbability + GetTotalPlusValue("STAT_CRITIAL_PROBABILITY") - GetTotalMinusValue("STAT_CRITIAL_PROBABILITY");

            // % 증가 및 감소 적용
            ApplyPercentageModifiers();
        }
        // % 증가 및 감소 적용
        private void ApplyPercentageModifiers()
        {
            TotalAtk = (int)(TotalAtk * (1 + GetTotalIncreaseValue("STAT_ATK") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_ATK") / 100.0f));
            TotalDef = (int)(TotalDef * (1 + GetTotalIncreaseValue("STAT_DEF") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_DEF") / 100.0f));
            TotalHp = (int)(TotalHp * (1 + GetTotalIncreaseValue("STAT_HP") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_HP") / 100.0f));
            TotalMp = (int)(TotalMp * (1 + GetTotalIncreaseValue("STAT_MP") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_MP") / 100.0f));
            TotalMoveSpeed *= (long)((1 + GetTotalIncreaseValue("STAT_MOVE_SPEED") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_MOVE_SPEED") / 100.0f));
            TotalAttackSpeed *= (long)((1 + GetTotalIncreaseValue("STAT_ATTACK_SPEED") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_ATTACK_SPEED") / 100.0f));
            TotalCriticalDamage *= (long)((1 + GetTotalIncreaseValue("STAT_CRITIAL_DAMAGE") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_CRITIAL_DAMAGE") / 100.0f));
            TotalCriticalProbability *= (long)((1 + GetTotalIncreaseValue("STAT_CRITIAL_PROBABILITY") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_CRITIAL_PROBABILITY") / 100.0f));
        }
        
        // 스탯 조회 함수
        private int GetTotalPlusValue(string stat) => PlusValues.GetValueOrDefault(stat, 0);
        private int GetTotalMinusValue(string stat) => MinusValues.GetValueOrDefault(stat, 0);
        private float GetTotalIncreaseValue(string stat) => IncreaseValues.GetValueOrDefault(stat, 0);
        private float GetTotalDecreaseValue(string stat) => DecreaseValues.GetValueOrDefault(stat, 0);
    }
}
