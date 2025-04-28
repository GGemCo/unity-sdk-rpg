using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo.Scripts
{
    public class UIWindowDialogue : UIWindow
    {
        public Image imageThumbnail;
        public TextMeshProUGUI textName;
        public TextMeshProUGUI textMessage;
        public GameObject prefabButtonChoice;
        public Transform containerButton;
        
        private const int ButtonCount = 3;
        private readonly Dictionary<int, Button> buttonChoices = new Dictionary<int, Button>();
        
        protected override void Awake()
        {
            uid = UIWindowManager.WindowUid.Dialogue;
            base.Awake();
            InitializeButtonChoice();
        }

        protected override void Start()
        {
            base.Start();
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
                GameObject buttonObj = Instantiate(prefabButtonChoice, containerButton);
                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    buttonChoices.TryAdd(i, button);
                    button.gameObject.SetActive(false); // 초기 상태 비활성화
                }
            }
        }
    }
}