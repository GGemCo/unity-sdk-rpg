using System.Collections.Generic;
using GGemCo.Scripts.Spine2d;
using Spine;
using UnityEngine;
using Event = Spine.Event;

namespace GGemCo.Scripts.Characters
{
    /// <summary>
    /// 캐릭터 스파인 애니메이션 처리
    /// </summary>
    public class CharacterCharacterAnimationControllerSpine : Spine2dController, ICharacterAnimationController
    {
        private CharacterBase characterBase;
        protected override void Awake()
        {
            base.Awake();
            characterBase = GetComponent<CharacterBase>();
            // 초기에는 빈값으로 넣어줘야 PlayAnimation 함수가 호출된다.
            SkeletonAnimation.AnimationState.SetEmptyAnimation(0, 0);
        }

        protected override void Start()
        {
            base.Start();
            // 스파인 height 값 구하고 character 에 넘겨주기
            characterBase.SetHeight(GetHeight());
        }

        private void UpdateDirection()
        {
            float scaleX = characterBase.directionPrev.x >= 0 ? -1 : 1;
            characterBase.transform.localScale = new Vector3(characterBase.OriginalScaleX * scaleX, characterBase.transform.localScale.y,characterBase.transform.localScale.z);
        }

        public void PlayWaitAnimation()
        {
            UpdateDirection();
            string idleAnim = characterBase.directionPrev.y != 0 
                ? (characterBase.directionPrev.y > 0 ? ICharacterAnimationController.WaitBackwardAnim : ICharacterAnimationController.WaitForwardAnim) 
                : ICharacterAnimationController.WaitForwardAnim;
            if (GetCurrentAnimation() == idleAnim) return;
            PlayAnimation(idleAnim,true, characterBase.GetCurrentMoveSpeed());
        }

        public void PlayRunAnimation()
        {
            UpdateDirection();
            characterBase.directionPrev = characterBase.direction;
            string moveAnim = characterBase.direction.y != 0 
                ? (characterBase.direction.y > 0 ? ICharacterAnimationController.WalkBackwardAnim : ICharacterAnimationController.WalkForwardAnim) 
                : ICharacterAnimationController.WalkForwardAnim;
            if (GetCurrentAnimation() == moveAnim) return;
            PlayAnimation(moveAnim, true, characterBase.GetCurrentMoveSpeed());
        }

        public void UpdateAnimation()
        {
            if (characterBase.IsAttacking) return; // 공격 중이면
            if (characterBase.IsStatusDead()) return; // 죽었으면
            
            // 정지 상태 처리
            if (characterBase.direction == Vector3.zero)
            {
                PlayWaitAnimation();
            }
            // 이동 상태 처리
            else
            {
                PlayRunAnimation();
            }
        }

        public float GetCharacterHeight()
        {
            return GetHeight();
        }

        public float GetCharacterWidth()
        {
            return GetWidth();
        }

        public Vector2 GetCharacterSize()
        {
            return GetSize();
        }

        public void ChangeCharacterImageInSlot(List<StruckChangeSlotImage> changeSlotImages)
        {
            ChangeImageInSlot(changeSlotImages);
        }

        public void PlayAttackAnimation()
        {
            UpdateDirection();
            List<StruckAddAnimation> addAnimations = new List<StruckAddAnimation>
            {
                new StruckAddAnimation(ICharacterAnimationController.WaitForwardAnim, true, 0, characterBase.GetCurrentMoveSpeed())
            };
            PlayAnimation(ICharacterAnimationController.AttackAnim, false, characterBase.GetCurrentAttackSpeed(), addAnimations);
        }
        public void PlayDeadAnimation()
        {
            PlayAnimation(ICharacterAnimationController.DeadAnim);
        }
        /// <summary>
        /// 애니메이션이 중단되면 호출되는 콜백 함수
        /// </summary>
        /// <param name="entry"></param>
        protected override void OnAnimationInterrupt(TrackEntry entry)
        {
            // GcLogger.Log("OnAnimationInterrupt gameobject: " + this.gameObject.name + " / animationName: " + entry.Animation.Name);
            if (SkeletonAnimation == null) return;
            if (entry.Animation.Name == ICharacterAnimationController.AttackAnim)
            {
                characterBase.IsAttacking = false; // 공격 상태 해제
            }
        }
        protected override void OnSpineEventShake(Event eEvent) 
        {
        
        }

        protected override void OnSpineEventAttack(Event eEvent)
        {
            characterBase.OnSpineEventAttack();
        }

        protected override void OnSpineEventSound(Event eEvent) 
        {
        }

    }
}