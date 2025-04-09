using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo.Scripts
{
    /// <summary>
    /// 인벤토리 아이템 나누기
    /// </summary>
    public class UIWindowItemBuy : UIWindow
    {
        [Header("기본오브젝트")]
        // 미리 만들어놓은 slot 이 있을 경우
        public GameObject[] preLoadSlots;
        [Tooltip("아이템 이름")]
        public TextMeshProUGUI textItemName;
        [Tooltip("아이템 개수")]
        public TextMeshProUGUI textItemCount;
        [Tooltip("나누기 슬라이드")]
        public Slider sliderSplit;
        [Tooltip("구매하기 버튼")]
        public Button buttonConfirm;
        [Tooltip("취소 버트")]
        public Button buttonCancel;

        // 내가 가지고 있는 아이템 개수
        private int maxItemCount;
        // 구매 하는 아이템 uid
        private int itemUid;
        // 구매 하는 아이템 개수
        private int buyItemCount;
        // 구매 하는 아이템의 인벤토리 slot index
        private int buyItemIndex;
        // 판매하는 아이템의 shop 테이블 정보
        private StruckTableShop struckTableShop;
        private PlayerData playerData;
        protected override void Awake()
        {
            uid = UIWindowManager.WindowUid.ItemBuy;
            base.Awake();
            buttonConfirm.onClick.AddListener(OnClickConfirm);
            buttonCancel.onClick.AddListener(OnClickCancel);
            sliderSplit.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void Start()
        {
            base.Start();
            playerData = SceneGame.Instance.saveDataManager.Player;
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
        public override void CopyIconCount(int toIndex, int fromIndex, int fromItemUid, int fromItemCount)
        {
            buyItemIndex = fromIndex;
            SetIconCount(0, fromItemUid, fromItemCount);
        }
        protected override void OnSetIcon(int slotIndex, int iconUid, int iconCount, int iconLevel = 0, bool iconLearn = false)
        {
            var info = TableLoaderManager.Instance.TableItem.GetDataByUid(iconUid);
            if (info == null) return;
            itemUid = iconUid;
            textItemName.text = info.Name;
            textItemCount.text = $"{iconCount / iconCount}";
            maxItemCount = iconCount;
            Show(true);
            sliderSplit.value = 0.5f;
            // 강제로 특정 값으로 이벤트 호출 (값은 그대로 유지)
            sliderSplit.onValueChanged.Invoke(sliderSplit.value);
        }

        private void OnValueChanged(float value)
        {
            if (sliderSplit == null) return;
            buyItemCount = (int)(maxItemCount * value);
            if (buyItemCount == 0)
            {
                buyItemCount = 1;
                sliderSplit.value = (float)buyItemCount / maxItemCount;
            }
            textItemCount.text = $"{buyItemCount} / {maxItemCount}";
        }
        /// <summary>
        /// 아이템 나누기
        /// </summary>
        private void OnClickConfirm()
        {
            if (struckTableShop == null) return;
            // 구매 하기
            SceneGame.Instance.BuyItem(struckTableShop.ItemUid, struckTableShop.CurrencyType,
                struckTableShop.CurrencyValue, buyItemCount);

            struckTableShop = null;
            Show(false);
        }

        private void OnClickCancel()
        {
            struckTableShop = null;
            Show(false);
        }

        public void SetPriceInfo(StruckTableShop pstruckTableShop)
        {
            struckTableShop = pstruckTableShop;
        }
    }
}