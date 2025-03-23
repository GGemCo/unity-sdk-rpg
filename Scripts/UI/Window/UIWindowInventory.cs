using System;
using GGemCo.Scripts.Characters.Player;
using GGemCo.Scripts.Items;
using GGemCo.Scripts.SaveData;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.SystemMessage;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI.Icon;
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
        // [Header("아이템 나누기 버튼")]
        // public Button buttonSplitItem;
        
        private GameObject iconItem;
        private TableItem tableItem;
        private Camera mainCamera;
        private ItemManager itemManager;
        private InventoryData inventoryData;
        private EquipData equipData;

        protected override void Awake()
        {
            uid = UIWindowManager.WindowUid.Inventory;
            if (TableLoaderManager.Instance == null) return;
            tableItem = TableLoaderManager.Instance.TableItem;
            buttonMergeAllItems?.onClick.AddListener(OnClickMergeAllItems);
            // buttonSplitItem?.onClick.AddListener(OnClickSplitItem);
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
                equipData = SceneGame.Instance.saveDataManager.Equip;
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
            for (int index = 0; index < maxCountIcon; index++)
            {
                if (index >= icons.Length) continue;
                var icon = icons[index];
                if (icon == null) continue;
                UIIconItem uiIcon = icon.GetComponent<UIIconItem>();
                if (uiIcon == null) continue;
                if (!datas.ContainsKey(index))
                {
                    uiIcon.ClearIconInfos();
                    continue;
                }
                var info = datas[index];
                StructInventoryIcon structInventoryIcon = info;
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
            UIIconItem droppedUIIcon = droppedIcon.GetComponent<UIIconItem>();
            UIWindow droppedWindow = droppedUIIcon.window;
            UIWindowManager.WindowUid droppedWindowUid = droppedUIIcon.windowUid;
            int dropIconSlotIndex = droppedUIIcon.slotIndex;
            int dropIconUid = droppedUIIcon.uid;
            int dropIconCount = droppedUIIcon.count;
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
            int targetIconCount = targetUIIcon.count;

            // 다른 윈도우에서 인벤토리로 드래그 앤 드랍 했을 때 
            if (droppedWindowUid != targetWindowUid)
            {
                switch (droppedWindowUid)
                {
                    case UIWindowManager.WindowUid.Equip:
                        // 같은 uid 아이템인지 확인
                        if (droppedUIIcon.uid == targetUIIcon.uid || targetUIIcon.uid <= 0)
                        {
                            var result = equipData.MinusItem(dropIconSlotIndex, dropIconUid, 1);
                            droppedWindow.SetIcons(result);
                            
                            result = inventoryData.AddItem(targetIconSlotIndex, dropIconUid, 1);
                            targetWindow.SetIcons(result);
                        }
                        else if (droppedUIIcon.GetPartsType() == targetUIIcon.GetPartsType())
                        {
                            // 교체 가능한 PartsType이면 교체 진행
                            // 순서 중요
                            // 인벤토리에서 하나 빼고
                            var result = inventoryData.MinusItem(targetIconSlotIndex, targetIconUid, 1);
                            targetWindow.SetIcons(result);
                            
                            // 장비창에 있던것도 빼서 0 을 만듬
                            result = equipData.MinusItem(dropIconSlotIndex, dropIconUid, 1);
                            droppedWindow.SetIcons(result);
                            
                            // 장비창에 있던것은 인벤토리에 추가한다 
                            result = inventoryData.AddItem(targetIconSlotIndex, dropIconUid, 1);
                            targetWindow.SetIcons(result);
                            // 장비창에 하나 넣기
                            result = equipData.AddItem(dropIconSlotIndex, targetIconUid, 1);
                            droppedWindow.SetIcons(result);
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
                    if (dropIconUid == targetIconUid)
                    {
                        // 중첩 가능한지 체크
                        var info = tableItem.GetDataByUid(targetUIIcon.uid);
                        if (info is { MaxOverlayCount: > 1 })
                        {
                            var result = inventoryData.MergeItem(dropIconSlotIndex, targetIconSlotIndex);
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
            GoBackToSlot(droppedIcon);
        }
        protected override void OnSetIcon(int slotIndex, int iconUid, int iconCount)
        {
            base.OnSetIcon(slotIndex, iconUid, iconCount);
            UIIcon uiIcon = GetIconByIndex(slotIndex);
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
        /// <summary>
        /// 아이템 나누기
        /// </summary>
        private void OnClickSplitItem()
        {
            var icon = GetSelectedIcon();
            if (icon == null || icon.uid <= 0)
            {
                SceneGame.Instance.popupManager.ShowPopupError("나누기를 할 아이템을 선택해주세요.");
                return;
            }

            if (icon.count <= 1)
            {
                SceneGame.Instance.popupManager.ShowPopupError("아이템 개수가 2개 이상일때만 나눌 수 있습니다.");
                return;
            }
            // 팝업창 띄우기
            var window =
                SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowItemSplit>(UIWindowManager.WindowUid
                    .ItemSplit);
            if (window == null) return;
            window.CopyIconCount(0, icon.slotIndex, icon.uid, icon.count);
        }
        /// <summary>
        /// 아이콘 우클릭했을때 처리 
        /// </summary>
        /// <param name="icon"></param>
        public override void OnRightClick(UIIcon icon)
        {
            if (icon == null) return;
            // 장비일때
            if (icon.IsEquipType())
            {
                var partSlotIndex = (int)icon.GetPartsType();
                SceneGame.Instance.uIWindowManager.MoveIcon(uid, icon.index, UIWindowManager.WindowUid.Equip, 1, partSlotIndex);
            }
            // hp 물약일 때 
            else if (icon.IsHpPotionType())
            {
                if (icon.uid <= 0 || icon.count <= 0)
                {
                    SceneGame.Instance.popupManager.ShowPopupError("사용할 수 있는 아이템 개수가 없습니다.");
                    return;
                }
                if (SceneGame.Instance.player.GetComponent<Player>().IsMaxHp())
                {
                    SceneGame.Instance.systemMessageManager.ShowMessageWarning("현재 생명력이 가득하여 사용할 수 없습니다.");
                    return;
                }
                int value = icon.GetStatusValue1();
                SetIconCount(icon.index, icon.uid, icon.count - 1);
                SceneGame.Instance.player.GetComponent<Player>().AddHp(value);
            }
            // mp 물약일 때 
            else if (icon.IsMpPotionType())
            {
                if (icon.uid <= 0 || icon.count <= 0)
                {
                    SceneGame.Instance.popupManager.ShowPopupError("사용할 수 있는 아이템 개수가 없습니다.");
                    return;
                }
                if (SceneGame.Instance.player.GetComponent<Player>().IsMaxMp())
                {
                    SceneGame.Instance.systemMessageManager.ShowMessageWarning("현재 마력이 가득하여 사용할 수 없습니다.");
                    return;
                }
                SetIconCount(icon.index, icon.uid, icon.count - 1);
                int value = icon.GetStatusValue1();
                SceneGame.Instance.player.GetComponent<Player>().AddMp(value);
            }
            // 이동속도 물약일 때 
            else if (icon.IsIncreaseMoveSpeedPotionType())
            {
                if (icon.uid <= 0 || icon.count <= 0)
                {
                    SceneGame.Instance.popupManager.ShowPopupError("사용할 수 있는 아이템 개수가 없습니다.");
                    return;
                }
                if (SceneGame.Instance.player.GetComponent<Player>().IsMaxMp())
                {
                    SceneGame.Instance.systemMessageManager.ShowMessageWarning("현재 마력이 가득하여 사용할 수 없습니다.");
                    return;
                }
            }
        }
        /// <summary>
        /// index 가 없을때는, 같은 uid 는 중첩 가능여부를 확인하고 합치고, 나머지는 추가
        /// </summary>
        /// <param name="iconUid"></param>
        /// <param name="iconCount"></param>
        public override void SetIconCount(int iconUid, int iconCount)
        {
            ResultCommon result = inventoryData.AddItem(iconUid, iconCount);
            SetIcons(result);
        }
        /// <summary>
        /// 아이템 나누기 단축키 : shift + 좌클릭 적용 
        /// </summary>
        /// <param name="index"></param>
        public override void SetSelectedIcon(int index)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                UIIcon icon = GetIconByIndex(index);
                if (icon == null || icon.uid <= 0)
                {
                    SceneGame.Instance.popupManager.ShowPopupError("나누기를 할 아이템을 선택해주세요.");
                    return;
                }

                if (icon.count <= 1)
                {
                    SceneGame.Instance.popupManager.ShowPopupError("아이템 개수가 2개 이상일때만 나눌 수 있습니다.");
                    return;
                }
                // 팝업창 띄우기
                var window =
                    SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowItemSplit>(UIWindowManager.WindowUid
                        .ItemSplit);
                if (window == null) return;
                window.CopyIconCount(0, icon.slotIndex, icon.uid, icon.count);
            }
        }
    }
}