using System.Collections.Generic;
using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI.Icon;
using GGemCo.Scripts.UI.Inventory;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GGemCo.Scripts.UI.WindowShop
{
    /// <summary>
    /// 플레이어 스킬
    /// </summary>
    public class UIWindowShop : UIWindow
    {
        [Tooltip("아이템 정보 윈도우")]
        public UIWindowItemInfo uIWindowItemInfo;
        [Tooltip("샵 element 프리팹")]
        public GameObject prefabUIElementShop;
        
        private TableShop tableShop;
        private readonly Dictionary<int, UIElementShop> uiElementShops = new Dictionary<int, UIElementShop>();

        private int currentShopUid;
        
        protected override void Awake()
        {
            currentShopUid = 0;
            uiElementShops.Clear();
            uid = UIWindowManager.WindowUid.Skill;
            if (TableLoaderManager.Instance == null) return;
            tableShop = TableLoaderManager.Instance.TableShop;
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }
        /// <summary>
        /// skill 테이블을 읽어서 개수만큼 풀을 확장하여 추가 생성.
        /// </summary>
        public void SetInfoByShopUid(int shopUid)
        {
            // 같은 상점을 열었으면 업데이트 하지 않는다
            if (currentShopUid > 0 && currentShopUid == shopUid) return;
            // 기존 element 지우기
            int index = 0;
            foreach (var data in uiElementShops)
            {
                Destroy(data.Value.gameObject);
                if (slots[index])
                {
                    Destroy(slots[index].gameObject);
                }
                if (icons[index])
                {
                    Destroy(icons[index].gameObject);
                }
                index++;
            }

            slots = null;
            icons = null;
            uiElementShops.Clear();
            currentShopUid = shopUid;
            
            if (AddressableSettingsLoader.Instance == null || containerIcon == null) return;
            if (prefabUIElementShop == null)
            {
                GcLogger.LogError("UIElementShop 프리팹이 없습니다.");
                return;
            }
            if (shopUid <= 0) return;
            var datas = tableShop.GetDataByUid(shopUid);
            if (datas == null)
            {
                GcLogger.LogError("shop 테이블에 정보가 없습니다. shop Uid: " + shopUid);
                return;
            }
            maxCountIcon = datas.Count;
            if (datas.Count <= 0) return;
            slots = new GameObject[maxCountIcon];
            icons = new GameObject[maxCountIcon];
            
            GameObject iconItem = AddressablePrefabLoader.Instance.GetPreLoadGamePrefabByName(ConfigAddressables.KeyPrefabIconItem);
            GameObject slot = AddressablePrefabLoader.Instance.GetPreLoadGamePrefabByName(ConfigAddressables.KeyPrefabSlot);
            if (iconItem == null) return;

            index = 0;
            foreach (var info in datas)
            {
                GameObject parent = gameObject;
                // UI Element 프리팹이 있으면 만든다.
                if (prefabUIElementShop != null)
                {
                    parent = Instantiate(prefabUIElementShop, containerIcon.gameObject.transform);
                    if (parent == null) continue;
                    UIElementShop uiElementShop = parent.GetComponent<UIElementShop>();
                    if (uiElementShop == null) continue;
                    uiElementShop.Initialize(this, index, info);
                    uiElementShop.UpdateInfos(datas[index]);
                    uiElementShops.TryAdd(index, uiElementShop);
                }

                GameObject slotObject = Instantiate(slot, parent.transform);
                UISlot uiSlot = slotObject.GetComponent<UISlot>();
                if (uiSlot == null) continue;
                uiSlot.Initialize(this, uid, index, slotSize);
                SetPositionUiSlot(uiSlot, index);
                slots[index] = slotObject;
                
                GameObject icon = Instantiate(iconItem, slotObject.transform);
                UIIcon uiIcon = icon.GetComponent<UIIcon>();
                if (uiIcon == null) continue;
                // deactivate 상태에서는 awake 가 호출되지 않는다.
                uiIcon.Initialize(this, uid, index, index, iconSize, slotSize);
                // count  1로 초기화
                uiIcon.ChangeInfoByUid(info.ItemUid, 1);
                // element 에서 마우스 이벤트 처리
                uiIcon.SetRaycastTarget(false);
                
                icons[index] = icon;
                index++;
            }
            // GcLogger.Log($"풀 확장: {amount}개 아이템 추가 (총 {poolDropItem.Count}개)");
        }
        /// <summary>
        /// 슬롯 위치 정해주기
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="index"></param>
        private void SetPositionUiSlot(UISlot slot, int index)
        {
            UIElementShop uiElementSkill = uiElementShops[index];
            if (uiElementSkill == null) return;
            Vector3 position = uiElementSkill.GetIconPosition();
            if (position == Vector3.zero) return;
            slot.transform.localPosition = position;
        }

        public override void OnShow(bool show)
        {
            if (SceneGame.Instance == null || TableLoaderManager.Instance == null) return;
            if (!show)
            {
                uIWindowItemInfo?.Show(false);
                return;
            }
            // LoadIcons();
        }

        /// <summary>
        /// npc uid 정보로 아이콘 셋팅하기
        /// 상점이 열려있지 않으면 업데이트 하지 않음
        /// </summary>
        private void LoadIcons()
        {
            if (!gameObject.activeSelf) return;
            var datas = tableShop.GetDataByUid(currentShopUid);
            if (datas == null) return;
            for (int index = 0; index < maxCountIcon; index++)
            {
                if (index >= icons.Length) continue;
                var icon = icons[index];
                if (icon == null) continue;
                UIIconItem uiIcon = icon.GetComponent<UIIconItem>();
                if (uiIcon == null) continue;
                
                var info = TableLoaderManager.Instance.TableItem.GetDataByUid(datas[index].ItemUid);
                if (info == null) continue;
                uiIcon.ChangeInfoByUid(info.Uid, 1);
                UIElementShop uiElementShop = uiElementShops[index];
                if (uiElementShop != null)
                {
                    uiElementShop.UpdateInfos(datas[index]);
                }
            }
        }
        
        /// <summary>
        ///  window 밖에 드래그앤 드랍 했을때 처리 
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="droppedIcon"></param>
        /// <param name="targetIcon"></param>
        /// <param name="originalPosition"></param>
        public override void OnEndDragOutWindow(PointerEventData eventData, GameObject droppedIcon, GameObject targetIcon, Vector3 originalPosition)
        {
            // GcLogger.Log("OnEndDragOutWindow");
            GoBackToSlot(droppedIcon);
        }
        /// <summary>
        /// 아이콘 프리팹을 미리 만들어 놓기 때문에 무조건 아이콘 위에서 드래그가 끝나면 GameObject 는 존재한다. 
        /// </summary>
        /// <param name="droppedIcon">드랍한 한 아이콘</param>
        /// <param name="targetIcon">드랍되는 곳에 있는 아이콘</param>
        public override void OnEndDragInIcon(GameObject droppedIcon, GameObject targetIcon)
        {
            // GcLogger.Log("skill window. OnEndDragInIcon");
            UIIconItem droppedUIIcon = droppedIcon.GetComponent<UIIconItem>();
            UIWindow droppedWindow = droppedUIIcon.window;
            UIWindowManager.WindowUid droppedWindowUid = droppedUIIcon.windowUid;
            int dropIconSlotIndex = droppedUIIcon.slotIndex;
            int dropIconUid = droppedUIIcon.uid;
            int dropIconCount = droppedUIIcon.GetCount();
            if (dropIconUid <= 0)
            {
                GoBackToSlot(droppedIcon);
                return;
            }
            
            UIIconItem targetUIIcon = targetIcon.GetComponent<UIIconItem>();
            // 드래그앤 드랍 한 곳에 아무것도 없을때 
            if (targetUIIcon == null)
            {
                GoBackToSlot(droppedIcon);
                return;
            }
            UIWindow targetWindow = targetUIIcon.window;
            UIWindowManager.WindowUid targetWindowUid = targetUIIcon.windowUid;
            int targetIconSlotIndex = targetUIIcon.slotIndex;
            int targetIconUid = targetUIIcon.uid;
            int targetIconCount = targetUIIcon.GetCount();

            // 다른 윈도우에서 Skill로 드래그 앤 드랍 했을 때 
            if (droppedWindowUid != targetWindowUid)
            {
            }
            else
            {
                if (targetIconSlotIndex < maxCountIcon)
                {
                }
            }
            GoBackToSlot(droppedIcon);
        }
        /// <summary>
        /// 아이콘 우클릭했을때 처리 
        /// </summary>
        /// <param name="icon"></param>
        public override void OnRightClick(UIIcon icon)
        {
            
        }
        // protected override void OnSetIcon(int slotIndex, int iconUid, int iconCount, int iconLevel = 0, bool iconLearn = false)
        // {
        //     base.OnSetIcon(slotIndex, iconUid, iconCount, iconLevel, iconLearn);
        //     UIIcon uiIcon = GetIconByIndex(slotIndex);
        //     if (uiIcon == null) return;
        //     skillData.SetSkill(slotIndex, iconUid, iconCount, iconLevel, iconLearn);
        //     UIElementShop uiElementSkill = uiElementShops[slotIndex];
        //     if (uiElementSkill != null)
        //     {
        //         UIIconItem uiIconSkill = uiIcon.GetComponent<UIIconItem>();
        //         uiElementSkill.UpdateInfos(uiIconSkill.GetTableInfo(), uiIconSkill.GetSaveDataInfo());
        //     }
        // }
    }
}