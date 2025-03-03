﻿using System;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Editor.GGemCoTool.TableLoader
{
    public static class TableLoaderManager
    {
        // 공통적인 로드 메서드로, 제네릭 타입과 파일명을 받아 로드
        private static T LoadTable<T>(string fileName) where T : DefaultTable, new()
        {
            T tableData = null;
            try
            {
                TextAsset textFile = Resources.Load<TextAsset>($"Tables/{fileName}");
                if (textFile != null)
                {
                    string content = textFile.text;
                    if (!string.IsNullOrEmpty(content))
                    {
                        tableData = new T();
                        tableData.LoadData(content);
                    }
                    else
                    {
                        GcLogger.LogError($"테이블 내용이 없습니다. Tables/{fileName}");
                    }
                }
                else
                {
                    GcLogger.LogError($"테이블 파일을 찾을 수 없습니다. Tables/{fileName}");
                }
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"테이블 파일을 읽는중 오류 발생. Tables/{fileName}: {ex.Message}");
            }

            return tableData;
        }

        public static TableMap LoadMapTable()
        {
            return LoadTable<TableMap>(ConfigTableFileName.Map);
        }
        public static TableNpc LoadNpcTable()
        {
            return LoadTable<TableNpc>(ConfigTableFileName.Npc);
        }
        public static TableMonster LoadMonsterTable()
        {
            return LoadTable<TableMonster>(ConfigTableFileName.Monster);
        }
        public static TableAnimation LoadSpineTable()
        {
            return LoadTable<TableAnimation>(ConfigTableFileName.Animation);
        }
        public static TableItem LoadItemTable()
        {
            return LoadTable<TableItem>(ConfigTableFileName.Item);
        }
        public static TableItemDropGroup LoadItemDropGroupTable()
        {
            return LoadTable<TableItemDropGroup>(ConfigTableFileName.ItemDropGroup);
        }
        public static TableMonsterDropRate LoadMonsterDropRateTable()
        {
            return LoadTable<TableMonsterDropRate>(ConfigTableFileName.MonsterDropRate);
        }
    }
}