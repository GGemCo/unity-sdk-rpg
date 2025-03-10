using System;
using System.Threading.Tasks;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.ScriptableSettings;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GGemCo.Scripts.Addressable
{
    /// <summary>
    /// GGemCo Settings 불러오기
    /// </summary>
    public class AddressableSettingsLoader : MonoBehaviour
    {
        public static AddressableSettingsLoader Instance { get; private set; }

        public GGemCoSettings settings;
        public GGemCoPlayerSettings playerSettings;
        public GGemCoMapSettings mapSettings;
        public GGemCoSaveSettings saveSettings;

        public delegate void DelegateLoadSettings();
        public event DelegateLoadSettings OnLoadSettings;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Addressable Settings를 비동기적으로 로드하는 함수
        /// </summary>
        public async Task InitializeAsync()
        {
            await LoadAllSettingsAsync();
        }

        /// <summary>
        /// 모든 설정 파일을 Addressables에서 로드
        /// </summary>
        private async Task LoadAllSettingsAsync()
        {
            try
            {
                // 여러 개의 설정을 병렬적으로 로드
                var settingsTask = LoadSettingsAsync<GGemCoSettings>(ConfigAddressables.KeySettings);
                var playerSettingsTask = LoadSettingsAsync<GGemCoPlayerSettings>(ConfigAddressables.KeyPlayerSettings);
                var mapSettingsTask = LoadSettingsAsync<GGemCoMapSettings>(ConfigAddressables.KeyMapSettings);
                var saveSettingsTask = LoadSettingsAsync<GGemCoSaveSettings>(ConfigAddressables.KeySaveSettings);

                // 모든 작업이 완료될 때까지 대기
                await Task.WhenAll(settingsTask, playerSettingsTask, mapSettingsTask, saveSettingsTask);

                // 결과 저장
                settings = settingsTask.Result;
                playerSettings = playerSettingsTask.Result;
                mapSettings = mapSettingsTask.Result;
                saveSettings = saveSettingsTask.Result;

                // 로그 출력
                if (settings != null)
                    GcLogger.Log("Spine2d 사용여부 : " + settings.useSpine2d);
                if (playerSettings != null)
                    GcLogger.Log("Player statAtk : " + playerSettings.statAtk);
                if (mapSettings != null)
                    GcLogger.Log("Tilemap 크기 : " + mapSettings.tilemapGridCellSize);
                if (saveSettings != null)
                    GcLogger.Log("최대 저장 슬롯 개수 : " + saveSettings.saveDataMaxSlotCount);

                // 이벤트 호출
                OnLoadSettings?.Invoke();
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"설정 로딩 중 오류 발생: {ex.Message}");
            }
        }

        /// <summary>
        /// 제네릭을 사용하여 Addressables에서 설정을 로드하는 함수
        /// </summary>
        private async Task<T> LoadSettingsAsync<T>(string key) where T : ScriptableObject
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            return await handle.Task;
        }
    }
}
