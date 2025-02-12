using System.Collections;
using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    public class DefaultCharacterBehavior : MonoBehaviour
    {
        protected Transform AttackerTransform;
        protected bool IsAttacking;

        protected Vector3 Direction;
        protected Vector3 DirectionPrev;

        public string walkForwardAnim = "run";
        public string walkBackwardAnim = "run";
        public string waitForwardAnim = "wait";
        public string waitBackwardAnim = "wait";
        public string attackAnim = "attack";
        public string deadAnim = "die";
        
        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
            
        }

        public virtual float GetCharacterHeight()
        {
            return 0;
        }
        public virtual float GetCharacterWidth()
        {
            return 0;
        }
        public virtual Vector2 GetCharacterSize()
        {
            return new Vector2(GetCharacterWidth(), GetCharacterHeight());
        }

        public virtual IEnumerator FadeEffect(float duration, bool fadeIn)
        {
            yield return null;
        }
        /// <summary>
        /// 공격한 오브젝트 설정하기 
        /// </summary>
        /// <param name="attacker"></param>
        public void SetAttackerTarget(Transform attacker)
        {
            AttackerTransform = attacker;
        }
        public Transform GetAttackerTarget()
        {
            return AttackerTransform;
        }

        public virtual void PlayDeadAnimation()
        {
            
        }
    }
}