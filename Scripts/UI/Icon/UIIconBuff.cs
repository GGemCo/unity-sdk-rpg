using GGemCo.Scripts.Characters;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;

namespace GGemCo.Scripts.UI.Icon
{
    public class UIIconBuff : UIIcon
    {
        private StruckTableItem struckTableItem;
        private TableItem tableItem;
        
        protected override void Awake()
        {
            base.Awake();
            IconType = IIcon.Type.Buff;
            struckTableItem = null;
            tableItem = TableLoaderManager.Instance.TableItem;
        }

        protected override void Start()
        {
            base.Start();
            CoolTimeHandler.PlayCoolTime();
        }

        public void Initialize(StruckBuff struckBuff)
        {
            if (struckBuff == null) return;
            float duration = struckBuff.Duration;
            if (duration == 0) return;
            CoolTimeHandler.SetCoolTime(duration);
            ChangeInfoByUid(struckBuff.Uid);
        }
        /// <summary>
        /// 다른 uid 로 변경하기
        /// </summary>
        /// <param name="iconUid"></param>
        /// <param name="iconCount"></param>
        /// <param name="remainCoolTime"></param>
        public override void ChangeInfoByUid(int iconUid, int iconCount = 0, int remainCoolTime = 0)
        {
            base.ChangeInfoByUid(iconUid, iconCount, remainCoolTime);
            if (iconUid == 0 && iconCount == 0)
            {
                ClearIconInfos();
                return;
            }
            var info = tableItem.GetDataByUid(iconUid);
            if (info == null)
            {
                GcLogger.LogError("아이콘 테이블에 없는 아이템 입니다.");
                return;
            }
            uid = iconUid;
            struckTableItem = info;
            UpdateInfo();
        }
        protected override bool UpdateInfo()
        {
            if (!base.UpdateInfo()) return false;
            UpdateIconImage();
            return true;
        }
        /// <summary>
        /// 아이콘 이미지 경로 가져오기 
        /// </summary>
        /// <returns></returns>
        protected override string GetIconImagePath()
        {
            if (struckTableItem == null) return null;
            return $"Images/Icon/{struckTableItem.Type.ToString()}/{struckTableItem.Category.ToString()}/{struckTableItem.SubCategory.ToString()}/{struckTableItem.ImagePath}";
        }
    }
}