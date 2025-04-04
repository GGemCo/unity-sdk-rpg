﻿using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;

namespace GGemCo.Scripts.UI.Icon
{
    /// <summary>
    /// 버프 아이콘
    /// </summary>
    public class UIIconBuff : UIIcon
    {
        private StruckTableAffect struckTableAffect;
        
        protected override void Awake()
        {
            base.Awake();
            windowUid = UIWindowManager.WindowUid.PlayerBuffInfo;
            IconType = IconConstants.Type.Buff;
        }
        public void Initialize(int affectUid)
        {
            if (affectUid <= 0) return;
            var info = TableLoaderManager.Instance.TableAffect.GetDataByUid(affectUid);
            if (info == null)
            {
                GcLogger.LogError("affect 테이블에 없는 어펙트 입니다. affect Uid: "+affectUid);
                return;
            }

            if (!ChangeInfoByUid(affectUid, 1)) return;

            uid = affectUid;

            if (info.Duration <= 0)
            {
                GcLogger.LogWarning("지속 시간이 0 입니다.");
            }

            struckTableAffect = info;
            UpdateInfo();
        }

        protected override void Start()
        {
            base.Start();
            SceneGame.Instance.uIIconCoolTimeManager.PlayCoolTime(windowUid, this, struckTableAffect.Duration);
        }
        /// <summary>
        /// 아이콘 이미지 경로 가져오기 
        /// </summary>
        /// <returns></returns>
        protected override string GetIconImagePath()
        {
            if (struckTableAffect == null) return null;
            return $"Images/Icon/Affect/Buff/{struckTableAffect.IconFileName}";
        }
    }
}