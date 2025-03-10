using System.Collections.Generic;

namespace GGemCo.Scripts.Configs
{
    public static class ConfigAddressables
    {
        // 기본 설정 Scriptable Object
        public const string KeySettings = "GGemCo_Settings";
        public const string KeyPlayerSettings = "GGemCo_PlayerSettings";
        public const string KeyMapSettings = "GGemCo_MapSettings";
        public const string KeySaveSettings = "GGemCo_SaveSettings";
        // 몬스터 데미지 표시 text object
        public const string KeyTextFloatingDamage = "GGemCo_TextFloatingDamage";
        // 드랍 아이템 프리팹
        public const string KeyPrefabDropItem = "GGemCo_PrefabDropItem";
        // 윈도우 slot 프리팹
        public const string KeyPrefabSlot = "GGemCo_PrefabSlot";
        // 아이콘 아이템 프리팹
        public const string KeyPrefabIconItem = "GGemCo_PrefabIconItem";
        
        public static readonly (string key, string path)[] AssetsToAdd =
        {
            (KeySettings, "Assets/GGemCo/GGemCoSettings.asset"),
            (KeyPlayerSettings, "Assets/GGemCo/GGemCoPlayerSettings.asset"),
            (KeyMapSettings, "Assets/GGemCo/GGemCoMapSettings.asset"),
            (KeySaveSettings, "Assets/GGemCo/GGemCoSaveSettings.asset"),
            (KeyTextFloatingDamage, "Assets/GGemCo/Prefabs/UI/TextDamage.prefab"),
            (KeyPrefabSlot, "Assets/GGemCo/Prefabs/UI/Slot.prefab"),
            (KeyPrefabIconItem, "Assets/GGemCo/Prefabs/UI/IconItem.prefab"),
            (KeyPrefabDropItem, "Assets/GGemCo/Prefabs/Item/DropItem.prefab"),
        };
        
        // 게임 에서 사용하는 미리 로드해야하는 프리팹
        public const string LabelPreLoadGamePrefabs = "GGemCo_PreLoadGamePrefabs";
        
        public static readonly Dictionary<string, string> AssetsToAddLabel = new Dictionary<string, string>
        {
            { KeyTextFloatingDamage, LabelPreLoadGamePrefabs },
            { KeyPrefabSlot, LabelPreLoadGamePrefabs },
            { KeyPrefabIconItem, LabelPreLoadGamePrefabs },
            { KeyPrefabDropItem, LabelPreLoadGamePrefabs },
        };
    }
}