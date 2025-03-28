using System;
using System.Collections.Generic;
using System.Linq;
using GGemCo.Scripts.Items;
using GGemCo.Scripts.TableLoader;
using TMPro;
using UnityEngine;

namespace GGemCo.Scripts.UI.Inventory
{
    /// <summary>
    /// 아이템 정보
    /// </summary>
    public class UIWindowItemInfo : UIWindow
    {
        private TableItem tableItem;
        [Header("기본정보")]
        [Tooltip("아이템 이름")]
        public TextMeshProUGUI textName;
        [Tooltip("아이템 타입")]
        public TextMeshProUGUI textType;
        [Tooltip("아이템 카테고리")]
        public TextMeshProUGUI textCategory;
        [Tooltip("아이템 서브카테고리")]
        public TextMeshProUGUI textSubCategory;
        
        [Header("메인옵션")]
        [Tooltip("옵션 이름")]
        public TextMeshProUGUI textStatus1;
        private float valueStatus1;
        public TextMeshProUGUI textStatus2;
        private float valueStatus2;
        
        [Header("서브옵션")]
        public TextMeshProUGUI[] textOptions;
        public float[] valueOptions;
        
        private Dictionary<ItemConstants.Category, Action> categoryUIHandlers;
        
        private StruckTableItem currentStruckTableItem;
        private TableStatus tableStatus;
        
        protected override void Awake()
        {
            uid = UIWindowManager.WindowUid.ItemInfo;
            if (TableLoaderManager.Instance == null) return;
            tableItem = TableLoaderManager.Instance.TableItem;
            tableStatus = TableLoaderManager.Instance.TableStatus;
            base.Awake();
            InitializeCategoryUIHandlers();
        }
        private void InitializeCategoryUIHandlers()
        {
            categoryUIHandlers = new Dictionary<ItemConstants.Category, Action>
            {
                { ItemConstants.Category.Weapon, SetWeaponUI },
                { ItemConstants.Category.Armor, SetArmorUI },
                { ItemConstants.Category.Potion, SetPotionUI },
            };
        }

        public void SetItemUid(int itemUid)
        {
            if (itemUid <= 0) return;
            currentStruckTableItem = tableItem.GetDataByUid(itemUid);
            if (currentStruckTableItem is not { Uid: > 0 }) return;
            
            SetName();
            SetType();
            SetCategory();
            SetStatusOptions();
            SetCategoryUI();
            Show(true);
        }
        /// <summary>
        /// 이름 설정하기
        /// </summary>
        private void SetName()
        {
            if (currentStruckTableItem == null) return;
            textName.text = $"이름: {currentStruckTableItem.Name}";
        }
        /// <summary>
        /// 타입 설정하기
        /// </summary>
        private void SetType()
        {
            if (currentStruckTableItem == null) return;
            textType.text = $"타입: {currentStruckTableItem.Type}";
        }
        
        private void SetCategoryUI()
        {
            if (categoryUIHandlers.TryGetValue(currentStruckTableItem.Category, out var handler))
            {
                handler?.Invoke();
            }
            else
            {
                SetDefaultUI(); // 기본 UI 설정
            }
        }

        /// <summary>
        /// 카테고리, 서브 카테고리 설정하기
        /// </summary>
        private void SetCategory()
        {
            if (currentStruckTableItem == null) return;
            textCategory.text = $"카테고리: {currentStruckTableItem.Category}";
            textSubCategory.text = $"서브카테고리: {currentStruckTableItem.SubCategory}";
        }
        private void SetStatusOptions()
        {
            SetTextMeshPro(textStatus1, currentStruckTableItem.StatusID1, currentStruckTableItem.StatusValue1);
            SetTextMeshPro(textStatus2, currentStruckTableItem.StatusID2, currentStruckTableItem.StatusValue2);

            string[] optionTypes = 
            {
                currentStruckTableItem.OptionType1, 
                currentStruckTableItem.OptionType2, 
                currentStruckTableItem.OptionType3, 
                currentStruckTableItem.OptionType4, 
                currentStruckTableItem.OptionType5
            };

            float[] optionValues = 
            {
                currentStruckTableItem.OptionValue1, 
                currentStruckTableItem.OptionValue2, 
                currentStruckTableItem.OptionValue3, 
                currentStruckTableItem.OptionValue4, 
                currentStruckTableItem.OptionValue5
            };

            for (int i = 0; i < textOptions.Length; i++)
            {
                SetTextMeshPro(textOptions[i], optionTypes[i], optionValues[i]);
                valueOptions[i] = optionValues[i];
            }
        }

        private void SetTextMeshPro(TextMeshProUGUI textMesh, string statusId, float value)
        {
            string statusName = GetStatusName(statusId);
            if (string.IsNullOrEmpty(statusName))
            {
                textMesh.gameObject.SetActive(false);
                return;
            }

            string valueText = GetValueText(statusId, value);
            textMesh.gameObject.SetActive(true);
            textMesh.text = $"{statusName}: {valueText}";
        }

        private string GetValueText(string statusId, float value)
        {
            string valueText = $"{value}";
            foreach (var suffix in ItemConstants.StatusSuffixFormats.Keys)
            {
                if (statusId.EndsWith(suffix))
                {
                    valueText = string.Format(ItemConstants.StatusSuffixFormats[suffix], value);
                    break; // 첫 번째로 매칭된 값만 적용
                }
            }

            return valueText;
        }

        private string GetStatusName(string statusId)
        {
            if (string.IsNullOrEmpty(statusId)) return "";
            string cleanedId = ItemConstants.StatusSuffixFormats.Aggregate(statusId, (current, suffix) => current.Replace(suffix.Key, ""));
            var info = tableStatus.GetDataById(cleanedId);
            return info?.Name ?? "";
        }
        // 카테고리별 UI 설정 함수
        private void SetWeaponUI()
        {
        }

        private void SetArmorUI()
        {
        }

        private void SetPotionUI()
        {
            textStatus1.gameObject.SetActive(true);
            textStatus1.text = $"회복량 : {currentStruckTableItem.StatusValue1}";
            if (currentStruckTableItem.SubCategory == ItemConstants.SubCategory.IncreaseMoveSpeed ||
                currentStruckTableItem.SubCategory == ItemConstants.SubCategory.IncreaseAttackSpeed)
            {
                string statusName = GetStatusName(currentStruckTableItem.StatusID1);
                string valueText = GetValueText(currentStruckTableItem.StatusID1, currentStruckTableItem.StatusValue1);
                textStatus1.text = $"{statusName} : {valueText}";
            }
        }

        private void SetDefaultUI()
        {
        }
    }
}