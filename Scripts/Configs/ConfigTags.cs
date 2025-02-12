using System;
using System.Collections.Generic;

namespace GGemCo.Scripts.Configs
{
    public static class ConfigTags
    {
        public enum Keys
        {
            // 메인 카메라
            MainCamera,
            // 플레이어
            Player,
            // 몬스터
            Monster,
            Npc,
            Map,
            // 맵에 배치되는 오브젝트
            MapObject,
            // 맵에 배치되는 워프
            MapObjectWarp,
            // 타일 맵이 들어가는 부모 grid object
            GridTileMap,
            // 맵 이동시 화면 interaction 을 막는 canvas
            CanvasBlockInteraction,
            // npc 머리 위에 나오는 퀘스트 버튼
            ButtonNpcQuest,
            // 월드 position 으로 보여지는 UI 용 canvas. 예) 데미지 텍스트, npc 이름 
            CanvasUIByWorld,
            // UI가 포함될 canvas
            CanvasUI,
        }

        public static readonly Dictionary<Keys, string> Tags = new Dictionary<Keys, string>
        {
            { Keys.MainCamera, "GGemCo_MainCamera" },
            { Keys.Player, "GGemCo_Player" },
            { Keys.Monster, "GGemCo_Monster" },
            { Keys.Npc, "GGemCo_Npc" },
            { Keys.Map, "GGemCo_Map" },
            { Keys.MapObject, "GGemCo_MapObject" },
            { Keys.MapObjectWarp, "GGemCo_MapObjectWarp" },
            { Keys.GridTileMap, "GGemCo_GridTileMap" },
            { Keys.CanvasBlockInteraction, "GGemCo_CanvasBlockInteraction" },
            { Keys.ButtonNpcQuest, "GGemCo_ButtonNpcQuest" },
            { Keys.CanvasUIByWorld, "GGemCo_CanvasUIByWorld" },
            { Keys.CanvasUI, "GGemCo_CanvasUI" },
        };

        static string GetTag(Keys key)
        {
            return Tags.TryGetValue(key, out string value) ? value : throw new ArgumentException($"Invalid key: {key}");
        }

        public static string GetMainCamera()
        {
            return GetTag(Keys.MainCamera);
        }
        public static string GetMonster()
        {
            return GetTag(Keys.Monster);
        }
        public static string GetGridTileMap()
        {
            return GetTag(Keys.GridTileMap);
        }
        public static string GetMap()
        {
            return GetTag(Keys.Map);
        }
        public static string GetNpc()
        {
            return GetTag(Keys.Npc);
        }
        public static string GetMapObject()
        {
            return GetTag(Keys.MapObject);
        }
        public static string GetMapObjectWarp()
        {
            return GetTag(Keys.MapObjectWarp);
        }
        public static string GetPlayer()
        {
            return GetTag(Keys.Player);
        }
        public static string GetCanvasUIByWorld()
        {
            return GetTag(Keys.CanvasUIByWorld);
        }
        public static string GetCanvasBlockInteraction()
        {
            return GetTag(Keys.CanvasBlockInteraction);
        }
        public static string GetButtonNpcQuest()
        {
            return GetTag(Keys.ButtonNpcQuest);
        }
        public static string GetCanvasUI()
        {
            return GetTag(Keys.CanvasUI);
        }
    }
}