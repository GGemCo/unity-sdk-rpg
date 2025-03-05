using System.Collections.Generic;

namespace GGemCo.Scripts.Configs
{
    public static class ConfigAddressables
    {
        // 기본 설정 Scriptable Object
        public const string KeySettings = "GGemCo_Settings";
        // 몬스터 데미지 표시 text object
        public const string KeyTextFloatingDamage = "GGemCo_TextFloatingDamage";
        // 드랍 아이템 이미지
        public const string KeySpriteDropItem = "GGemCo_SpriteDropItem";
        // 아이콘 아이템
        public const string KeySpriteIconItem = "GGemCo_SpriteIconItem";

        public static readonly (string key, string path)[] AssetsToAdd =
        {
            (KeySettings, "Assets/GGemCo/GGemCoSettings.asset"),
            (KeyTextFloatingDamage, "Assets/GGemCo/Prefabs/UI/TextDamage.prefab"),
            (KeySpriteIconItem, "Assets/GGemCo/Prefabs/UI/IconItem.prefab"),
            (KeySpriteDropItem, "Assets/GGemCo/Prefabs/Item/DropItem.prefab"),
        };
        
        // 게임 에서 사용하는 미리 로드해야하는 프리팹
        public const string LabelPreLoadGamePrefabs = "GGemCo_PreLoadGamePrefabs";
        
        public static readonly Dictionary<string, string> AssetsToAddLabel = new Dictionary<string, string>
        {
            { KeyTextFloatingDamage, LabelPreLoadGamePrefabs },
            { KeySpriteIconItem, LabelPreLoadGamePrefabs },
            { KeySpriteDropItem, LabelPreLoadGamePrefabs },
        };
    }
}