using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo.Scripts
{
    public class UIWindowDialogue : UIWindow
    {
        [Header("대사속성")]
        [Tooltip("말하는 캐릭터 썸네일")]
        public Image imageThumbnail;
        [Tooltip("말하는 캐릭터 이름")]
        public TextMeshProUGUI textName;
        [Tooltip("대사")]
        public TextMeshProUGUI textMessage;
        [Tooltip("선택지 버튼 프리팹")]
        public GameObject prefabButtonAnswer;
        [Tooltip("선택지 버튼이 들어가는 Panel")]
        public Transform containerAnswer;
        [Tooltip("한번에 보여줄 대사 라인 수")]
        public int maxLineCount = 3;

        [Header("버튼")]
        [Tooltip("다음 대사 보기")]
        public Button buttonNextMessage;
        public int paddingWidth = 20;
        
        private int index;
        private List<string> messages;
        private Dictionary<string, DialogueNodeData> dialogueNodeDatas;
        
        private int currentDialogueUid;
        private DialogueNodeData currentDialogue;
        
        private const int ButtonCount = 3;
        private readonly Dictionary<int, Button> buttonChoices = new Dictionary<int, Button>();
        
        private SystemMessageManager systemMessageManager;
        
        protected override void Awake()
        {
            uid = UIWindowManager.WindowUid.Dialogue;
            base.Awake();
            Initialize();
            InitializeButtonChoice();
        }
        private void Initialize()
        {
            if (buttonNextMessage != null)
            {
                buttonNextMessage.onClick.AddListener(OnClickNext);
            }
            messages = new List<string>();
            dialogueNodeDatas = new Dictionary<string, DialogueNodeData>();
        }

        protected override void Start()
        {
            base.Start();
            systemMessageManager = SceneGame.Instance.systemMessageManager;
        }

        /// <summary>
        /// interaction 버튼 초기화
        /// </summary>
        private void InitializeButtonChoice()
        {
            if (prefabButtonAnswer == null)
            {
                GcLogger.LogError("선택 버튼 프리팹이 없습니다.");
                return;
            }
            if (containerAnswer == null)
            {
                GcLogger.LogError("선택 버튼 container 가 없습니다.");
                return;
            }
            buttonChoices.Clear();

            for (int i = 0; i < ButtonCount; i++)
            {
                GameObject buttonObj = Instantiate(prefabButtonAnswer, containerAnswer);
                Button button = buttonObj.GetComponent<Button>();
                if (button == null) continue;
                buttonChoices.TryAdd(i, button);
                var buttonIndex = i;
                button.onClick.AddListener(() => OnClickAnswer(buttonIndex));
                button.gameObject.SetActive(false); // 초기 상태 비활성화
            }
        }

        private void OnClickAnswer(int buttonIndex)
        {
            if (buttonIndex > currentDialogue.options.Count) return;
            DialogueOption dialogueOption = currentDialogue.options[buttonIndex];
            if (dialogueOption == null) return;
            HideChoiceButton();
            ProcessNextDialogue(dialogueOption.nextNodeGuid);
        }

        public void LoadDialogue(int dialogueUid)
        {
            if (dialogueUid <= 0) return;
            var info = TableLoaderManager.Instance.TableDialogue.GetDataByUid(dialogueUid);
            if (info == null) return;

            currentDialogueUid = dialogueUid;
            string fileName = info.FileName;
            string jsonFilePath = $"Dialogue/{fileName}";
            try
            {
                TextAsset textFile = Resources.Load<TextAsset>($"{jsonFilePath}");
                if (textFile != null)
                {
                    string content = textFile.text;
                    if (string.IsNullOrEmpty(content)) return;
                    DialogueData data = JsonConvert.DeserializeObject<DialogueData>(content);
                    SetDialogue(data);
                }
                else
                {
                    GcLogger.LogError("파일이 없습니다. path: " + jsonFilePath);
                }
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"json 파일을 읽어오는데 오류가 발생하였습니다. path: {jsonFilePath}, error message: {ex.Message}");
            }
        }
        
        private void ResetDialogue()
        {
            messages.Clear();
            dialogueNodeDatas.Clear();
            currentDialogueUid = 0;
            currentDialogue = null;
            index = 0;
            HideChoiceButton();
        }

        private void HideChoiceButton()
        {
            containerAnswer?.gameObject.SetActive(false);
            for (int i = 0; i < ButtonCount; i++)
            {
                buttonChoices.GetValueOrDefault(i)?.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 일반 대화 시작
        /// </summary>
        private void SetDialogue(DialogueData data)
        {
            if (data == null) return;

            ResetDialogue();
            foreach (var nodeData in data.nodes)
            {
                dialogueNodeDatas.TryAdd(nodeData.guid, nodeData);
            }

            if (!gameObject.activeSelf)
            {
                Show(true);
            }
            
            index = 0;
            DialogueNodeData dialogue = data.nodes[0];

            ProcessNextDialogue(dialogue.guid);
        }
        /// <summary>
        /// 다음 대사 처리
        /// </summary>
        private void ProcessNextDialogue(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                // FgLogger.Log("일반 대화 종료");
                EndDialogue();
            }
            else
            {
                index = 0;
                DialogueNodeData dialogue = dialogueNodeDatas.GetValueOrDefault(guid);
                currentDialogue = dialogue;
                // SetCharacterName(dialogue);
                // SetCharacterThumnail(dialogue);
                messages = SplitMessage(dialogue.dialogueText);
                DisplayNextMessage();
            }
        }

        private void SetChoiceButton()
        {
            if (currentDialogue == null) return;

            float maxWidth = 0f;

            // 1. 유효한 옵션과 버튼들을 모아서 처리
            var validOptions = currentDialogue.options
                .Select((option, buttonIndex) => new { option, button = buttonChoices.GetValueOrDefault(buttonIndex) })
                .Where(x => x.option != null && x.button != null)
                .ToList();

            if (validOptions.Count <= 0) return;
            
            containerAnswer?.gameObject.SetActive(true);
            // 2. 버튼 활성화 및 최대 너비 계산
            foreach (var entry in validOptions)
            {
                entry.button.gameObject.SetActive(true);
                float width = entry.button.GetComponent<UIButtonAnswer>().SetButtonTitle(entry.option.optionText);
                if (width > maxWidth)
                {
                    maxWidth = width;
                }
            }

            // 3. 모든 버튼에 동일한 너비 적용
            float targetWidth = maxWidth + paddingWidth;
            foreach (var entry in validOptions)
            {
                entry.button.GetComponent<UIButtonAnswer>().ChangeWidth(targetWidth);
            }
        }
        /// <summary>
        /// 메시지 표시
        /// </summary>
        private void DisplayNextMessage()
        {
            if (index >= messages.Count)
            {
                // 선택지가 있으면 넘어가지 않는다
                if (currentDialogue.options.Count > 0)
                {
                    systemMessageManager.ShowMessageWarning("선택지를 선택해주세요.");
                    return;
                }
                ProcessNextDialogue(currentDialogue.nextNodeGuid);
                return;
            }
            textMessage.text = messages[index];
            // DialogueNodeData의 대사가 마지막이고 선택지가 있으면 
            if (index == messages.Count - 1 && currentDialogue.options.Count > 0)
            {
                SetChoiceButton();
            }
            index++;
        }

        private List<string> SplitMessage(string message)
        {
            var result = new List<string>();
            string[] lines = message.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i += maxLineCount)
            {
                string pair = lines[i];
                if (i + 1 < lines.Length)
                {
                    pair += "\n" + lines[i + 1];
                }
                result.Add(pair);
            }

            return result;
        }

        private void OnClickNext()
        {
            DisplayNextMessage();
        }
        /// <summary>
        /// 일반 대화 도중 종료
        /// </summary>
        public void OnClickCancel()
        {
            ResetDialogue();
            gameObject.SetActive(false);
        }
        /// <summary>
        /// 일반 대화 종료
        /// </summary>
        private void EndDialogue()
        {
            ResetDialogue();
            gameObject.SetActive(false);
        }
    }
}