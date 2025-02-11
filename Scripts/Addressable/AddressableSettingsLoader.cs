using System.Threading.Tasks;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Core;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GGemCo.Scripts.Addressable
{
    public class AddressableSettingsLoader : MonoBehaviour
    {
        private static GGemCoSettings _settings;

        private async void Awake()
        {
            _settings = await LoadSettings();
            if (_settings != null)
            {
                GcLogger.Log("useSpine2d : " + _settings.useSpine2d);
            }
        }

        private static async Task<GGemCoSettings> LoadSettings()
        {
            if (_settings == null)
            {
                AsyncOperationHandle<GGemCoSettings> handle = Addressables.LoadAssetAsync<GGemCoSettings>(ConfigAddressableKeys.Settings);
                _settings = await handle.Task;

                if (_settings == null)
                {
                    Debug.LogError("GGemCoSettings을 Addressables에서 불러오지 못했습니다!");
                }
            }
            return _settings;
        }

        public bool GetUseSpine2d()
        {
            return _settings.useSpine2d;
        }
    }
}