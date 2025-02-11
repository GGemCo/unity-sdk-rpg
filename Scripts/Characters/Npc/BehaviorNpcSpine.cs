#if GGEMCO_USE_SPINE
using System.Collections;
using GGemCo.Scripts.Spine2d;
using Spine.Unity;
using UnityEngine;

namespace GGemCo.Scripts.Characters.Npc
{
    public class BehaviorNpcSpine : Spine2dController
    {
        private Npc npc;

        protected override void Awake()
        {
            base.Awake();
            npc = GetComponent<Npc>();
        }
        
        public override IEnumerator FadeEffect(float duration, bool fadeIn)
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

            npc.isStartFade = false;
        }
    }
}
#endif