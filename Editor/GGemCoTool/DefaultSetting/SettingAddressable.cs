using GGemCo.Editor.GGemCoTool.Utils;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace GGemCo.Editor.GGemCoTool.DefaultSetting
{
    public class SettingAddressable
    {
        private const string Title = "Addressable 추가하기";
        private const string DefaultGroupName = "Default Local Group"; // 기본 그룹 이름

        // 추가할 파일 목록 (경로, 키 값)
        private static readonly (string path, string key)[] AssetsToAdd =
        {
            ("Assets/GGemCo/GGemCoSettings.asset", "GGemCo_Settings"),
            ("Assets/GGemCo/Prefabs/UI/TextDamage.prefab", "GGemCo_TextFloatingDamage")
        };

        public void OnGUI()
        {
            Common.OnGUITitle(Title);

            if (GUILayout.Button(Title))
            {
                SetupAddressable();
            }
        }

        private void SetupAddressable()
        {
            // AddressableSettings 가져오기 (없으면 생성)
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("Addressable 설정을 찾을 수 없습니다. 새로 생성합니다.");
                settings = CreateAddressableSettings();
            }

            // 기본 그룹 가져오기 (없으면 생성)
            AddressableAssetGroup defaultGroup = settings.DefaultGroup ?? CreateDefaultGroup(settings);

            if (defaultGroup == null)
            {
                Debug.LogError("기본 Addressable 그룹을 설정할 수 없습니다.");
                return;
            }

            foreach (var (assetPath, keyName) in AssetsToAdd)
            {
                // 대상 파일 가져오기
                var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (asset == null)
                {
                    Debug.LogError($"파일을 찾을 수 없습니다: {assetPath}");
                    continue;
                }

                // 기존 Addressable 항목 확인
                AddressableAssetEntry entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(assetPath));

                if (entry == null)
                {
                    // 신규 Addressable 항목 추가
                    entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), defaultGroup);
                    Debug.Log($"Addressable 항목을 추가했습니다: {assetPath}");
                }
                else
                {
                    Debug.Log($"이미 Addressable에 등록된 항목입니다: {assetPath}");
                }

                // 키 값 설정
                entry.address = keyName;
                Debug.Log($"Addressable 키 값 설정: {keyName}");
            }

            // 설정 저장
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog(Title, "Addressable 설정 완료", "OK");
        }

        /// <summary>
        /// Addressable 설정이 없을 경우 새로 생성
        /// </summary>
        private AddressableAssetSettings CreateAddressableSettings()
        {
            var settings = AddressableAssetSettings.Create(
                "Assets/AddressableAssetsData", 
                "AddressableAssetSettings", 
                true, 
                true
            );

            AddressableAssetSettingsDefaultObject.Settings = settings;
            AssetDatabase.SaveAssets();
            Debug.Log("새로운 Addressable 설정을 생성했습니다.");
            return settings;
        }

        /// <summary>
        /// 기본 Addressable 그룹이 없을 경우 생성
        /// </summary>
        private AddressableAssetGroup CreateDefaultGroup(AddressableAssetSettings settings)
        {
            var defaultGroup = settings.CreateGroup(
                DefaultGroupName, 
                false, 
                false, 
                true, 
                settings.DefaultGroup.Schemas
            );

            settings.DefaultGroup = defaultGroup;
            Debug.Log("새로운 기본 Addressable 그룹을 생성했습니다.");
            return defaultGroup;
        }
    }
}
