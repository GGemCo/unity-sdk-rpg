using GGemCo.Scripts.TableLoader;
using TMPro;
using UnityEngine;

namespace GGemCo.Scripts.UI.Window
{
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
        public TextMeshProUGUI textOption1;
        // 옵션 값
        private float valueOption1;
        
        private StruckTableItem currentStruckTableItem;
        
        protected override void Awake()
        {
            base.Awake();
            if (TableLoaderManager.Instance == null) return;
            tableItem = TableLoaderManager.Instance.TableItem;
        }

        public void SetItemUid(int itemUid)
        {
            if (itemUid <= 0) return;
            var info = tableItem.GetDataByUid(itemUid);
            if (info == null || info.Uid <= 0) return;
            currentStruckTableItem = info;
            SetName();
            SetType();
            SetCategory();
            SetOption();
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
        /// <summary>
        /// 카테고리, 서브 카테고리 설정하기
        /// </summary>
        private void SetCategory()
        {
            if (currentStruckTableItem == null) return;
            textCategory.text = $"카테고리: {currentStruckTableItem.Category}";
            textSubCategory.text = $"서브카테고리: {currentStruckTableItem.SubCategory}";
        }
        /// <summary>
        /// 옵션 설정하기
        /// </summary>
        private void SetOption()
        {
            if (currentStruckTableItem == null) return;
            textOption1.text = $"{currentStruckTableItem.OptionType1}: {currentStruckTableItem.OptionValue1}";
            valueOption1 = currentStruckTableItem.OptionValue1;
        }
    }
}