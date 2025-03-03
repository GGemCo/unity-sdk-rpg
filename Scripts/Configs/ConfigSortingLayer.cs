using System;
using System.Collections.Generic;

namespace GGemCo.Scripts.Configs
{
    /// <summary>
    /// Sorting Layer 에 사용되는 Config 값
    /// </summary>
    public static class ConfigSortingLayer
    {
        public enum Keys
        {
            // 맵 터레인
            MapTerrain,
            // 맵 오브젝트
            MapObject,
            // 캐릭터
            Character,
            // 캐릭터 위에 
            CharacterTop,
            // UI 
            UI,
        }

        public static readonly Dictionary<Keys, string> Tags = new Dictionary<Keys, string>
        {
            { Keys.MapTerrain, "GGemCo_MapTerrain" },
            { Keys.MapObject, "GGemCo_MapObject" },
            { Keys.Character, "GGemCo_Character" },
            { Keys.CharacterTop, "GGemCo_CharacterTop" },
            { Keys.UI, "GGemCo_UI" },
        };
        private static string GetTag(Keys key)
        {
            return Tags.TryGetValue(key, out string value) ? value : throw new ArgumentException($"Invalid key: {key}");
        }
        public static string GetMapTerrain()
        {
            return GetTag(Keys.MapTerrain);
        }
        public static string GetMapObject()
        {
            return GetTag(Keys.MapObject);
        }
        public static string GetCharacter()
        {
            return GetTag(Keys.Character);
        }
        public static string GetCharacterTop()
        {
            return GetTag(Keys.CharacterTop);
        }
        public static string GetUI()
        {
            return GetTag(Keys.UI);
        }
        
    }
}