﻿namespace GGemCo.Scripts.Configs
{
    public static class ConfigAddressableKeys
    {
        // 기본 설정 scriptable object
        public const string Settings = "GGemCo_Settings";
        // 몬스터 데미지 표시 text object
        public const string TextFloatingDamage = "GGemCo_TextFloatingDamage";
        // 드랍 아이템 이미지
        public const string SpriteDropItem = "GGemCo_SpriteDropItem";

        public static readonly (string path, string key)[] AssetsToAdd =
        {
            ("Assets/GGemCo/GGemCoSettings.asset", Settings),
            ("Assets/GGemCo/Prefabs/UI/TextDamage.prefab", TextFloatingDamage),
            ("Assets/GGemCo/Prefabs/Item/DropItem.prefab", SpriteDropItem)
        };
    }
}