﻿using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGemCo.Scripts
{
    /// <summary>
    /// 스킬 리스트 element
    /// </summary>
    public class UIElementShop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Vector3 iconPosition;
        public TextMeshProUGUI textName;
        public TextMeshProUGUI textPrice;
        public Button buttonBuy;
        
        private UIWindowShop uiWindowShop;
        private StruckTableShop struckTableShop;
        private TableItem tableItem;
        private PlayerData playerData;
        private int slotIndex;
        
        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="puiWindowShop"></param>
        /// <param name="pslotIndex"></param>
        /// <param name="pstruckTableShop"></param>
        public void Initialize(UIWindowShop puiWindowShop, int pslotIndex, StruckTableShop pstruckTableShop)
        {
            playerData = SceneGame.Instance.saveDataManager.Player;
            struckTableShop = pstruckTableShop;
            slotIndex = pslotIndex;
            if (buttonBuy != null)
            {
                buttonBuy.onClick.AddListener(OnClickBuy);
            }

            uiWindowShop = puiWindowShop;
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
                // 구매할 수 있는 최대 수량으로 등록
                int count = (int)playerData.GetPossibleBuyCount(struckTableShop.CurrencyType, struckTableShop.CurrencyValue);
                if (count <= 0)
                {
                    SceneGame.Instance.systemMessageManager.ShowWarningCurrency(struckTableShop.CurrencyType);
                    return;
                }
                var info = tableItem.GetDataByUid(struckTableShop.ItemUid);
                if (info != null && count > info.MaxOverlayCount)
                {
                    count = info.MaxOverlayCount;
                }
                
                uiWindowShop.uIWindowItemBuy?.CopyIconCount(0, slotIndex, struckTableShop.ItemUid, count);
                uiWindowShop.uIWindowItemBuy?.SetPriceInfo(struckTableShop);
            }
            // 한번에 하나만 살 수 있는지
                // 골드가 충분하지 체크
            else
            {
                SceneGame.Instance.BuyItem(struckTableShop.ItemUid, struckTableShop.CurrencyType,
                    struckTableShop.CurrencyValue);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            uiWindowShop.uIWindowItemInfo.SetItemUid(struckTableShop.ItemUid);
            RectTransform itemInfoRect = uiWindowShop.uIWindowItemInfo.GetComponent<RectTransform>();
            itemInfoRect.pivot = new Vector2(0, 1f);
            uiWindowShop.uIWindowItemInfo.transform.position =
                new Vector3(transform.position.x + uiWindowShop.containerIcon.cellSize.x / 2f,
                    transform.position.y + uiWindowShop.containerIcon.cellSize.y / 2f, 0);
            // 화면 밖 체크 & 보정
            MathHelper.ClampToScreen(itemInfoRect);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            uiWindowShop.uIWindowItemInfo.Show(false);
        }

        public Vector3 GetIconPosition() => iconPosition;
    }
}