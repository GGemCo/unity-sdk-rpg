using TMPro;
using UnityEngine;

namespace GGemCo.Scripts
{
    public class TagNameItem : MonoBehaviour
    {
        [Header("기본오브젝트")]
        [Tooltip("이름을 표시할 UI Text 오브젝트")]
        public TextMeshProUGUI textName;
        
        [Header("기본속성")]
        [Tooltip("폰트 크기")]
        public float fontSize = 24;
        [Tooltip("폰트 색상")]
        public Color color = Color.white;
        [Tooltip("네임 태그 위치. 아이템 이미지 가운데를 기준으로 계산하여 입력.")]
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