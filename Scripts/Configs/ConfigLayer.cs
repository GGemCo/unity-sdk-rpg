using System;
using System.Collections.Generic;

namespace GGemCo.Scripts.Configs
{
    /// <summary>
    /// Sorting Layer 에 사용되는 Config 값
    /// </summary>
    public static class ConfigLayer
    {
        public enum Keys
        {
            // 타일맵에서 가지 못하는 영역
            TileMapWall,
        }
        
        public static readonly Dictionary<Keys, string> Tags = new Dictionary<Keys, string>
        {
            { Keys.TileMapWall, "GGemCo_TileMapWall" },
        };
        private static string GetTag(Keys key)
        {
            return Tags.TryGetValue(key, out string value) ? value : throw new ArgumentException($"Invalid key: {key}");
        }
        public static string GetTileMapWall()
        {
            return GetTag(Keys.TileMapWall);
        }
        
    }
}