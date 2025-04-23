using System;
using UnityEngine;

namespace GGemCo.Scripts
{
    [Serializable]
    public class CameraMoveData
    {
        [Header("타겟 따라가기")]
        public bool followTarget;
        public string targetName;

        [Header("이동")]
        public Vec2 startPosition;
        public Vec2 endPosition;
        public float duration = 1f;
        
        public bool fadeIn;
        public bool fadeOut;

        public Easing.EaseType easing = Easing.EaseType.Linear;
    }
}