using System;
using GGemCo.Scripts.Items;
using GGemCo.Scripts.SaveData;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI.Icon;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGemCo.Scripts.UI.Window
{
    public class StructInventoryIcon
    {
        public int ItemUid;
        public int ItemCount;

        public StructInventoryIcon(int uid, int count)
        {
            ItemUid = uid;
            ItemCount = count;
        }
    }
    public class UIWindowInventory : UIWindow
    {
        [Header("장비 윈도우")]
        public UIWindowEquip uIWindowEquip;
        [Header("플레이어 정보 윈도우")]
        public UIWindowPlayerInfo uiWindowPlayerInfo;
        [Header("아이템 정보 윈도우")]
        public UIWindowItemInfo uIWindowItemInfo;
        [Header("모든 아이템 합치기 버튼")]
        public Button buttonMergeAllItems;
        
        private GameObject iconItem;
        private TableItem tableItem;
        private Camera mainCamera;
        private ItemManager itemManager;
        private InventoryData inventoryData;
        
        protected override void Awake()
        {
            uid = UIWindowManager.WindowUid.Inventory;
            if (TableLoaderManager.Instance == null) return;
            tableItem = TableLoaderManager.Instance.TableItem;
            buttonMergeAllItems?.onClick.AddListener(OnClickMergeAllItems);
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            mainCamera = SceneGame.Instance.mainCamera;
            itemManager = SceneGame.Instance.itemManager;
            if (SceneGame.Instance != null && SceneGame.Instance.saveDataManager != null)
            {
                inventoryData = SceneGame.Instance.saveDataManager.Inventory;
            }
        }

        public override bool Show(bool show)
        {
            if (!base.Show(show)) return false;
            uIWindowEquip?.Show(show);
            uiWindowPlayerInfo?.Show(show);
            return true;
        }
        public override void OnShow(bool show)
        {
            if (SceneGame.Instance == null || TableLoaderManager.Instance == null) return;
            if (!show)
            {
                uIWindowItemInfo?.Show(false);
                return;
            }
            LoadIcons();
        }
        /// <summary>
        /// 저장되어있는 아이템 정보로 아이콘 셋팅하기
        /// 인벤토리가 열려있지 않으면 업데이트 하지 않음
        /// </summary>
        public void LoadIcons()
        {
            if (!gameObject.activeSelf) return;
            var datas = SceneGame.Instance.saveDataManager.Inventory.GetAllItemCounts();
            if (datas == null) return;
            foreach (var info in datas)
            {
                int index = info.Key;
                if (index >= icons.Length) continue;
                var icon = icons[index];
                if (icon == null) continue;
                UIIconItem uiIcon = icon.GetComponent<UIIconItem>();
                if (uiIcon == null) continue;
                StructInventoryIcon structInventoryIcon = info.Value;
                int itemUid = structInventoryIcon.ItemUid;
                int itemCount = structInventoryIcon.ItemCount;
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
            // GcLogger.Log("OnEndDragOutWindow");
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(eventData.position.x,
                eventData.position.y, mainCamera.nearClipPlane));
            UIIcon icon = droppedIcon.GetComponent<UIIcon>();
            
            // 맵에 드랍하기
            itemManager.ShowDropItem(worldPosition, icon.uid, icon.count);
            // 윈도우에서 아이콘 정보 지워주기 
            icon.window.DetachIcon(icon.slotIndex);
            
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
            UIWindowManager uiWindowManager = SceneGame.Instance.uIWindowManager;

            UIIconItem droppedUIIcon = droppedIcon.GetComponent<UIIconItem>();
            UIIconItem targetUIIcon = targetIcon.GetComponent<UIIconItem>();
            
            UIWindowManager.WindowUid droppedWindowUid = droppedUIIcon.windowUid;
            UIWindowManager.WindowUid targetWindowUid = targetUIIcon.windowUid;
        
            int dropIconSlotIndex = droppedUIIcon.slotIndex;
            
            // 드래그앤 드랍 한 곳에 아무것도 없을때 
            if (targetUIIcon == null)
            {
                GoBackToSlot(droppedIcon);
                return;
            }
            int targetIconSlotIndex = targetUIIcon.slotIndex;

            // 다른 윈도우에서 인벤토리로 드래그 앤 드랍 했을 때 
            if (droppedWindowUid != targetWindowUid)
            {
                switch (droppedWindowUid)
                {
                    case UIWindowManager.WindowUid.Equip:
                        // 같은 uid 아이템인지 확인
                        if (droppedUIIcon.uid == targetUIIcon.uid)
                        {
                            int dropIndex = droppedUIIcon.index;
                            int dropUid = droppedUIIcon.uid;
                        
                            int targetIndex = targetUIIcon.index;
                            int targetUid = targetUIIcon.uid;
                            // 장비창에 있던것도 빼서 0 을 만듬
                            SceneGame.Instance.saveDataManager.Equip.AddItemToSlot(dropIndex, dropUid, -1);
                            // 장비창에 있던것은 인벤토리에 추가한다 
                            SceneGame.Instance.saveDataManager.Inventory.AddItem(dropUid, 1);
                            LoadIcons();
                            uIWindowEquip?.LoadIcons();
                            GoBackToSlot(droppedIcon);
                        }
                        else if (targetUIIcon.uid <= 0)
                        {
                            // 장비창에 하나 빼고
                            SceneGame.Instance.saveDataManager.Equip.AddItemToSlot(droppedUIIcon.index, droppedUIIcon.uid, -1);
                            // 인벤토리에서 하나 넣기
                            SceneGame.Instance.saveDataManager.Inventory.AddItemToSlot(targetUIIcon.index, droppedUIIcon.uid, 1);
                            LoadIcons();
                            uIWindowEquip?.LoadIcons();
                            GoBackToSlot(droppedIcon);
                        }
                        else if (droppedUIIcon.GetPartsType() == targetUIIcon.GetPartsType())
                        {
                            // 교체 가능한 PartsType이면 교체 진행
                            // 순서 중요
                            int dropIndex = droppedUIIcon.index;
                            int dropUid = droppedUIIcon.uid;
                
                            int targetIndex = targetUIIcon.index;
                            int targetUid = targetUIIcon.uid;
                            // 인벤토리에서 하나 빼고
                            SceneGame.Instance.saveDataManager.Inventory.AddItemToSlot(dropIndex, dropUid, -1);
                            // 장비창에 있던것도 빼서 0 을 만듬
                            SceneGame.Instance.saveDataManager.Equip.AddItemToSlot(targetIndex, targetUid, -1);
                            // 장비창에 있던것은 인벤토리에 추가한다 
                            SceneGame.Instance.saveDataManager.Inventory.AddItem(targetUid, 1);
                            // 장비창에 하나 넣기
                            SceneGame.Instance.saveDataManager.Equip.AddItemToSlot(targetIndex, dropUid, 1);
                            LoadIcons();
                            uIWindowEquip?.LoadIcons();
                            GoBackToSlot(droppedIcon);
                            
                            // uiWindowManager.MoveIcon(droppedUIIcon.window.uid, dropIconSlotIndex, targetUIIcon.window.uid, targetIconSlotIndex);
                            // LoadIcons();
                            // uIWindowEquip?.LoadIcons();
                        }
                        else
                        {
                            // 교체가 불가능하면 원래 자리로 되돌리기
                            SceneGame.Instance.systemMessageManager.ShowMessageWarning("해당 부위에 장착할 수 없는 아이템입니다.");
                            GoBackToSlot(droppedIcon);
                        }
                        break;
                    case UIWindowManager.WindowUid.None:
                    case UIWindowManager.WindowUid.Hud:
                    case UIWindowManager.WindowUid.Inventory:
                    case UIWindowManager.WindowUid.ItemInfo:
                    case UIWindowManager.WindowUid.PlayerInfo:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                if (targetIconSlotIndex < maxCountIcon)
                {
                    // 같은 아이템일때 
                    if (droppedUIIcon.uid == targetUIIcon.uid)
                    {
                        // 중첩 가능한지 체크
                        var info = tableItem.GetDataByUid(targetUIIcon.uid);
                        if (info != null && info.MaxOverlayCount > 1)
                        {
                            var result = inventoryData.MergeItem(dropIconSlotIndex, targetIconSlotIndex);
                            if (!result.IsSuccess())
                            {
                                GcLogger.LogError(result.Message);
                                SceneGame.Instance.systemMessageManager.ShowMessageWarning(result.Message);
                            }
                            LoadIcons();
                            GoBackToSlot(droppedIcon);
                        }
                        else
                        {
                            uiWindowManager.SwitchIcon(droppedUIIcon.window.uid, dropIconSlotIndex, targetUIIcon.window.uid,
                                targetIconSlotIndex);
                        }
                    }
                    else
                    {
                        uiWindowManager.SwitchIcon(droppedUIIcon.window.uid, dropIconSlotIndex, targetUIIcon.window.uid,
                            targetIconSlotIndex);
                    }
                }
                // regist icon 에서 리스트로 드래그 앤 드랍 했을때 원래 자리로 되돌리기
                else
                {
                    GoBackToSlot(droppedIcon);
                }
            }
        }
        protected override void OnSetIcon(int slotIndex, GameObject icon)
        {
            base.OnSetIcon(slotIndex, icon);
            UIIcon uiIcon = icon.GetComponent<UIIcon>();
            if (uiIcon == null) return;
            inventoryData.SetItemCount(slotIndex, uiIcon.uid, uiIcon.count);
        }
        protected override void OnDetachIcon(int slotIndex)
        {
            inventoryData.RemoveItemCount(slotIndex);
        }
        /// <summary>
        /// 모든 아이템 합치기
        /// </summary>
        private void OnClickMergeAllItems()
        {
            inventoryData.MergeAllItems();
            LoadIcons();
        }
    }
}