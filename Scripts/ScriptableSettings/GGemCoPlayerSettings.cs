using UnityEngine;

namespace GGemCo.Scripts.ScriptableSettings
{
    [CreateAssetMenu(fileName = "GGemCoPlayerSettings", menuName = "GGemCo/Settings/GGemCoPlayerSettings", order = 2)]
    public class GGemCoPlayerSettings :ScriptableObject
    {
        [Header("플레이어 디폴트 값 설정")] 
        [Header("플레이어 최대 레벨")] public int maxLevel;
        [Header("기본 공격력")] public long statAtk;
        [Header("기본 방어력")] public long statDef;
        [Header("기본 생명력")] public long statHp;
        [Header("기본 마력")] public long statMp;
        [Header("애니메이션 스텝 당 이동거리")] public float statMoveStep;
        [Header("이동 속도")] public float statMoveSpeed;
        
        /// <summary>
        /// 기존 값이 비어있을 때만 기본값을 설정
        /// </summary>
        private void OnEnable()
        {
        }
        /// <summary>
        /// 처음 생성 시 한 번만 실행됨
        /// </summary>
        private void Reset()
        {
            statAtk = 100;
            statDef = 100;
            statHp = 100;
            statMp = 100;
            statMoveStep = 100;
            statMoveSpeed = 1;
        }
    }
}