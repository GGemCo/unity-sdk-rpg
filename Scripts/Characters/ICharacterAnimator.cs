using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    public interface ICharacterAnimator
    {
        void PlayMoveAnimation(Vector2 direction);
        void PlayAttackAnimation();
    }
}