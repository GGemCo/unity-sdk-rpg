#if GGEMCO_USE_SPINE
using System.Collections.Generic;
using Spine;

namespace GGemCo.Scripts.Spine2d
{
    /// <summary>
    /// 캐릭터가 사용하는 스파인 클레스
    /// </summary>
    public class SpineCharacter : Spine2dController
    {
        public static readonly Dictionary<string, string> CharacterDefaultAnimationName = new Dictionary<string, string>
        {
            { "idle", "idle" },
            { "run", "run" },
            { "damage", "damage" },
            { "attack", "attack" },
            { "attack1", "attack1" }
        };
        /// <summary>
        /// 스킨 바꾸기
        /// </summary>
        /// <param name="skinName"></param>
        protected void ChangeSkin(string skinName)
        {
            if (skinName == "") return;
            if (SkeletonAnimation.Skeleton.Data.FindSkin(skinName) == null) return;
            SkeletonAnimation.Skeleton.SetSkin(skinName);
            SkeletonAnimation.Skeleton.SetSlotsToSetupPose(); // see note below
        }
        /// <summary>
        /// 여러 스킨 합치기. 예) 얼굴, 무기 스킨 적용하기
        /// </summary>
        /// <param name="skinNames"></param>
        protected bool CombiningSkins(params string[] skinNames)
        {
            Skin customSkin = new Skin("custom");

            Skeleton skeleton = SkeletonAnimation.Skeleton;
            SkeletonData skeletonData = skeleton.Data;
            foreach (var skinName in skinNames)
            {
                if (skinName == "") return false;
                customSkin.AddSkin(skeletonData.FindSkin(skinName));
            }
            skeleton.SetSkin(customSkin);
            skeleton.SetSlotsToSetupPose();
            return true;
        }
    }
}
#endif