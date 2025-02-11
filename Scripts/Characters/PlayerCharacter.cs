using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    public class PlayerCharacter : CharacterBase
    {
        private ICharacterAnimator characterAnimator;

        private void Start()
        {
            // characterAnimator = GetComponent<ICharacterAnimator>();
#if GGEMCO_USE_SPINE
            characterAnimator = gameObject.AddComponent<SpineCharacterAnimator>();
#else
            characterAnimator = gameObject.AddComponent<SpriteCharacterAnimator>();
#endif
        }

        private void Update()
        {
            Vector2 inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            Move(inputDirection);

            if (Input.GetButtonDown("Fire1"))
            {
                Attack();
            }
        }

        public override void Move(Vector2 direction)
        {
            rb.velocity = direction * moveSpeed;
            characterAnimator.PlayMoveAnimation(direction);
        }

        public override void Attack()
        {
            characterAnimator.PlayAttackAnimation();
        }
    }
}