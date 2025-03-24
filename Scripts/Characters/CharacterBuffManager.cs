using System.Collections;
using System.Collections.Generic;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    public class StruckBuff
    {
        public int Uid;
        public string Name;
        public float Duration;
        public Dictionary<string, float> Buffs;

        public StruckBuff(int uid, string name, float duration, Dictionary<string, float> buffs)
        {
            Uid = uid;
            Name = name;
            Duration = duration;
            Buffs = buffs;
        }
    }

    public class CharacterBuffManager
    {
        private readonly CharacterStat characterStat;
        private readonly List<StruckBuff> activeBuffs = new();

        public CharacterBuffManager(CharacterStat stat)
        {
            characterStat = stat;
        }

        public void ApplyBuff(StruckBuff buff)
        {
            // GcLogger.Log($"ApplyBuff {buff.Uid}/{buff.Name}/{buff.Duration}");
            activeBuffs.Add(buff);
            characterStat.ApplyStatModifiers(buff.Buffs);
            characterStat.RecalculateStats();
            characterStat.StartCoroutine(RemoveBuffAfterDuration(buff));
        }

        private IEnumerator RemoveBuffAfterDuration(StruckBuff buff)
        {
            yield return new WaitForSeconds(buff.Duration);
            // GcLogger.Log($"RemoveBuffAfterDuration {buff.Uid}/{buff.Name}/{buff.Duration}");
            activeBuffs.Remove(buff);
            characterStat.RemoveStatModifiers(buff.Buffs);
            characterStat.RecalculateStats();
        }
    }
}