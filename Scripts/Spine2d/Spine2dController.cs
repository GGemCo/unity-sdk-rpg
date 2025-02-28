#if GGEMCO_USE_SPINE
using GGemCo.Scripts.Characters;
using Spine;
using Spine.Unity;
using GGemCo.Scripts.Utils;
using Event = Spine.Event;

namespace GGemCo.Scripts.Spine2d
{
    /// <summary>
    /// 스파인 컨트롤러
    /// </summary>
    public class Spine2dController : DefaultCharacterBehavior
    {
        protected SkeletonAnimation SkeletonAnimation;

        protected override void Awake() {
            // Spine 오브젝트의 SkeletonAnimation 컴포넌트 가져오기
            SkeletonAnimation = GetComponent<SkeletonAnimation>();

            if (SkeletonAnimation == null)
            {
                GcLogger.LogError("SkeletonAnimation component not found!");
            }
            SkeletonAnimation.AnimationState.Event += HandleEvent;
        }

        private void HandleEvent(TrackEntry trackEntry, Event e)
        {
            // Logger.Log("effect spine event: "+e.Data.Name);
            if (e.Data.Name == Spine2dConstants.EventNameAttack)
            {
                // FG_Logger.Log("hit event " + this.gameObject.name + " | json: " + e.String);
                OnSpineEventAttack(e);
            }
            else if (e.Data.Name == Spine2dConstants.EventNameSound)
            {
                OnSpineEventSound(e);
            }
            else if (e.Data.Name == Spine2dConstants.EventNameShake)
            {
                if (e.Float <= 0) return;
                OnSpineEventShake(e);
            }
        }

        protected virtual void OnSpineEventShake(Event eEvent) 
        {
        
        }

        protected virtual void OnSpineEventAttack(Event eEvent) 
        {
            
        }

        protected virtual void OnSpineEventSound(Event eEvent) 
        {
        }
        protected override void Start()
        {
        }
        /// <summary>
        /// 애니메이션 재생
        /// </summary>
        /// <param name="animationName"></param>
        /// <param name="loop"></param>
        protected virtual void PlayAnimation(string animationName, bool loop = false)
        {
            if (SkeletonAnimation == null) return;
            //  FG_Logger.Log("PlayAnimation gameobject: " + this.gameObject.name + " / animationName: " + animationName + " / " + loop);
            SkeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
        }
        /// <summary>
        /// 현재 재생 중인 애니메이션 이름 가져오기
        /// </summary>
        /// <returns></returns>
        protected string GetCurrentAnimation()
        {
            if (SkeletonAnimation == null || SkeletonAnimation.AnimationState == null) return null;
            TrackEntry currentEntry = SkeletonAnimation.AnimationState.GetCurrent(0);
            return currentEntry?.Animation.Name;
        }
        /// <summary>
        /// 애니메이션을 한 번 실행하고, 그 후에 다른 애니메이션을 loop로 실행
        /// </summary>
        /// <param name="animationName"></param>
        protected virtual void PlayAnimationOnceAndThenLoop(string animationName)
        {
            if (SkeletonAnimation == null) return;
            // FG_Logger.Log("PlayAnimationOnceAndThenLoop gameobject: " + this.gameObject.name + " / animationName: " + animationName );
            // 애니메이션 실행
            SkeletonAnimation.AnimationState.SetAnimation(0, animationName, false);

            // 애니메이션 이벤트 리스너 등록
            SkeletonAnimation.AnimationState.Complete += OnAnimationCompleteToIdle;
        }
        /// <summary>
        /// 애니메이션이 끝나면 호출되는 콜백 함수
        /// </summary>
        /// <param name="entry"></param>
        protected virtual void OnAnimationCompleteToIdle(TrackEntry entry)
        {
            if (SkeletonAnimation == null) return;
            // 애니메이션 이벤트 리스너 제거
            SkeletonAnimation.AnimationState.Complete -= OnAnimationCompleteToIdle;

            // 다른 애니메이션 loop로 실행
            SkeletonAnimation.AnimationState.SetAnimation(0, SpineCharacter.CharacterDefaultAnimationName["idle"], true);
        }
        protected virtual void AddCompleteEvent() 
        {
            SkeletonAnimation.AnimationState.Complete += OnAnimationCompleteToIdle;
        }
        protected virtual void RemoveCompleteEvent() 
        {
            if (SkeletonAnimation == null) return;
            SkeletonAnimation.AnimationState.Complete -= OnAnimationCompleteToIdle;
        }
        protected float GetAnimationDuration(string animationName, bool isMilliseconds = true)
        {
            var findAnimation = SkeletonAnimation.Skeleton.Data.FindAnimation(animationName);

            if (findAnimation == null)
            {
                GcLogger.LogWarning($"Animation '{animationName}' not found.");
                return 0;
            }

            float duration = findAnimation.Duration;
            return isMilliseconds ? duration * 1000 : duration;
        }
        protected void SetTrackNoEnd(int trackId = 0)
        {
            if (SkeletonAnimation == null) return;
            TrackEntry trackEntry = SkeletonAnimation.AnimationState.GetCurrent(trackId);
            if(trackEntry == null) return;
            trackEntry.AnimationEnd = 999999f;
        }

        protected void StopAnimation(int trackId = 0)
        {
            if (SkeletonAnimation == null) return;
            SkeletonAnimation.AnimationState.SetEmptyAnimation(trackId, 0);
            SkeletonAnimation.AnimationState.ClearTrack(trackId);
        }
        protected bool IsPlaying()
        {
            if (SkeletonAnimation == null) return false;
            var state = SkeletonAnimation.AnimationState;
            // 각 트랙에서 현재 애니메이션이 있는지 확인
            for (int i = 0; i < state.Tracks.Count; i++)
            {
                if (state.Tracks.Items[i] != null && state.Tracks.Items[i].Animation != null)
                {
                    return true; // 재생 중인 애니메이션이 존재함
                }
            }
            return false; // 재생 중인 애니메이션이 없음
        }
        /// <summary>
        /// 캐릭터 height 값 구하기
        /// </summary>
        /// <returns></returns>
        public override float GetCharacterHeight()
        {
            // Skeleton에서 바운딩 박스 계산
            float[] vertexBuffer = new float[8];
            SkeletonAnimation.Skeleton.GetBounds(out float x, out float y, out float width, out float height, ref vertexBuffer);
            return height;
        }
        /// <summary>
        /// 캐릭터 width 값 구하기
        /// </summary>
        /// <returns></returns>
        protected override float GetCharacterWidth()
        {
            // Skeleton에서 바운딩 박스 계산
            float[] vertexBuffer = new float[8];
            SkeletonAnimation.Skeleton.GetBounds(out float x, out float y, out float width, out float height, ref vertexBuffer);
            return width;
        }
    }
}
#endif