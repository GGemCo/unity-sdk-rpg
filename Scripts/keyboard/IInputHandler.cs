﻿namespace GGemCo.Scripts.keyboard
{
    public interface IInputHandler
    {
        int Priority { get; }
        bool HandleInput(); // 처리 여부 반환
    }
}