using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    public class SpriteCharacterAnimator : MonoBehaviour, ICharacterAnimator
    {
        public Animator animator;

        public void PlayMoveAnimation(Vector2 direction)
        {
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveY", direction.y);
        }

        public void PlayAttackAnimation()
        {
            animator.SetTrigger("Attack");
        }
    }
}