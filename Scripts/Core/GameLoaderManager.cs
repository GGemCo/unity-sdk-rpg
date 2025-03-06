using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.TableLoader;
using TMPro;
using UnityEngine;

namespace GGemCo.Scripts.Core
{
    public class GameLoaderManager : MonoBehaviour
    {
        private enum Type
        {
            None,
            Table,
            GamePrefab
        }

        private static GameLoaderManager Instance { get; set; }

        public TextMeshProUGUI textLoadingPercent; // 진행률 표시
        private Dictionary<string, GameObject> gamePrefabs;
        private TableLoaderManager tableLoader;
        private AddressableSettingsLoader addressableLoader;

        private float tableLoadProgress;
        private float prefabLoadProgress;
        private float totalProgress;

        private void Awake()
        {
            tableLoadProgress = 0f;
            prefabLoadProgress = 0f;
            totalProgress = 0f;
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (textLoadingPercent != null)
            {
                textLoadingPercent.text = "0%";
            }

            tableLoader = gameObject.AddComponent<TableLoaderManager>();
            addressableLoader = gameObject.AddComponent<AddressableSettingsLoader>();
        }

        private void Start()
        {
            StartCoroutine(LoadGameData());
        }

        private IEnumerator LoadGameData()
        {
            yield return LoadTableData();
            yield return LoadAddressablePrefabs();
            UnityEngine.SceneManagement.SceneManager.LoadScene(ConfigDefine.SceneNameGame);
        }

        /// <summary>
        /// 테이블 데이터를 로드하고 진행률을 업데이트합니다.
        /// </summary>
        private IEnumerator LoadTableData()
        {
            string[] dataFiles = tableLoader.GetDataFiles();
            int fileCount = dataFiles.Length;

            for (int i = 0; i < fileCount; i++)
            {
                tableLoader.LoadDataFile(dataFiles[i]);
                tableLoadProgress = (float)(i + 1) / fileCount * 50f; // 전체 로드의 50%
                UpdateLoadingProgress(Type.Table);
                yield return new WaitForSeconds(0.1f);
            }
        }

        /// <summary>
        /// Addressable 리소스를 로드하고 진행률을 업데이트합니다.
        /// </summary>
        private IEnumerator LoadAddressablePrefabs()
        {
            Task<Dictionary<string, GameObject>> prefabLoadTask = addressableLoader.LoadAllPreLoadGamePrefabsAsync();

            while (!prefabLoadTask.IsCompleted)
            {
                prefabLoadProgress = addressableLoader.GetPrefabLoadProgress() * 50f; // 나머지 50%
                UpdateLoadingProgress(Type.GamePrefab);
                yield return null;
            }

            addressableLoader.SetPreLoadGamePrefabs(prefabLoadTask.Result);
        }

        /// <summary>
        /// 진행률을 계산하고 UI 업데이트
        /// </summary>
        private void UpdateLoadingProgress(Type type)
        {
            totalProgress = tableLoadProgress + prefabLoadProgress;
            string subTitle = "테이블";
            if (type == Type.GamePrefab)
            {
                subTitle = "리소스";
            }
            if (textLoadingPercent != null)
            {
                textLoadingPercent.text = $"{subTitle} 중... {Mathf.Floor(totalProgress)}%";
            }
        }
    }
}
