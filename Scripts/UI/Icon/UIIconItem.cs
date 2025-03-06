using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI.Window;
using UnityEngine.EventSystems;

namespace GGemCo.Scripts.UI.Icon
{
    public class UIIconItem : UIIcon, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        UIWindowItemInfo uiWindowItemInfo;
        private StruckTableItem struckTableItem;
        private TableItem tableItem;
        
        protected override void Awake()
        {
            base.Awake();
            struckTableItem = null;
            tableItem = TableLoaderManager.Instance.TableItem;
        }

        protected override void Start()
        {
            base.Start();
            uiWindowItemInfo =
                SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowItemInfo>(
                    UIWindowManager.WindowUid.ItemInfo);
        }
        /// <summary>
        /// 다른 uid 로 변경하기
        /// </summary>
        /// <param name="iconUid"></param>
        /// <param name="remainCoolTime"></param>
        public override void ChangeInfoByUid(int iconUid, int remainCoolTime = 0)
        {
            base.ChangeInfoByUid(iconUid, remainCoolTime);
            var info = tableItem.GetDataByUid(iconUid);
            if (info == null || info.Uid <= 0) return;
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            // GcLogger.Log("OnPointerEnter "+eventData);
            uiWindowItemInfo.SetItemUid(uid);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // GcLogger.Log("OnPointerExit "+eventData);
            uiWindowItemInfo.Show(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // uiWindowItemInfo.SetItemUid(uid);
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
