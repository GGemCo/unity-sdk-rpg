using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    public abstract class CharacterBase : MonoBehaviour
    {
        public string characterName;
        public float health;
        public float moveSpeed;

        protected Animator animator;
        protected Rigidbody2D rb;

        public abstract void Move(Vector2 direction);
        public abstract void Attack();
    }
}