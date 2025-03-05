using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GGemCo.Scripts.Addressable
{
    public class AddressableSettingsLoader : MonoBehaviour
    {
        public static AddressableSettingsLoader Instance { get; private set; }
        private GGemCoSettings settings;
        private Dictionary<string, GameObject> preLoadGamePrefabs;
        private float prefabLoadProgress;
        
        private async void Awake()
        {
            try
            {
                prefabLoadProgress = 0f;
                try
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
                    settings = await LoadSettings();
                    if (settings != null)
                    {
                        GcLogger.Log("Spine2d 사용여부 : " + settings.useSpine2d);
                    }
                }
                catch (Exception ex)
                {
                    GcLogger.LogError($"addressable 로딩중 오류. : {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"프리팹 로드중 오류 : {ex.Message}");
            }
        }
        /// <summary>
        /// 기본 셋팅 Srcriptable Object 로드하기
        /// </summary>
        /// <returns></returns>
        private async Task<GGemCoSettings> LoadSettings()
        {
            if (settings == null)
            {
                AsyncOperationHandle<GGemCoSettings> handle = Addressables.LoadAssetAsync<GGemCoSettings>(ConfigAddressables.KeySettings);
                settings = await handle.Task;

                if (settings == null)
                {
                    GcLogger.LogError("GGemCoSettings을 Addressables에서 불러오지 못했습니다!");
                }
            }
            return settings;
        }
        /// <summary>
        /// Spine2d 사용여부 가져오기
        /// </summary>
        /// <returns></returns>
        public bool GetUseSpine2d()
        {
            return settings.useSpine2d;
        }
        /// <summary>
        /// 타일맵 사이즈 가져오기
        /// </summary>
        /// <returns></returns>
        public Vector2 GetTilemapGridSize()
        {
            return settings.tilemapGridCellSize;
        }
        /// <summary>
        /// 프리팹 이름으로 프리팹 가져오기
        /// </summary>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        public GameObject GetPreLoadGamePrefabByName(string prefabName)
        {
            if (preLoadGamePrefabs.ContainsKey(prefabName)) return preLoadGamePrefabs.GetValueOrDefault(prefabName);
            GcLogger.LogError($"Addressables 에 {prefabName} 프리팹이 없습니다.");
            return null;
        }
        /// <summary>
        /// 진행률 가져오기
        /// </summary>
        /// <returns></returns>
        public float GetPrefabLoadProgress()
        {
            return prefabLoadProgress;
        }
        /// <summary>
        /// 게임에서 사용할 프리팹 로드하기
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, GameObject>> LoadAllPreLoadGamePrefabsAsync()
        {
            Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
            var locationHandle = Addressables.LoadResourceLocationsAsync(ConfigAddressables.LabelPreLoadGamePrefabs);
            await locationHandle.Task;
        
            if (!locationHandle.IsValid() || locationHandle.Status != AsyncOperationStatus.Succeeded)
            {
                GcLogger.LogError($"{ConfigAddressables.LabelPreLoadGamePrefabs} 레이블을 가진 리소스를 찾을 수 없습니다.");
                return prefabs;
            }
        
            int totalCount = locationHandle.Result.Count;
            int loadedCount = 0;
        
            foreach (var location in locationHandle.Result)
            {
                string address = location.PrimaryKey;
                var loadHandle = Addressables.LoadAssetAsync<GameObject>(address);
                
                while (!loadHandle.IsDone)
                {
                    prefabLoadProgress = (loadedCount + loadHandle.PercentComplete) / totalCount;
                    await Task.Yield();
                }
        
                GameObject prefab = await loadHandle.Task;
                if (prefab != null)
                {
                    prefabs[address] = prefab;
                    loadedCount++;
                }
            }
        
            prefabLoadProgress = 1f; // 100%
            return prefabs;
        }
        /// <summary>
        /// 로드된 프리팹 저장하기
        /// </summary>
        /// <param name="gamePrefabs"></param>
        public void SetPreLoadGamePrefabs(Dictionary<string, GameObject> gamePrefabs)
        {
            preLoadGamePrefabs = gamePrefabs;
        }
    }
}