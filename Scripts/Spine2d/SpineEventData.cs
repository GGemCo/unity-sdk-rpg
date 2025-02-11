#if GGEMCO_USE_SPINE
using System;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace GGemCo.Scripts.Spine2d
{
    [Serializable]
    public class SpineEventData : MonoBehaviour
    {
        public int uid;

        string targetEventName = "targetEvent";
        //string targetEventNameInFolder = "eventFolderName/targetEvent";
        private EventData targetEventData;

        private SkeletonAnimation skeletonAnimation;
        private Spine.AnimationState animationState;

        private SpineEventManager fGSpineEventManager;
        private void Awake()
        {
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            animationState = skeletonAnimation.AnimationState;

            // registering for events raised by any animation
            animationState.Start += OnSpineAnimationStart;
            animationState.Interrupt += OnSpineAnimationInterrupt;
            animationState.End += OnSpineAnimationEnd;
            animationState.Dispose += OnSpineAnimationDispose;
            animationState.Complete += OnSpineAnimationComplete;
            animationState.Event += OnUserDefinedEvent;
            //FG_Logger.Log("Awake ");
        }
        public void OnSpineAnimationStart(TrackEntry trackEntry)
        {
            // Add your implementation code here to react to start events
            //FG_Logger.Log("OnSpineAnimationStart " + trackEntry);
        }
        public void OnSpineAnimationInterrupt(TrackEntry trackEntry)
        {
            // Add your implementation code here to react to interrupt events
            //FG_Logger.Log("OnSpineAnimationInterrupt " + trackEntry);
        }
        public void OnSpineAnimationEnd(TrackEntry trackEntry)
        {
            // Add your implementation code here to react to end events
            //FG_Logger.Log("OnSpineAnimationEnd " + trackEntry);
        }
        public void OnSpineAnimationDispose(TrackEntry trackEntry)
        {
            // Add your implementation code here to react to dispose events
            //FG_Logger.Log("OnSpineAnimationDispose " + trackEntry);
        }
        public void OnSpineAnimationComplete(TrackEntry trackEntry)
        {
            // Add your implementation code here to react to complete events
            //FG_Logger.Log("OnSpineAnimationComplete " + trackEntry);
            Invoke(nameof(SetDisable), 1f);
        }
        public void OnUserDefinedEvent(TrackEntry trackEntry, Spine.Event e)
        {

            if (e.Data == targetEventData)
            {
                // Add your implementation code here to react to user defined event
            }
        }
        private void Start()
        {
            fGSpineEventManager = GameObject.Find("FG_SpineEventManager").GetComponent<SpineEventManager>();

            targetEventData = skeletonAnimation.Skeleton.Data.FindEvent(targetEventName);

            FirstSetDisable();
        }
        // 애니메이션 재생
        public void PlayAnimation(string animationName, bool loop = false)
        {
            if (skeletonAnimation == null) return;
            skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);

            /*
            trackEntry.Start += OnSpineAnimationStart;
            trackEntry.Interrupt += OnSpineAnimationInterrupt;
            trackEntry.End += OnSpineAnimationEnd;
            trackEntry.Dispose += OnSpineAnimationDispose;
            trackEntry.Complete += OnSpineAnimationComplete;
            trackEntry.Event += OnUserDefinedEvent;
            */
        }

        public void FirstSetDisable()
        {
            //Invoke("setDisable", 1f);
        }
        private void SetDisable()
        {
            gameObject.SetActive(false);
        }
        private void OnDisable()
        {
            //FG_Logger.Log("OnDisable ");
            Invoke(nameof(AddDestroyObject), 1f);
        }
        private void AddDestroyObject()
        {
            if (fGSpineEventManager == null) return;

            fGSpineEventManager.OnDisableObject(this.gameObject);
        }
    }
}
#endif