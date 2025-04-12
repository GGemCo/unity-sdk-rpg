using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GGemCo.Scripts
{
    public class UIWindowStash : UIWindow
    {
        private SceneGame sceneGame;
        private UIWindowInventory uiWindowInventory;
        private UIWindowItemInfo uiWindowItemInfo;
        
        private StashData stashData;
        private InventoryData inventoryData;
            
        private TableItem tableItem;
        
        protected override void Awake()
        {
            uid = UIWindowManager.WindowUid.Stash;
            base.Awake();
        }
        protected override void Start()
        {
            base.Start();
            sceneGame = SceneGame.Instance;
            if (sceneGame != null && sceneGame.saveDataManager != null)
            {
                stashData = sceneGame.saveDataManager.Stash;
                inventoryData = sceneGame.saveDataManager.Inventory;
            }

            tableItem = TableLoaderManager.Instance.TableItem;
            uiWindowInventory = 
                sceneGame.uIWindowManager.GetUIWindowByUid<UIWindowInventory>(UIWindowManager.WindowUid
                    .Inventory);
            uiWindowItemInfo = 
                sceneGame.uIWindowManager.GetUIWindowByUid<UIWindowItemInfo>(UIWindowManager.WindowUid
                    .ItemInfo);
        }

        public override void OnShow(bool show)
        {
            if (sceneGame == null || TableLoaderManager.Instance == null) return;
            if (!show) return;
            LoadIcons();
        }
        /// <summary>
        /// 저장되어있는 아이템 정보로 아이콘 셋팅하기
        /// </summary>
        private void LoadIcons()
        {
            if (!gameObject.activeSelf) return;
            var datas = sceneGame.saveDataManager.Stash.GetAllItemCounts();
            if (datas == null) return;
            foreach (var info in datas)
            {
                int index = info.Key;
                if (index >= icons.Length) continue;
                var icon = icons[index];
                if (icon == null) continue;
                UIIconItem uiIcon = icon.GetComponent<UIIconItem>();
                if (uiIcon == null) continue;
                SaveDataIcon structInventoryIcon = info.Value;
                int itemUid = structInventoryIcon.Uid;
                int itemCount = structInventoryIcon.Count;

                if (itemUid <= 0 || itemCount <= 0)
                {
                    uiIcon.ClearIconInfos();
                    continue;
                }
                var table = tableItem.GetDataByUid(itemUid);
                if (table == null || table.Uid <= 0) continue;
                uiIcon.ChangeInfoByUid(table.Uid);
                uiIcon.SetCount(itemCount);
            }
        }
        
        /// <summary>
        /// 아이템 아이콘 드랍이 끝났을때 
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnDrop(PointerEventData eventData)
        {
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
            GoBackToSlot(droppedIcon);
        }
        /// <summary>
        /// 아이콘 위에서 드래그가 끝났을때 처리 
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

            // 인벤토리에서 창고로 드래그 앤 드랍 했을 때만 처리한다 
            if (droppedWindowUid == UIWindowManager.WindowUid.Inventory && targetIconSlotIndex < maxCountIcon)
            {
                // 보관할 수 있는 아이템 인지 체크
                if (droppedUIIcon.IsAntiFlag(ItemConstants.AntiFlag.Stash))
                {
                    sceneGame.systemMessageManager.ShowMessageWarning("해당 아이템은 보관할 수 없는 아이템 입니다.");
                }
                else
                {
                    // 보관된 아이템이 있을 때
                    if (targetIconUid > 0)
                    {
                        return;
                    }

                    var result = inventoryData.MinusItem(dropIconSlotIndex, dropIconUid, dropIconCount);
                    droppedWindow.SetIcons(result);

                    result = stashData.AddItem(targetIconSlotIndex, dropIconUid, dropIconCount);
                    targetWindow.SetIcons(result);
                }
            }
            else
            {
                // 보관할 수 있는 아이템 인지 체크
                if (droppedUIIcon.IsAntiFlag(ItemConstants.AntiFlag.Stash))
                {
                    sceneGame.systemMessageManager.ShowMessageWarning("해당 아이템은 보관할 수 없는 아이템 입니다.");
                }
                else
                {
                    if (targetIconSlotIndex < maxCountIcon)
                    {
                        // 같은 아이템일때 
                        if (dropIconUid == targetIconUid)
                        {
                            // 중첩 가능한지 체크
                            var info = tableItem.GetDataByUid(targetUIIcon.uid);
                            if (info is { MaxOverlayCount: > 1 })
                            {
                                var result = stashData.MergeItem(dropIconSlotIndex, targetIconSlotIndex);
                                droppedWindow.SetIcons(result);
                            }
                            else
                            {
                                droppedWindow.SetIconCount(dropIconSlotIndex, targetIconUid, targetIconCount);
                                targetWindow.SetIconCount(targetIconSlotIndex, dropIconUid, dropIconCount);
                            }
                        }
                        else
                        {
                            droppedWindow.SetIconCount(dropIconSlotIndex, targetIconUid, targetIconCount);
                            targetWindow.SetIconCount(targetIconSlotIndex, dropIconUid, dropIconCount);
                        }
                    }
                }
            }

            GoBackToSlot(droppedIcon);
        }
        public override bool Show(bool show)
        {
            if (!base.Show(show)) return false;
            uiWindowInventory?.Show(show);
            if (!show)
            {
                uiWindowInventory?.Show(false);
            }
            return true;
        }
        /// <summary>
        /// 아이콘 우클릭했을때 처리 
        /// </summary>
        /// <param name="icon"></param>
        public override void OnRightClick(UIIcon icon)
        {
            if (icon == null) return;
            SceneGame.Instance.uIWindowManager.MoveIcon(UIWindowManager.WindowUid.Stash, icon.index, UIWindowManager.WindowUid.Inventory, icon.GetCount());
        }
        /// <summary>
        /// 아이템 정보 보기
        /// </summary>
        /// <param name="icon"></param>
        public override void ShowItemInfo(UIIcon icon)
        {
            uiWindowItemInfo.SetItemUid(icon.uid);
            RectTransform itemInfoRect = uiWindowItemInfo.GetComponent<RectTransform>();
            itemInfoRect.pivot = new Vector2(1f, 1f);
            uiWindowItemInfo.transform.position =
                new Vector3(icon.transform.position.x + itemInfoRect.sizeDelta.x + slotSize.x / 2f,
                    icon.transform.position.y + slotSize.y / 2f, 0);

            // 화면 밖 체크 & 보정
            MathHelper.ClampToScreen(itemInfoRect);
        }
        protected override void OnSetIcon(int slotIndex, int iconUid, int iconCount, int iconLevel = 0, bool iconLearn = false)
        {
            base.OnSetIcon(slotIndex, iconUid, iconCount, iconLevel, iconLearn);
            UIIcon uiIcon = GetIconByIndex(slotIndex);
            if (uiIcon == null) return;
         
            stashData.SetItemCount(slotIndex, iconUid, iconCount);
        }

        protected override void OnDetachIcon(int slotIndex)
        {
            base.OnDetachIcon(slotIndex);
            UIIcon uiIcon = GetIconByIndex(slotIndex);
            if (uiIcon == null) return;
         
            stashData.RemoveItemCount(slotIndex);
        }
        /// <summary>
        /// index 가 없을때는, 같은 uid 는 중첩 가능여부를 확인하고 합치고, 나머지는 추가
        /// </summary>
        /// <param name="iconUid"></param>
        /// <param name="iconCount"></param>
        public override void SetIconCount(int iconUid, int iconCount)
        {
            ResultCommon result = stashData.AddItem(iconUid, iconCount);
            SetIcons(result);
        }
    }
}