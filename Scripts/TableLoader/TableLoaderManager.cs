using System;
using System.Collections;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.TableLoader
{
    public class TableLoaderManager : MonoBehaviour
    {
        public static TableLoaderManager Instance;

        private static string[] _dataFiles;
        
        public TableConfig TableConfig { get; private set; } = new TableConfig();
        public TableNpc TableNpc { get; private set; } = new TableNpc();
        public TableMap TableMap { get; private set; } = new TableMap();
        public TableMonster TableMonster { get; private set; } = new TableMonster();
        public TableAnimation TableAnimation { get; private set; } = new TableAnimation();
        public TableItem TableItem { get; private set; } = new TableItem();

        private float loadProgress;
        private SceneLoading mySceneLoading;

        protected void Awake()
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
            _dataFiles = new[] { ConfigTableFileName.Config, ConfigTableFileName.Map, ConfigTableFileName.Monster, ConfigTableFileName.Npc, ConfigTableFileName.Animation, ConfigTableFileName.Item };
            mySceneLoading = GameObject.Find("SceneLoading").GetComponent<SceneLoading>();
            loadProgress = 0f;
        }

        private void Start()
        {
            StartCoroutine(LoadAllDataFiles());
        }

        private IEnumerator LoadAllDataFiles()
        {
            int fileCount = _dataFiles.Length;
            for (int i = 0; i < fileCount; i++)
            {
                LoadDataFile(_dataFiles[i]);
                loadProgress = (float)(i + 1) / fileCount * 100f;
                mySceneLoading?.SetTextLoadingPercent(loadProgress);
                yield return new WaitForSeconds(0.1f);
            }

            OnEndLoad();
        }

        private void LoadDataFile(string fileName)
        {
            try
            {
                TextAsset textFile = Resources.Load<TextAsset>($"Tables/{fileName}");
                if (textFile != null)
                {
                    string content = textFile.text;
                    if (!string.IsNullOrEmpty(content))
                    {
                        switch (fileName)
                        {
                            case ConfigTableFileName.Config:
                                TableConfig.LoadData(content);
                                break;
                            case ConfigTableFileName.Animation:
                                TableAnimation.LoadData(content);
                                break;
                            case ConfigTableFileName.Monster:
                                TableMonster.LoadData(content);
                                break;
                            case ConfigTableFileName.Npc:
                                TableNpc.LoadData(content);
                                break;
                            case ConfigTableFileName.Map:
                                TableMap.LoadData(content);
                                break;
                            case ConfigTableFileName.Item:
                                TableItem.LoadData(content);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"Error reading file {fileName}: {ex.Message}");
            }
        }

        private static void OnEndLoad()
        {
            // 로드 완료 후의 로직 추가
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
    }
}
