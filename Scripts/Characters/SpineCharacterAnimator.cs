using Spine.Unity;
using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    public class SpineCharacterAnimator : MonoBehaviour, ICharacterAnimator
    {
        public SkeletonAnimation skeletonAnimation;
        public AnimationReferenceAsset moveAnimation, attackAnimation;

        public void PlayMoveAnimation(Vector2 direction)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, moveAnimation, true);
        }

        public void PlayAttackAnimation()
        {
            skeletonAnimation.AnimationState.SetAnimation(0, attackAnimation, false);
        }
    }
}