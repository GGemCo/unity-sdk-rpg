using GGemCo.Scripts.Items;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI.Window;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GGemCo.Scripts.UI.Icon
{
    public class UIIconItem : UIIcon, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private UIWindowItemInfo uiWindowItemInfo;
        private StruckTableItem struckTableItem;
        private TableItem tableItem;
        
        protected override void Awake()
        {
            base.Awake();
            IconType = IIcon.Type.Item;
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
            SetCount(iconCount);
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
            ShowSelected(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // GcLogger.Log("OnPointerExit "+eventData);
            uiWindowItemInfo.Show(false);
            ShowSelected(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                Debug.Log("Mouse Click Button : Left");
                if (!window) return;
                window.SetSelectedIcon(index);
            }
            else if(eventData.button == PointerEventData.InputButton.Middle)
            {
                Debug.Log("Mouse Click Button : Middle");
            }
            else if(eventData.button == PointerEventData.InputButton.Right)
            {
                if (uid <= 0 || count <= 0) return;
                Debug.Log("Mouse Click Button : Right");
                window.OnRightClick(this);
            }

            Debug.Log("Mouse Position : " + eventData.position);
            Debug.Log("Mouse Click Count : " + eventData.clickCount);
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
        /// <summary>
        /// 장착 가능한 타입 인지 체크 
        /// </summary>
        /// <returns></returns>
        public bool IsTypeEquip()
        {
            return struckTableItem.Type == ItemConstants.Type.Equip;
        }
        /// <summary>
        /// 장착 가능한 부위 아이템인지 체크 
        /// </summary>
        /// <param name="toEquipIndex">착용하려는 부위 slot index</param>
        /// <returns></returns>
        public bool IsEquipParts(int toEquipIndex)
        {
            return (int)struckTableItem.PartsID == toEquipIndex;
        }
        /// <summary>
        /// 착용 부위 type 가져오기
        /// </summary>
        /// <returns></returns>
        public override ItemConstants.PartsType GetPartsType()
        {
            return struckTableItem.PartsID;
        }
        
        public override bool IsEquipType()
        {
            return IconType == IIcon.Type.Item && struckTableItem.Type == ItemConstants.Type.Equip;
        }
    }
}
