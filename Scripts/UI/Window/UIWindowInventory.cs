using GGemCo.Scripts.Items;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI.Icon;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

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
        private GameObject iconItem;
        private TableItem tableItem;
        private Camera mainCamera;
        private ItemManager itemManager;

        protected override void Awake()
        {
            base.Awake();
            if (TableLoaderManager.Instance == null) return;
            tableItem = TableLoaderManager.Instance.TableItem;
        }

        protected override void Start()
        {
            base.Start();
            mainCamera = SceneGame.Instance.mainCamera;
            itemManager = SceneGame.Instance.itemManager;
        }

        public override void OnShow(bool show)
        {
            if (SceneGame.Instance == null || TableLoaderManager.Instance == null) return;
            if (!show) return;
            LoadIcons();
        }
        /// <summary>
        /// 저장되어있는 아이템 정보로 아이콘 셋팅하기
        /// </summary>
        public void LoadIcons()
        {
            var datas = SceneGame.Instance.saveDataManager.Inventory.GetAllItemCounts();
            if (datas == null) return;
            foreach (var info in datas)
            {
                int index = info.Key;
                StructInventoryIcon structInventoryIcon = info.Value;
                int itemUid = structInventoryIcon.ItemUid;
                int itemCount = structInventoryIcon.ItemCount;
                if (itemUid <= 0 || itemCount <= 0) continue;
                var table = tableItem.GetDataByUid(itemUid);
                if (table == null || table.Uid <= 0) continue;
                // GcLogger.Log($"index: {index}, item: {table.Name} , count: {itemCount}");
                var icon = icons[index];
                if (icon == null) continue;
                UIIconItem uiIcon = icon.GetComponent<UIIconItem>();
                if (uiIcon == null) continue;
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
            // GcLogger.Log("uiwindow skill OnEndDrag ");
            GameObject droppedItem = eventData.pointerDrag;
            if (droppedItem == null || droppedItem.GetComponent<UIIcon>() == null) return;

            UIIcon uiIcon = droppedItem.GetComponent<UIIcon>();
            int iconSlotIndex = uiIcon.slotIndex;
            GameObject slot = slots[iconSlotIndex];
            if (slot == null) return;
            // 자신의 위치 그대로 일때
            droppedItem.transform.SetParent(slot.transform);
            droppedItem.transform.position = uiIcon.GetDragOriginalPosition();
            droppedItem.transform.SetSiblingIndex(1);
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
            // 아이콘 원래 자리로 이동시키기
            GameObject targetSlot = icon.window.slots[icon.slotIndex];
            droppedIcon.transform.SetParent(targetSlot.transform);
            droppedIcon.transform.position = originalPosition;
        }
        /// <summary>
        /// 아이콘 위에서 드래그가 끝났을때 처리 
        /// </summary>
        /// <param name="droppedIcon">드랍한 한 아이콘</param>
        /// <param name="targetIcon">드랍되는 곳에 있는 아이콘</param>
        public override void OnEndDragInIcon(GameObject droppedIcon, GameObject targetIcon)
        {
            // GcLogger.Log("skill window. OnEndDragInIcon");
            UIWindowManager uiWindowManager = SceneGame.Instance.uIWindowManager;

            UIIcon droppedUIIcon = droppedIcon.GetComponent<UIIcon>();
            UIIcon targetUIIcon = targetIcon.GetComponent<UIIcon>();
        
            int dropIconSlotIndex = droppedUIIcon.slotIndex;
            
            // 드래그앤 드랍 한 곳에 아무것도 없을때 
            if (targetUIIcon == null)
            {
                GameObject targetSlot = slots[dropIconSlotIndex];
                if (targetSlot == null)
                {
                    GcLogger.LogError("드랍하는 아이콘 slot index 아무것도 없습니다. dropIconSlotIndex:"+dropIconSlotIndex);
                    return;
                }
                droppedUIIcon.transform.SetParent(targetSlot.transform);
                droppedUIIcon.transform.position = droppedUIIcon.GetDragOriginalPosition();
                droppedUIIcon.transform.SetSiblingIndex(1);
                return;
            }
            int targetIconSlotIndex = targetUIIcon.slotIndex;

            if (dropIconSlotIndex < maxCountIcon)
            {
                uiWindowManager.MoveIcon(UIWindowManager.WindowUid.Inventory, dropIconSlotIndex,
                    UIWindowManager.WindowUid.Inventory, targetIconSlotIndex);
            }
            // regist icon 에서 리스트로 드래그 앤 드랍 했을때 원래 자리로 되돌리기
            else
            {
                GameObject targetSlot = slots[dropIconSlotIndex];
                if (targetSlot == null)
                {
                    GcLogger.LogError("don exist dropIconSlotIndex. dropIconSlotIndex:"+dropIconSlotIndex);
                    return;
                }
                droppedUIIcon.transform.SetParent(targetSlot.transform);
                droppedUIIcon.transform.position = droppedUIIcon.GetDragOriginalPosition();
                droppedUIIcon.transform.SetSiblingIndex(1);
            }
        }
    }
}