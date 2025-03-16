using System.Collections.Generic;
using GGemCo.Scripts.Spine2d;
using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    /// <summary>
    /// 애니메이션 관리
    /// </summary>
    public interface ICharacterAnimationController
    {
        public const string WalkForwardAnim = "run";
        public const string WalkBackwardAnim = "run";
        public const string WaitForwardAnim = "wait";
        public const string WaitBackwardAnim = "wait";
        public const string AttackAnim = "attack";
        public const string DeadAnim = "die";
        
        void PlayWaitAnimation();
        void PlayRunAnimation();
        void PlayAttackAnimation();
        void PlayDeadAnimation();

        float GetCharacterHeight();

        /// <summary>
        /// 캐릭터 width 값 구하기
        /// </summary>
        /// <returns></returns>
        float GetCharacterWidth();

        Vector2 GetCharacterSize();

        void ChangeCharacterImageInSlot(List<StruckChangeSlotImage> changeSlotImages);
        void RemoveCharacterImageInSlot(List<StruckChangeSlotImage> changeSlotImages);
    }
}