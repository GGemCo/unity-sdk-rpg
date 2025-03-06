using GGemCo.Scripts.Items;
using GGemCo.Scripts.TableLoader;
using TMPro;
using UnityEngine;

namespace GGemCo.Scripts.TagName
{
    public class TagNameItem : MonoBehaviour
    {
        public TextMeshProUGUI textName;
        
        public float fontSize = 24;
        public Color color = Color.white;
        public Vector3 diffTextPosition;
        
        private Item item;
        private StruckTableItem struckTableItem;

        private void Awake()
        {
        }
        public void Initialize(GameObject itemObject, int itemCount)
        {
            if (itemObject == null || TableLoaderManager.Instance == null) return;
            item = itemObject.GetComponent<Item>();
            struckTableItem = TableLoaderManager.Instance.TableItem.GetDataByUid(item.itemUid);
            textName.text = $"{struckTableItem.Name} ({itemCount})";
            ApplyTextEffect();
        }

        private void ApplyTextEffect()
        {
            // 텍스트 색상 및 효과 설정
            switch (struckTableItem.Type)
            {
                case ItemConstants.Type.Equip:
                case ItemConstants.Type.None:
                case ItemConstants.Type.Consumable:
                default:
                    textName.color = color;
                    textName.fontSize = fontSize;
                    break;
            }
        }
        private void LateUpdate()
        {
            // 아이템 위 월드 좌표 설정
            Vector3 npcNameWorldPosition = item.gameObject.transform.position + diffTextPosition;
            gameObject.transform.position = npcNameWorldPosition;
        }
        /// <summary>
        /// 아이템 비활성에 맞춰서 이름도 비활성화 처리 
        /// </summary>
        private void OnDisable()
        {
            textName.gameObject.SetActive(false);
        }
        /// <summary>
        /// 아이템 활성에 맞춰서 이름도 활성화 처리 
        /// </summary>
        private void OnEnable()
        {
            textName.gameObject.SetActive(true);
        }
    }
}