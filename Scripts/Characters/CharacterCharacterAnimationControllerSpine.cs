using System.Collections;
using System.Collections.Generic;
using GGemCo.Scripts.Spine2d;
using Spine;
using Spine.Unity;
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

        public void PlayWaitAnimation()
        {
            string idleAnim = characterBase.directionPrev.y != 0 
                ? (characterBase.directionPrev.y > 0 ? ICharacterAnimationController.WaitBackwardAnim : ICharacterAnimationController.WaitForwardAnim) 
                : ICharacterAnimationController.WaitForwardAnim;
            if (GetCurrentAnimation() == idleAnim) return;
            PlayAnimation(idleAnim,true, characterBase.GetCurrentMoveSpeed());
        }

        public void PlayRunAnimation()
        {
            string moveAnim = characterBase.direction.y != 0 
                ? (characterBase.direction.y > 0 ? ICharacterAnimationController.WalkBackwardAnim : ICharacterAnimationController.WalkForwardAnim) 
                : ICharacterAnimationController.WalkForwardAnim;
            if (GetCurrentAnimation() == moveAnim) return;
            PlayAnimation(moveAnim, true, characterBase.GetCurrentMoveSpeed());
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
        public void RemoveCharacterImageInSlot(List<StruckChangeSlotImage> changeSlotImages)
        {
            RemoveImageInSlot(changeSlotImages);
        }

        public void PlayAttackAnimation()
        {
            PlayAnimation(ICharacterAnimationController.AttackAnim, false, characterBase.GetCurrentAttackSpeed());
        }
        public void PlayDeadAnimation()
        {
            PlayAnimation(ICharacterAnimationController.DeadAnim);
        }
        /// <summary>
        /// 애니메이션이 중단되면 호출되는 콜백 함수
        /// </summary>
        /// <param name="entry"></param>
        protected override void OnAnimationComplete(TrackEntry entry)
        {
            // GcLogger.Log("OnAnimationInterrupt gameobject: " + this.gameObject.name + " / animationName: " + entry.Animation.Name);
            if (SkeletonAnimation == null) return;
            if (entry.Animation.Name == ICharacterAnimationController.AttackAnim)
            {
                characterBase.SetStatusIdle(); // 공격 상태 해제
                PlayWaitAnimation();
            }
        }
        protected override void OnSpineEventShake(Event eEvent) 
        {
        
        }

        protected override void OnSpineEventAttack(Event eEvent)
        {
            characterBase.OnEventAttack();
        }

        protected override void OnSpineEventSound(Event eEvent) 
        {
        }

        public IEnumerator FadeEffect(float duration, bool fadeIn)
        {
            float elapsedTime = 0f;
            float startAlpha = fadeIn ? 0 : 1;
            float endAlpha = fadeIn ? 1 : 0;

            Color color = SkeletonAnimation.Skeleton.GetColor();

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                SkeletonAnimation.Skeleton.SetColor(color);
                yield return null;
            }

            characterBase.SetIsStartFade(false);
        }
        /// <summary>
        /// track index 의 time scale 변경해주기
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        public void UpdateTimeScaleByTrackIndex(float value, int index = 0)
        {
            TrackEntry trackEntry = SkeletonAnimation.AnimationState.GetCurrent(index);
            if (trackEntry == null) return;
            trackEntry.TimeScale = value;
        } 
        
    }
}