using UnityEngine;

namespace GGemCo.Scripts.Configs
{
    public static class ConfigTags
    {
        // 메인 카메라
        public const string MainCamera = "GGemCo_MainCamera";
        // 플레이어
        public const string Player = "GGemCo_Player";
        // 몬스터
        public const string Monster = "GGemCo_Monster";
        // npc
        public const string Npc = "GGemCo_Npc";
        
        [Header("Map")]
        // 맵
        public const string Map = "GGemCo_Map";
        public const string MapObject = "GGemCo_MapObject";
        // 맵에 배치되는 워프
        public const string MapObjectWarp = "GGemCo_MapObjectWarp";
        // 타일 맵이 들어가는 부모 grid object
        public const string GridTileMap = "GGemCo_GridTileMap";
        
        [Header("UI")]
        // 맵 이동시 화면 interaction 을 막는 canvas
        public const string CanvasBlockInteraction = "GGemCo_CanvasBlockInteraction";
        // npc 머리 위에 나오는 퀘스트 버튼
        public const string ButtonNpcQuest = "GGemCo_ButtonNpcQuest";
        // 월드 position 으로 보여지는 UI 용 canvas. 예) 데미지 텍스트, npc 이름 
        public const string CanvasUIByWorld = "GGemCo_CanvasUIByWorld";
        // UI가 포함될 canvas
        public const string CanvasUI = "GGemCo_CanvasUI";
    }
}