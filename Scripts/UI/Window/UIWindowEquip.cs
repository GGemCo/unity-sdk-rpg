using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI.Icon;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GGemCo.Scripts.UI.Window
{
    public class UIWindowEquip : UIWindow
    {
        private TableItem tableItem;
        
        // 미리 만들어놓은 slot 이 있을 경우
        public GameObject[] preLoadSlots;
        public delegate void DelegateOnSetIconEquip(int slotIndex, int itemUid, int itemCount);
        public event DelegateOnSetIconEquip OnSetIconEquip;
        public delegate void DelegateOnDetachIconEquip(int slotIndex);
        public event DelegateOnDetachIconEquip OnDetachIconEquip;
        protected override void Awake()
        {
            base.Awake();
            if (TableLoaderManager.Instance == null) return;
            tableItem = TableLoaderManager.Instance.TableItem;
            uid = UIWindowManager.WindowUid.Equip;
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
                if (slotObject == null && containerIcon == null)
                {
                    slotObject = Instantiate(slot, containerIcon.gameObject.transform);
                }
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
            var datas = SceneGame.Instance.saveDataManager.Equip.GetAllItemCounts();
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
            // GcLogger.Log("OnEndDragOutWindow");
            UIIcon icon = droppedIcon.GetComponent<UIIcon>();
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

            if (targetIconSlotIndex < maxCountIcon)
            {
                uiWindowManager.MoveIcon(droppedUIIcon.window.uid, dropIconSlotIndex, targetUIIcon.window.uid, targetIconSlotIndex);
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