using System.Collections.Generic;
using GGemCo.Scripts.Interaction;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI.WindowShop;
using GGemCo.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo.Scripts.UI.Window
{
    public class UIWindowInteractionDialogue : UIWindow
    {
        public Image imageThumbnail;
        public TextMeshProUGUI textName;
        public TextMeshProUGUI textMessage;
        public GameObject prefabButtonChoice;
        public Transform containerButton;
        public UIWindowShop uiWindowShop;

        // 최대 interaction 버튼 개수
        private const int ButtonCount = 3;
        private readonly Dictionary<int, Button> buttonChoices = new Dictionary<int, Button>();
        private InteractionConstants.Type currentInteractionType;
        
        protected override void Awake()
        {
            uid = UIWindowManager.WindowUid.Dialogue;
            base.Awake();
            InitializeButtonChoice();
        }
        /// <summary>
        /// interaction 버튼 초기화
        /// </summary>
        private void InitializeButtonChoice()
        {
            if (prefabButtonChoice == null)
            {
                GcLogger.LogError("선택 버튼 프리팹이 없습니다.");
                return;
            }
            if (containerButton == null)
            {
                GcLogger.LogError("선택 버튼 container 가 없습니다.");
                return;
            }
            buttonChoices.Clear();

            for (int i = 0; i < ButtonCount; i++)
            {
                int index = i; // Lambda 캡처용
                GameObject buttonObj = Instantiate(prefabButtonChoice, containerButton);
                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    // button.onClick.AddListener(() => OnClickChoice(index));
                    buttonChoices.TryAdd(i, button);
                    button.gameObject.SetActive(false); // 초기 상태 비활성화
                }
            }
        }
        /// <summary>
        /// interaction 정보 셋티
        /// </summary>
        /// <param name="npcData"></param>
        /// <param name="interactionData"></param>
        public void SetInfos(StruckTableNpc npcData, StruckTableInteraction interactionData)
        {
            imageThumbnail.sprite = Resources.Load<Sprite>(npcData.ImageThumbnailPath);
            textName.text = npcData.Name;
            textMessage.text = interactionData.Message;
            currentInteractionType = InteractionConstants.Type.None;
            
            SetupChoiceButton(0, interactionData.Type1, interactionData.Value1);
            SetupChoiceButton(1, interactionData.Type2, interactionData.Value2);
            SetupChoiceButton(2, interactionData.Type3, interactionData.Value3);
        }
        /// <summary>
        /// interaction 버튼 셋팅
        /// </summary>
        /// <param name="index"></param>
        /// <param name="interactionType"></param>
        /// <param name="value"></param>
        private void SetupChoiceButton(int index, InteractionConstants.Type interactionType, int value)
        {
            if (index < 0 || index >= ButtonCount)
                return;

            Button button = buttonChoices.GetValueOrDefault(index);
            if (button == null) return;
            bool isActive = interactionType != InteractionConstants.Type.None;
            button.gameObject.SetActive(isActive);

            if (!isActive) return;
            // 중복 호출을 막기 위해 기존에 연결된 리스너를 모두 지워준다.
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClickChoice(interactionType, value));
            
            TextMeshProUGUI textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = InteractionConstants.GetTypeName(interactionType);
            }
        }
        /// <summary>
        /// interaction 버튼 처리 
        /// </summary>
        /// <param name="interactionType"></param>
        /// <param name="value"></param>
        private void OnClickChoice(InteractionConstants.Type interactionType, int value)
        {
            GcLogger.Log(interactionType);
            if (interactionType == InteractionConstants.Type.None) return;
            if (interactionType == InteractionConstants.Type.Shop)
            {
                uiWindowShop.Show(true);
                uiWindowShop.SetInfoByShopUid(value);
            }
            currentInteractionType = interactionType;

            Show(false);
        }
        /// <summary>
        /// 플레이어가 npc 에서 멀어져서 interaction 이 끝났을때 처리 
        /// </summary>
        public void OnEndInteraction()
        {
            Show(false);
            
            if (currentInteractionType == InteractionConstants.Type.Shop)
            {
                uiWindowShop.Show(false);
            }
        }
    }
}