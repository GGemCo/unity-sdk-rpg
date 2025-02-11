using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    /// <summary>
    /// 캐릭터 인터페이스
    /// </summary>
    public interface ICharacter
    {
        /// <summary>
        /// 캐릭터 상태
        /// </summary>
        enum CharacterStatus
        {
            None,
            /// <summary>
            /// 기본 상태
            /// </summary>
            Idle,
            /// <summary>
            /// 움직이는 중
            /// </summary>
            Run,
            /// <summary>
            /// 공격 중
            /// </summary>
            Attack,
            /// <summary>
            /// 데미지 입는 중
            /// </summary>
            Damage,
            /// <summary>
            /// 죽음
            /// </summary>
            Dead,
            /// <summary>
            /// 움직이지 못함
            /// </summary>
            DontMove
        }
        /// <summary>
        /// 캐릭터 등급
        /// </summary>
        enum Grade
        {
            None,
            Common,
            Boss,
        }
        /// <summary>
        /// 캐릭터 정렬
        /// </summary>
        enum CharacterSortingOrder
        {
            Normal,
            AlwaysOnTop,
            AlwaysOnBottom,
            Fixed
        }
        
        /// <value>
        /// The <c>Uid</c> property represents a label for this instance.
        /// </value>
        /// <remarks>
        /// The <see cref="Uid"/> is a <see langword="string"/>
        /// that you use for a label.
        /// <para>
        /// Note that there isn't a way to provide a "cref" to
        /// each accessor, only to the property itself.
        /// </para>
        /// </remarks>
        int Uid { get; set; }
        /// <value>캐릭터 생성시 부여된 id</value>
        int Vid { get; set; }
        /// <value>캐릭터 기본 hp</value>
        float StatHp { get; set; }
        /// <value>캐릭터 기본 공격력</value>
        float StatAtk { get; set; }
        /// <value>캐릭터 기본 이동거리</value>
        float StatMoveStep { get; set; }
        /// <value>캐릭터 기본 이동속도</value>
        float StatMoveSpeed { get; set; }
        
        /// <value>캐릭터 현재 hp</value>
        long CurrentHp { get; set; }
        /// <value>캐릭터 현재 공격력</value>
        long CurrentAtk { get; set; }
        /// <value>캐릭터 현재 이동거리</value>
        float CurrentMoveStep { get; set; }
        /// <value>캐릭터 현재 이동속도</value>
        float CurrentMoveSpeed { get; set; }
        CharacterStatus CurrentStatus { get; set; }
        
        CharacterSortingOrder SortingOrder { get; set; }
        bool PossibleAttack { get; set; }
        float OriginalScaleX { get; set; }
        bool IsAttacking { get; set; }
        bool Flip { get; set; }
        string MyTag { get; set; }
        
        void Move(Vector3 direction);
        void TakeDamage(int damage);
        
    }
}