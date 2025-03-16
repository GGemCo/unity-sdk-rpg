using System.Collections.Generic;
using GGemCo.Scripts.Spine2d;
using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    /// <summary>
    /// 캐릭터 유니티 애니메이션 처리
    /// </summary>
    public class CharacterCharacterAnimationControllerSprite : MonoBehaviour, ICharacterAnimationController
    {
        public void PlayWaitAnimation()
        {
        }

        public void PlayRunAnimation()
        {
        }

        public void PlayAttackAnimation()
        {
        }

        public void PlayDeadAnimation()
        {
        }

        public void UpdateAnimation()
        {
        }

        public float GetCharacterHeight()
        {
            return 0f;
        }

        public float GetCharacterWidth()
        {
            return 0f;
        }

        public Vector2 GetCharacterSize()
        {
            return Vector2.zero;
        }

        public void ChangeCharacterImageInSlot(List<StruckChangeSlotImage> changeSlotImages)
        {
            
        }

        public void RemoveCharacterImageInSlot(List<StruckChangeSlotImage> changeSlotImages)
        {
            
        }
    }
}