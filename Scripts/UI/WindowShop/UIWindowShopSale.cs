using PlasticGui;
using R3;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGemCo.Scripts
{
    /// <summary>
    /// 플레이어 스킬
    /// </summary>
    public class UIWindowShopSale : UIWindow
    {
        [Tooltip("판매하기 버튼")] public Button buttonSale;
        [Tooltip("판매 총 금액 골드")] public TextMeshProUGUI textTotalPriceGold;
        [Tooltip("판매 총 금액 실버")] public TextMeshProUGUI textTotalPriceSilver;
        
        private TableShop tableShop;
        private int currentShopUid;
        
        private SceneGame sceneGame;
        private UIWindowItemInfo uiWindowItemInfo;
        private UIWindowInventory uiWindowInventory;

        private readonly BehaviorSubject<int> totalPriceGold = new(0);
        private readonly BehaviorSubject<int> totalPriceSilver = new(0);

        private InventoryData inventoryData;
        private ShopSaleData shopSaleData;
        private TableItem tableItem;
        
        protected override void Awake()
        {
            currentShopUid = 0;
            uid = UIWindowManager.WindowUid.ShopSale;
            if (TableLoaderManager.Instance == null) return;
            tableShop = TableLoaderManager.Instance.TableShop;
            base.Awake();
            buttonSale?.onClick.AddListener(OnClickSale);
            
            totalPriceGold.DistinctUntilChanged()
                .CombineLatest(totalPriceSilver, (_, _) => Unit.Default)
                .Subscribe(_ => SetTotalPriceText())
                .AddTo(this);
        }
        protected override void Start()
        {
            base.Start();
            tableItem = TableLoaderManager.Instance.TableItem;
            
            sceneGame = SceneGame.Instance;
            if (sceneGame != null && sceneGame.saveDataManager != null)
            {
                inventoryData = sceneGame.saveDataManager.Inventory;
                shopSaleData = sceneGame.saveDataManager.ShopSale;
            }
            uiWindowItemInfo = 
                sceneGame.uIWindowManager.GetUIWindowByUid<UIWindowItemInfo>(UIWindowManager.WindowUid
                    .ItemInfo);
            uiWindowInventory = 
                sceneGame.uIWindowManager.GetUIWindowByUid<UIWindowInventory>(UIWindowManager.WindowUid
                    .Inventory);
        }
        public override bool Show(bool show)
        {
            if (!base.Show(show)) return false;
            uiWindowInventory?.Show(show);
            if (!show)
            {
                uiWindowItemInfo?.Show(false);
            }
            else
            {
                Initialize();
            }
            return true;
        }

        private void Initialize()
        {
            totalPriceGold.OnNext(0);
            totalPriceSilver.OnNext(0);
            foreach (var icon in icons)
            {
                UIIcon uiIcon = icon.GetComponent<UIIcon>();
                if (uiIcon == null) continue;
                uiIcon.ClearIconInfos();
            }
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
        /// <summary>
        /// 아이템 팔기
        /// </summary>
        private void OnClickSale()
        {
            // 먼저 재화 처리한다
            sceneGame.saveDataManager.Player.AddCurrency(CurrencyConstants.Type.Gold, totalPriceGold.Value);
            sceneGame.saveDataManager.Player.AddCurrency(CurrencyConstants.Type.Silver, totalPriceSilver.Value);
            foreach (var icon in icons)
            {
                UIIconItem uiIconItem = icon.GetComponent<UIIconItem>();
                if (uiIconItem == null || uiIconItem.uid <= 0 || uiIconItem.GetCount() <= 0) continue;
                DetachIcon(uiIconItem.slotIndex);
                var parentInfo = uiIconItem.GetParentInfo();
                sceneGame.uIWindowManager.RemoveIcon(parentInfo.Item1, parentInfo.Item2);
            }
        }
        protected override void OnSetIcon(int slotIndex, int iconUid, int iconCount, int iconLevel = 0, bool iconLearn = false)
        {
            base.OnSetIcon(slotIndex, iconUid, iconCount, iconLevel, iconLearn);
            var icon = GetIconByIndex(slotIndex);
            if (icon != null)
            {
                icon.SetDrag(false);
            }
            ReCalculateSaleItem();
        }
        /// <summary>
        /// 총 판매 금액 계산하기
        /// </summary>
        private void ReCalculateSaleItem()
        {
            int totalGold = 0;
            int totalSilver = 0;
            foreach (var icon in icons)
            {
                UIIconItem uiIconItem = icon.GetComponent<UIIconItem>();
                if (uiIconItem == null || uiIconItem.uid <= 0 || uiIconItem.GetCount() <= 0) continue;
                CurrencyConstants.Type currencyType = uiIconItem.GetSaleCurrencyType();
                int value = uiIconItem.GetSaleCurrencyValue();
                int count = uiIconItem.GetCount();
                if (currencyType == CurrencyConstants.Type.Gold)
                {
                    totalGold += value * count;
                }
                else if (currencyType == CurrencyConstants.Type.Silver)
                {
                    totalSilver += value * count;
                }
            }
            totalPriceGold.OnNext(totalGold);
            totalPriceSilver.OnNext(totalSilver);
        }

        private void SetTotalPriceText()
        {
            textTotalPriceGold.text = $"{CurrencyConstants.GetNameGold()}: {totalPriceGold.Value}";
            textTotalPriceSilver.text = $"{CurrencyConstants.GetNameSilver()}: {totalPriceSilver.Value}";
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

            // 인벤토리에서 상점으로 드래그 앤 드랍 했을 때만 처리한다 
            if (droppedWindowUid == UIWindowManager.WindowUid.Inventory && targetIconSlotIndex < maxCountIcon)
            {
                // 판매할 수 있는 아이템 인지 체크
                if (droppedUIIcon.IsAntiFlag(ItemConstants.AntiFlag.ShopSale))
                {
                    sceneGame.systemMessageManager.ShowMessageWarning("해당 아이템은 판매할 수 없는 아이템 입니다.");
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

                    result = shopSaleData.AddItem(targetIconSlotIndex, dropIconUid, dropIconCount);
                    targetWindow.SetIcons(result);
                }
            }
            else
            {
                // 판매할 수 있는 아이템 인지 체크
                if (droppedUIIcon.IsAntiFlag(ItemConstants.AntiFlag.ShopSale))
                {
                    sceneGame.systemMessageManager.ShowMessageWarning("해당 아이템은 판매할 수 없는 아이템 입니다.");
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
                                var result = shopSaleData.MergeItem(dropIconSlotIndex, targetIconSlotIndex);
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
        /// <summary>
        /// 아이콘 우클릭했을때 처리 
        /// </summary>
        /// <param name="icon"></param>
        public override void OnRightClick(UIIcon icon)
        {
            if (icon == null) return;
            sceneGame.uIWindowManager.UnRegisterIcon(UIWindowManager.WindowUid.ShopSale, icon.slotIndex, UIWindowManager.WindowUid.Inventory);
        }
        protected override void OnDetachIcon(int slotIndex)
        {
            base.OnDetachIcon(slotIndex);
            ReCalculateSaleItem();
        }
        public override void OnShow(bool show)
        {
            if (sceneGame == null || TableLoaderManager.Instance == null) return;
            if (!show)
            {
                foreach (var icon in icons)
                {
                    UIIcon uiIcon = icon.GetComponent<UIIcon>();
                    if (uiIcon == null || uiIcon.uid <= 0 || uiIcon.GetCount() <= 0) continue;
                    sceneGame.uIWindowManager.UnRegisterIcon(UIWindowManager.WindowUid.ShopSale, uiIcon.slotIndex, UIWindowManager.WindowUid.Inventory);
                }
            }
        }
    }
}