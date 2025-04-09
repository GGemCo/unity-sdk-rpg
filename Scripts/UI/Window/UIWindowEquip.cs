using UnityEngine;
using UnityEngine.EventSystems;

namespace GGemCo.Scripts
{
    /// <summary>
    /// 플레이어 장비
    /// </summary>
    public class UIWindowEquip : UIWindow
    {
        // 미리 만들어놓은 slot 이 있을 경우
        public GameObject[] preLoadSlots;
        
        private TableItem tableItem;
        private InventoryData inventoryData;
        private EquipData equipData;
        private SceneGame sceneGame;
        
        protected override void Awake()
        {
            // uid 를 먼저 지정해야 한다.
            uid = UIWindowManager.WindowUid.Equip;
            if (TableLoaderManager.Instance == null) return;
            tableItem = TableLoaderManager.Instance.TableItem;
            base.Awake();
        }
        protected override void Start()
        {
            base.Start();
            sceneGame = SceneGame.Instance;
            if (sceneGame != null && sceneGame.saveDataManager != null)
            {
                equipData = sceneGame.saveDataManager.Equip;
                inventoryData = sceneGame.saveDataManager.Inventory;
            }
        }
        public override void OnShow(bool show)
        {
            if (sceneGame == null || TableLoaderManager.Instance == null) return;
            if (!show) return;
            LoadIcons();
        }
        /// <summary>
        /// 특정 개수만큼 풀을 확장하여 아이템을 추가 생성.
        /// </summary>
        protected override void ExpandPool(int amount)
        {
            if (AddressableSettingsLoader.Instance == null) return;
            if (amount <= 0) return;
            GameObject iconItem = AddressablePrefabLoader.Instance.GetPreLoadGamePrefabByName(ConfigAddressables.KeyPrefabIconItem);
            if (iconItem == null) return;
            for (int i = 0; i < amount; i++)
            {
                GameObject slotObject = preLoadSlots[i];
                if (slotObject == null) continue;
                
                UISlot uiSlot = slotObject.GetComponent<UISlot>();
                if (uiSlot == null) continue;
                uiSlot.Initialize(this, uid, i, slotSize);
                slots[i] = slotObject;
                
                GameObject icon = Instantiate(iconItem, slotObject.transform);
                UIIcon uiIcon = icon.GetComponent<UIIcon>();
                if (uiIcon == null) continue;
                uiIcon.Initialize(this, uid, i, i, iconSize, slotSize);
                icons[i] = icon;
            }
            // GcLogger.Log($"풀 확장: {amount}개 아이템 추가 (총 {poolDropItem.Count}개)");
        }
        /// <summary>
        /// 저장되어있는 아이템 정보로 아이콘 셋팅하기
        /// </summary>
        private void LoadIcons()
        {
            if (!gameObject.activeSelf) return;
            var datas = sceneGame.saveDataManager.Equip.GetAllItemCounts();
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

            if (targetIconSlotIndex < maxCountIcon)
            {
                // 장착 아이템 인지 체크
                // 착용할 수 있는 부위인지 체크
                if (droppedUIIcon.IsTypeEquip() && droppedUIIcon.IsEquipParts(targetIconSlotIndex))
                {
                    var result = inventoryData.MinusItem(dropIconSlotIndex, dropIconUid, 1);
                    droppedWindow.SetIcons(result);
                        
                    // 장착된 아이템이 있을 때
                    if (targetIconUid > 0)
                    {
                        result = equipData.MinusItem(targetIconSlotIndex, targetIconUid, 1);
                        targetWindow.SetIcons(result);
                        
                        result = inventoryData.AddItem(dropIconSlotIndex, targetIconUid, 1);
                        droppedWindow.SetIcons(result);
                    }
                    result = equipData.AddItem(targetIconSlotIndex, dropIconUid, 1);
                    targetWindow.SetIcons(result);
                }
                else
                {
                    sceneGame.systemMessageManager.ShowMessageWarning("해당 슬롯에는 착용할 수 없는 아이템 입니다.");
                }
            }
            GoBackToSlot(droppedIcon);
        }
        protected override void OnSetIcon(int slotIndex, int iconUid, int iconCount, int iconLevel = 0, bool iconLearn = false)
        {
            base.OnSetIcon(slotIndex, iconUid, iconCount, iconLevel, iconLearn);
            UIIcon uiIcon = GetIconByIndex(slotIndex);
            if (uiIcon == null) return;
         
            sceneGame.player.GetComponent<Player>().EquipItem(slotIndex, iconUid, iconCount);
            equipData.SetItemCount(slotIndex, iconUid, iconCount);
        }

        protected override void OnDetachIcon(int slotIndex)
        {
            base.OnDetachIcon(slotIndex);
            UIIcon uiIcon = GetIconByIndex(slotIndex);
            if (uiIcon == null) return;
         
            sceneGame.player.GetComponent<Player>().UnEquipItem(slotIndex);
            equipData.RemoveItemCount(slotIndex);
        }
        /// <summary>
        /// 아이콘 우클릭했을때 처리 
        /// </summary>
        /// <param name="icon"></param>
        public override void OnRightClick(UIIcon icon)
        {
            if (icon == null) return;
            SceneGame.Instance.uIWindowManager.MoveIcon(UIWindowManager.WindowUid.Equip, icon.index, UIWindowManager.WindowUid.Inventory, 1);
        }
    }
}