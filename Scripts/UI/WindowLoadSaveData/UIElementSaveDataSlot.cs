using System.IO;
using GGemCo.Scripts.SaveData;
using GGemCo.Scripts.Scenes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo.Scripts.UI.WindowLoadSaveData
{
    public class UIElementSaveDataSlot : MonoBehaviour
    {
        public Image imageThumnail;
        public Image iconCheck;
        public TextMeshProUGUI textLevel;
        public TextMeshProUGUI textSaveDate;

        [HideInInspector] public int slotIndex = 0;
        private Button button;
        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(SetElement);
        }
        public void Initialize(SlotMetaInfo slotMetaInfo, ThumbnailController thumbnailController, bool isCheck)
        {
            slotIndex = slotMetaInfo.SlotIndex;
            iconCheck?.gameObject.SetActive(isCheck);
            if (imageThumnail != null)
            {
                string thumbnailPath = thumbnailController.GetThumbnailPath(slotMetaInfo.SlotIndex);
                if (File.Exists(thumbnailPath))
                {
                    byte[] fileData = File.ReadAllBytes(thumbnailPath);
                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(fileData);
                    imageThumnail.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }
            if (textLevel != null) textLevel.text = $"Lv.{slotMetaInfo.Level}";
            if (textSaveDate != null) textSaveDate.text = slotMetaInfo.SaveTime;
            
            gameObject.SetActive(slotMetaInfo.Exists);
        }
        /// <summary>
        /// 현재 element 를 선택하기
        /// </summary>
        private void SetElement()
        {
            SceneIntro.Instance.uIWindowLoadSaveData.SetSelectElement(slotIndex);
        }
        /// <summary>
        /// 체크 아이콘 on/off
        /// </summary>
        /// <param name="isCheck"></param>
        public void SetIconCheck(bool isCheck)
        {
            iconCheck?.gameObject.SetActive(isCheck);
        }

        public void ClearInfo()
        {
            imageThumnail.sprite = null;
            textLevel.text = "빈 슬롯";
            textSaveDate.text = "";
            SetIconCheck(false);
        }
    }
}