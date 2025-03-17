using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI.Icon;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GGemCo.Scripts.UI.Window
{
    public class UIWindowEquip : UIWindow
    {
        // 미리 만들어놓은 slot 이 있을 경우
        public GameObject[] preLoadSlots;
        public UIWindowInventory uiWindowInventory;
        
        public delegate void DelegateOnSetIconEquip(int slotIndex, int itemUid, int itemCount);
        public event DelegateOnSetIconEquip OnSetIconEquip;
        public delegate void DelegateOnDetachIconEquip(int slotIndex);
        public event DelegateOnDetachIconEquip OnDetachIconEquip;
        
        private TableItem tableItem;
        protected override void Awake()
        {
            uid = UIWindowManager.WindowUid.Equip;
            if (TableLoaderManager.Instance == null) return;
            tableItem = TableLoaderManager.Instance.TableItem;
            base.Awake();
        }
        public override void OnShow(bool show)
        {
            if (SceneGame.Instance == null || TableLoaderManager.Instance == null) return;
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
            GameObject slot = AddressablePrefabLoader.Instance.GetPreLoadGamePrefabByName(ConfigAddressables.KeyPrefabSlot);
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
        public void LoadIcons()
        {
            if (!gameObject.activeSelf) return;
            var datas = SceneGame.Instance.saveDataManager.Equip.GetAllItemCounts();
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
                OnSetIcon(index, icon);
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
            UIWindowManager uiWindowManager = SceneGame.Instance.uIWindowManager;

            UIIconItem droppedUIIcon = droppedIcon.GetComponent<UIIconItem>();
            UIIconItem targetUIIcon = targetIcon.GetComponent<UIIconItem>();
        
            int dropIconSlotIndex = droppedUIIcon.slotIndex;
            
            // 드래그앤 드랍 한 곳에 아무것도 없을때 
            if (targetUIIcon == null)
            {
                GoBackToSlot(droppedIcon);
                return;
            }
            int targetIconSlotIndex = targetUIIcon.slotIndex;

            if (targetIconSlotIndex < maxCountIcon)
            {
                // 장착 아이템 인지 체크
                // 착용할 수 있는 부위인지 체크
                if (droppedUIIcon.IsTypeEquip() && droppedUIIcon.IsEquipParts(targetIconSlotIndex))
                {
                    // 장착된 아이템이 없을 때
                    if (targetUIIcon.uid <= 0)
                    {
                        // 인벤토리에서 하나 빼고
                        SceneGame.Instance.saveDataManager.Inventory.AddItemToSlot(droppedUIIcon.index, droppedUIIcon.uid, -1);
                        // 장비창에 하나 넣기
                        SceneGame.Instance.saveDataManager.Equip.AddItemToSlot(targetUIIcon.index, droppedUIIcon.uid, 1);
                        LoadIcons();
                        uiWindowInventory?.LoadIcons();
                        GoBackToSlot(droppedIcon);
                    }
                    else
                    {
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
                        uiWindowInventory?.LoadIcons();
                        GoBackToSlot(droppedIcon);
                    }
                }
                else
                {
                    SceneGame.Instance.systemMessageManager.ShowMessageWarning("해당 슬롯에는 착용할 수 없는 아이템 입니다.");
                    GoBackToSlot(droppedIcon);
                }
            }
            // regist icon 에서 리스트로 드래그 앤 드랍 했을때 원래 자리로 되돌리기
            else
            {
                GoBackToSlot(droppedIcon);
            }
        }
        protected override void OnSetIcon(int slotIndex, GameObject icon)
        {
            base.OnSetIcon(slotIndex, icon);
            UIIcon uiIcon = icon.GetComponent<UIIcon>();
            if (uiIcon == null) return;

            OnSetIconEquip?.Invoke(slotIndex, uiIcon.uid, uiIcon.count);
        }
        protected override void OnDetachIcon(int slotIndex)
        {
            base.OnDetachIcon(slotIndex);
            
            OnDetachIconEquip?.Invoke(slotIndex);
        }
    }
}