using System;
using System.Collections.Generic;
using GGemCo.Scripts;
using UnityEngine;

namespace GGemCo.Editor
{
    [Serializable]
    public class DialogueData
    {
        public List<DialogueNodeData> nodes = new List<DialogueNodeData>();
    }

    /// <summary>
    /// 대사 json에 저장되는 항목
    /// </summary>
    [Serializable]
    public class DialogueNodeData
    {
        public string guid;
        public string title;
        public string dialogueText;
        public Vec2 position;
        public CharacterConstants.Type characterType;
        public int characterUid;
        public float fontSize;
        public string thumbnailImage;
        public List<DialogueOption> options;
    }
    /// <summary>
    /// 대사 노드
    /// </summary>
    [Serializable]
    public class DialogueNode : ScriptableObject
    {
        public string guid;
        public string title = "대화 노드";
        [TextArea(3, 10)]
        public string dialogueText = "대사를 입력하세요";
        public Vector2 position;
        [Tooltip("대화를 하는 캐릭터 타입")] 
        public CharacterConstants.Type characterType;
        [Tooltip("대화를 하는 캐릭터 고유번호")]
        public int characterUid;
        [Tooltip("폰트 사이즈")]
        public float fontSize;
        [Tooltip("썸네일 이미지")]
        public string thumbnailImage;
        public Vector2 cachedSize = Vector2.zero;

        public List<DialogueOption> options = new List<DialogueOption>();
        
        public DialogueNode()
        {
            guid = Guid.NewGuid().ToString();
        }
    }
}