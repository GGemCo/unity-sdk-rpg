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
        protected int BaseCriticalDamage { get; set; }
        protected int BaseCriticalProbability { get; set; }
        
        // 내부 변수 (구독자에게 알리지 않고 값만 저장)
        private long totalAtk;
        private long totalDef;
        private long totalHp;
        private long totalMp;
        private long totalMoveSpeed;
        private long totalAttackSpeed;
        private long totalCriticalDamage;
        private long totalCriticalProbability;
        // 최종 적용된 스탯 (캐싱)
        public readonly BehaviorSubject<long> TotalAtk = new(1);
        public readonly BehaviorSubject<long> TotalDef = new(1);
        public readonly BehaviorSubject<long> TotalHp = new(100);
        public readonly BehaviorSubject<long> TotalMp = new(100);
        public readonly BehaviorSubject<long> TotalMoveSpeed = new(100);
        public readonly BehaviorSubject<long> TotalAttackSpeed = new(100);
        public readonly BehaviorSubject<long> TotalCriticalDamage = new(100);
        public readonly BehaviorSubject<long> TotalCriticalProbability = new(100);
       
        // 현재 활성화된 버프
        // protected readonly List<Buff> ActiveBuffs = new List<Buff>();
        
        // 스탯 캐싱을 위한 Dictionary
        private readonly Dictionary<string, int> plusValues = new Dictionary<string, int>();  // 고정 추가
        private readonly Dictionary<string, int> minusValues = new Dictionary<string, int>(); // 고정 감소
        private readonly Dictionary<string, float> increaseValues = new Dictionary<string, float>(); // % 증가
        private readonly Dictionary<string, float> decreaseValues = new Dictionary<string, float>(); // % 감소

        protected virtual void Awake()
        {
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
        protected void SetBaseInfos(int statAtk, int statDef, int statHp, int statMp, int statMoveSpeed, int statAttackSpeed)
        {
            BaseAtk = statAtk;
            BaseDef = statDef;
            BaseHp = statHp;
            BaseMp = statMp;
            BaseMoveSpeed = statMoveSpeed;
            BaseAttackSpeed = statAttackSpeed;
            RecalculateStats();
        }
        // 값 업데이트
        public void UpdateStatCache(Dictionary<int, StruckTableItem> equippedItems)
        {
            plusValues.Clear();
            minusValues.Clear();
            increaseValues.Clear();
            decreaseValues.Clear();

            // 아이템 효과 적용
            foreach (var item in equippedItems.Select(items => items.Value))
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
        // 스탯을 적절한 Dictionary에 추가하는 함수
        private void ApplyStatEffect(string statType, float value)
        {
            if (string.IsNullOrEmpty(statType)) return;

            if (statType.EndsWith("_PLUS"))
            {
                string baseStat = statType.Replace("_PLUS", "");
                plusValues.TryAdd(baseStat, 0);
                plusValues[baseStat] += (int)value;
            }
            else if (statType.EndsWith("_MINUS"))
            {
                string baseStat = statType.Replace("_MINUS", "");
                minusValues.TryAdd(baseStat, 0);
                minusValues[baseStat] += (int)value;
            }
            else if (statType.EndsWith("_INCREASE"))
            {
                string baseStat = statType.Replace("_INCREASE", "");
                increaseValues.TryAdd(baseStat, 0);
                increaseValues[baseStat] += value;
            }
            else if (statType.EndsWith("_DECREASE"))
            {
                string baseStat = statType.Replace("_DECREASE", "");
                decreaseValues.TryAdd(baseStat, 0);
                decreaseValues[baseStat] += value;
            }
        }
        /// <summary>
        /// 최종 스탯 재계산
        /// </summary>
        private void RecalculateStats()
        {
            // 기본값 + 추가값 적용
            // 내부 값만 변경 (OnNext 호출 X)
            totalAtk = BaseAtk + GetTotalPlusValue("STAT_ATK") - GetTotalMinusValue("STAT_ATK");
            totalDef = BaseDef + GetTotalPlusValue("STAT_DEF") - GetTotalMinusValue("STAT_DEF");
            totalHp = BaseHp + GetTotalPlusValue("STAT_HP") - GetTotalMinusValue("STAT_HP");
            totalMp = BaseMp + GetTotalPlusValue("STAT_MP") - GetTotalMinusValue("STAT_MP");
            totalMoveSpeed = BaseMoveSpeed + GetTotalPlusValue("STAT_MOVE_SPEED") - GetTotalMinusValue("STAT_MOVE_SPEED");
            totalAttackSpeed = BaseAttackSpeed + GetTotalPlusValue("STAT_ATTACK_SPEED") - GetTotalMinusValue("STAT_ATTACK_SPEED");
            totalCriticalDamage = BaseCriticalDamage + GetTotalPlusValue("STAT_CRITIAL_DAMAGE") - GetTotalMinusValue("STAT_CRITIAL_DAMAGE");
            totalCriticalProbability = BaseCriticalProbability + GetTotalPlusValue("STAT_CRITIAL_PROBABILITY") - GetTotalMinusValue("STAT_CRITIAL_PROBABILITY");

            // % 증가 및 감소 적용
            ApplyPercentageModifiers();
        }
        /// <summary>
        /// % 증가 및 감소 적용
        /// </summary>
        private void ApplyPercentageModifiers()
        {
            totalAtk = (long)(totalAtk * (1 + GetTotalIncreaseValue("STAT_ATK") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_ATK") / 100.0f));
            totalDef = (long)(totalDef * (1 + GetTotalIncreaseValue("STAT_DEF") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_DEF") / 100.0f));
            totalHp = (long)(totalHp * (1 + GetTotalIncreaseValue("STAT_HP") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_HP") / 100.0f));
            totalMp = (long)(totalMp * (1 + GetTotalIncreaseValue("STAT_MP") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_MP") / 100.0f));
            totalMoveSpeed = (long)(totalMoveSpeed * (1 + GetTotalIncreaseValue("STAT_MOVE_SPEED") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_MOVE_SPEED") / 100.0f));
            totalAttackSpeed = (long)(totalAttackSpeed * (1 + GetTotalIncreaseValue("STAT_ATTACK_SPEED") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_ATTACK_SPEED") / 100.0f));
            totalCriticalDamage = (long)(totalCriticalDamage * (1 + GetTotalIncreaseValue("STAT_CRITIAL_DAMAGE") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_CRITIAL_DAMAGE") / 100.0f));
            totalCriticalProbability = (long)(totalCriticalProbability * (1 + GetTotalIncreaseValue("STAT_CRITIAL_PROBABILITY") / 100.0f) * (1 - GetTotalDecreaseValue("STAT_CRITIAL_PROBABILITY") / 100.0f));

            ApplyStatChanges();
        }
        /// <summary>
        /// 최종 계산된 스탯을 한 번만 `OnNext`로 전달
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
        /// <summary>
        /// 스탯 조회 함수
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        private int GetTotalPlusValue(string stat) => plusValues.GetValueOrDefault(stat, 0);
        private int GetTotalMinusValue(string stat) => minusValues.GetValueOrDefault(stat, 0);
        private float GetTotalIncreaseValue(string stat) => increaseValues.GetValueOrDefault(stat, 0);
        private float GetTotalDecreaseValue(string stat) => decreaseValues.GetValueOrDefault(stat, 0);
        
        public float GetCurrentMoveSpeed()
        {
            return TotalMoveSpeed.Value / 100f;
        }
        public float GetCurrentAttackSpeed()
        {
            return TotalAttackSpeed.Value / 100f;
        }
    }
}