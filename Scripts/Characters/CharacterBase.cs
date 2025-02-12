using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    public abstract class CharacterBase : MonoBehaviour
    {
        public string characterName;
        public float health;
        public float moveSpeed;

        protected Animator Animator;

        public abstract void Move(Vector2 direction);
        public abstract void Attack();
    }
}