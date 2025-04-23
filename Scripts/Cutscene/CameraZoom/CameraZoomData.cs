using System;
using UnityEngine;

namespace GGemCo.Scripts
{
    [Serializable]
    public class CameraZoomData
    {
        [Header("줌 인,아웃. orthographicSize 값")]
        public float startSize = 5f;
        public float endSize = 5f;

        public Easing.EaseType easing = Easing.EaseType.Linear;
    }
}