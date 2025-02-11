#if GGEMCO_USE_SPINE
using GGemCo.Scripts.Core;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.Utils;
using Spine;
using Spine.Unity;
using UnityEngine;
using Event = Spine.Event;

namespace GGemCo.Scripts.Spine2d
{
    /// <summary>
    /// 스파인 컨트롤러
    /// </summary>
    public class Spine2dUIController : MonoBehaviour
    {
        [HideInInspector] public SkeletonGraphic skeletonGraphic;

        private void Awake() {
            // Spine 오브젝트의 SkeletonAnimation 컴포넌트 가져오기
            skeletonGraphic = GetComponent<SkeletonGraphic>();
            if (skeletonGraphic != null)
            {
                skeletonGraphic.AnimationState.Event += HandleEvent;
            }
        }
        private void HandleEvent(TrackEntry trackEntry, Event e)
        {
            // Logger.Log("effect spine event: "+e.Data.Name);
            if (e.Data.Name == SpineEventManager.EventNameAttack)
            {
                // FG_Logger.Log("hit event " + this.gameObject.name + " | json: " + e.String);
                OnSpineEventHit(e);
            }
            else if (e.Data.Name == SpineEventManager.EventNameSound)
            {
                OnSpineEventSound(e);
            }
            else if (e.Data.Name == SpineEventManager.EventNameShake)
            {
                if (e.Float <= 0) return;
                SceneGame.Instance.cameraManager.StartShake(e.Float, 0.1f);
            }
        }
        private void OnSpineEventHit(Event eEvent) 
        {
        
        }
        private void OnSpineEventSound(Event eEvent) 
        {
            int soundUid = eEvent.Int;
            if (soundUid <= 0) return;
        }

        /// <summary>
        /// 애니메이션 재생
        /// </summary>
        /// <param name="animationName"></param>
        /// <param name="loop"></param>
        public void PlayAnimation(string animationName, bool loop = false)
        {
            if (skeletonGraphic == null) return;
            //  FG_Logger.Log("PlayAnimation gameobject: " + this.gameObject.name + " / animationName: " + animationName + " / " + loop);
            skeletonGraphic.AnimationState.SetAnimation(0, animationName, loop);
        }
        /// <summary>
        /// 현재 재생 중인 애니메이션 이름 가져오기
        /// </summary>
        /// <returns></returns>
        public string GetCurrentAnimation()
        {
            if (skeletonGraphic == null || skeletonGraphic.AnimationState == null) return null;
            TrackEntry currentEntry = skeletonGraphic.AnimationState.GetCurrent(0);
            return currentEntry?.Animation.Name;
        }
        /// <summary>
        /// 애니메이션을 한 번 실행하고, 그 후에 다른 애니메이션을 loop로 실행
        /// </summary>
        /// <param name="animationName"></param>
        public void PlayAnimationOnceAndThenLoop(string animationName)
        {
            if (skeletonGraphic == null) return;
            // FG_Logger.Log("PlayAnimationOnceAndThenLoop gameobject: " + this.gameObject.name + " / animationName: " + animationName );
            // 애니메이션 실행
            skeletonGraphic.AnimationState.SetAnimation(0, animationName, false);

            // 애니메이션 이벤트 리스너 등록
            skeletonGraphic.AnimationState.Complete += OnAnimationCompleteToIdle;
        }
        /// <summary>
        /// 애니메이션이 끝나면 호출되는 콜백 함수
        /// </summary>
        /// <param name="entry"></param>
        public void OnAnimationCompleteToIdle(TrackEntry entry)
        {
            if (skeletonGraphic == null) return;
            // 애니메이션 이벤트 리스너 제거
            skeletonGraphic.AnimationState.Complete -= OnAnimationCompleteToIdle;

            // 다른 애니메이션 loop로 실행
            skeletonGraphic.AnimationState.SetAnimation(0, SpineCharacter.CharacterDefaultAnimationName["idle"], true);
        }
        public void AddCompleteEvent() 
        {
            if (skeletonGraphic == null) return;
            skeletonGraphic.AnimationState.Complete += OnAnimationCompleteToIdle;
        }
        public void RemoveCompleteEvent() 
        {
            if (skeletonGraphic == null) return;
            skeletonGraphic.AnimationState.Complete -= OnAnimationCompleteToIdle;
        }
        public float GetAnimationDuration(string animationName, bool isMilliseconds = true)
        {
            if (skeletonGraphic == null) return 0;
            var findAnimation = skeletonGraphic.Skeleton.Data.FindAnimation(animationName);

            if (findAnimation == null)
            {
                GcLogger.LogWarning($"Animation '{animationName}' not found.");
                return 0;
            }

            float duration = findAnimation.Duration;
            return isMilliseconds ? duration * 1000 : duration;
        }
        public void SetTrackNoEnd(int trackId = 0)
        {
            if (skeletonGraphic == null) return;
            TrackEntry trackEntry = skeletonGraphic.AnimationState.GetCurrent(trackId);
            if(trackEntry == null) return;
            trackEntry.AnimationEnd = 999999f;
        }

        public void StopAnimation(int trackId = 0)
        {
            if (skeletonGraphic == null) return;
            skeletonGraphic.AnimationState.SetEmptyAnimation(trackId, 0);
            skeletonGraphic.AnimationState.ClearTrack(trackId);
        }
    }
}
#endif