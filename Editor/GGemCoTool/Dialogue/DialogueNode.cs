using System;
using System.Collections.Generic;
using GGemCo.Scripts;
using UnityEngine;

namespace GGemCo.Editor
{
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
        
        // 대사 텍스트의 연결 대상
        public string nextNodeGuid;
        public Vector2 nodeConnectionPoint;
        
        public List<DialogueOption> options = new List<DialogueOption>();
        
        public DialogueNode()
        {
            guid = Guid.NewGuid().ToString();
        }
    }
}