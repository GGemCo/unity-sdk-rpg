using System;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.TableLoader
{
    /// <summary>
    /// 데이터 테이블 Loader
    /// </summary>
    public class TableLoaderManager : MonoBehaviour
    {
        public static TableLoaderManager Instance;

        private string[] dataFiles;
        
        public TableNpc TableNpc { get; private set; } = new TableNpc();
        public TableMap TableMap { get; private set; } = new TableMap();
        public TableMonster TableMonster { get; private set; } = new TableMonster();
        public TableAnimation TableAnimation { get; private set; } = new TableAnimation();
        public TableItem TableItem { get; private set; } = new TableItem();
        public TableMonsterDropRate TableMonsterDropRate { get; private set; } = new TableMonsterDropRate();
        public TableItemDropGroup TableItemDropGroup { get; private set; } = new TableItemDropGroup();
        public TableExp TableExp { get; private set; } = new TableExp();
        public TableWindow TableWindow { get; private set; } = new TableWindow();
        public TableStatus TableStatus { get; private set; } = new TableStatus();
        public TableSkill TableSkill { get; private set; } = new TableSkill();
        public TableAffect TableAffect { get; private set; } = new TableAffect();
        public TableEffect TableEffect { get; private set; } = new TableEffect();

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

            dataFiles = new[]
            {
                ConfigTableFileName.Config, ConfigTableFileName.Map, ConfigTableFileName.Monster,
                ConfigTableFileName.Npc, ConfigTableFileName.Animation, ConfigTableFileName.Item,
                ConfigTableFileName.MonsterDropRate, ConfigTableFileName.ItemDropGroup, ConfigTableFileName.Exp,
                ConfigTableFileName.Window, ConfigTableFileName.Status, ConfigTableFileName.Skill, ConfigTableFileName.Affect,
                ConfigTableFileName.Effect
            };
        }

        public void LoadDataFile(string fileName)
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
                            case ConfigTableFileName.MonsterDropRate:
                                TableMonsterDropRate.LoadData(content);
                                break;
                            case ConfigTableFileName.ItemDropGroup:
                                TableItemDropGroup.LoadData(content);
                                break;
                            case ConfigTableFileName.Exp:
                                TableExp.LoadData(content);
                                break;
                            case ConfigTableFileName.Window:
                                TableWindow.LoadData(content);
                                break;
                            case ConfigTableFileName.Status:
                                TableStatus.LoadData(content);
                                break;
                            case ConfigTableFileName.Skill:
                                TableSkill.LoadData(content);
                                break;
                            case ConfigTableFileName.Affect:
                                TableAffect.LoadData(content);
                                break;
                            case ConfigTableFileName.Effect:
                                TableEffect.LoadData(content);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"테이블 파싱중 오류. file {fileName}: {ex.Message}");
            }
        }

        public string[] GetDataFiles()
        {
            return dataFiles;
        }
    }
}
