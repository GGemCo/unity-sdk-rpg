using GGemCo.Scripts.Characters.Player;
using GGemCo.Scripts.SaveData;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI.Inventory;
using GGemCo.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGemCo.Scripts.UI.WindowShop
{
    /// <summary>
    /// 스킬 리스트 element
    /// </summary>
    public class UIElementShop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler 
    {
        public Vector3 iconPosition;
        public TextMeshProUGUI textName;
        public TextMeshProUGUI textPrice;
        public Button buttonBuy;
        
        private UIWindowShop uiWindowShop;
        private UIWindowItemInfo uiWindowItemInfo;
        private StruckTableShop struckTableShop;
        private TableItem tableItem;
        private int slotIndex;
        
        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="puiWindowShop"></param>
        /// <param name="pslotIndex"></param>
        /// <param name="pstruckTableShop"></param>
        public void Initialize(UIWindowShop puiWindowShop, int pslotIndex, StruckTableShop pstruckTableShop)
        {
            slotIndex = pslotIndex;
            struckTableShop = pstruckTableShop;
            if (buttonBuy != null)
            {
                buttonBuy.onClick.AddListener(OnClickBuy);
            }

            uiWindowShop = puiWindowShop;
            uiWindowItemInfo =
                SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowItemInfo>(
                    UIWindowManager.WindowUid.ItemInfo);
            tableItem = TableLoaderManager.Instance.TableItem;
            
            UpdateInfos(pstruckTableShop);
        }

        /// <summary>
        /// slotIndex 로 아이템 정보를 가져온다.
        /// </summary>
        public void UpdateInfos(StruckTableShop pstruckTableShop)
        {
            struckTableShop = pstruckTableShop;
            if (struckTableShop == null)
            {
                GcLogger.LogError($"item 테이블에 정보가 없습니다. struckTableItem is null");
                return;
            }

            if (textName != null)
            {
                var info = tableItem.GetDataByUid(struckTableShop.ItemUid);
                if (info == null) return;
                textName.text = info.Name;
            }
            if (textPrice != null) textPrice.text = $"{struckTableShop.CurrencyType} {struckTableShop.CurrencyValue}";
            buttonBuy.gameObject.SetActive(true);
        }
        /// <summary>
        /// 구매하기
        /// </summary>
        private void OnClickBuy()
        {
            // 여러개 살 수 있는지
                // 팝업창 띄어서 개수 정하기
                // 골드가 충분하지 체크
            if (struckTableShop.MaxBuyCount > 1)
            {
                
            }
            // 한번에 하나만 살 수 있는지
                // 골드가 충분하지 체크
            else
            {
                
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            uiWindowItemInfo.SetItemUid(struckTableShop.ItemUid);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            uiWindowItemInfo.Show(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClickBuy();
        }
        public Vector3 GetIconPosition() => iconPosition;
    }
}