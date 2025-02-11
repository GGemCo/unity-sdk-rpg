using System;
using System.Collections.Generic;
using System.IO;
using GGemCo.Scripts.Characters.Npc;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Maps;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GGemCo.Editor.GGemCoTool.MapEditor
{
    public class NpcExporter
    {
        private List<NpcData> npcList;
        private TableNpc tableNpc;
        private TableAnimation tableAnimation;
        private DefaultMap defaultMap;
        
        public void Initialize(TableNpc tableNpc, TableAnimation tableAnimation, DefaultMap defaultMap)
        {
            this.tableNpc = tableNpc;
            this.tableAnimation = tableAnimation;
            this.defaultMap = defaultMap;
        }
        public void SetDefaultMap(DefaultMap defaultMap)
        {
            this.defaultMap = defaultMap;
        }

        public void AddNpcToMap(int selectedNpcIndex)
        {
            if (defaultMap == null)
            {
                Debug.LogError("_defaultMap 이 없습니다.");
                return;
            }

            var npcDictionary = tableNpc.GetDatas();
            int index = 0;
            StruckTableNpc npcData = new StruckTableNpc();

            foreach (var outerPair in npcDictionary)
            {
                if (index == selectedNpcIndex)
                {
                    npcData = tableNpc.GetNpcData(outerPair.Key);
                    break;
                }
                index++;
            }

            if (npcData.Uid <= 0)
            {
                Debug.LogError("NPC 데이터가 없습니다.");
                return;
            }

            GameObject npcPrefab = tableAnimation.GetPrefab(npcData.SpineUid);
            if (npcPrefab == null)
            {
                Debug.LogError("NPC 프리팹을 찾을 수 없습니다.");
                return;
            }

            GameObject npc = Object.Instantiate(npcPrefab, Vector3.zero, Quaternion.identity, defaultMap.transform);

            var npcScript = npc.GetComponent<Npc>();
            if (npcScript != null)
            {
                npcScript.Uid = npcData.Uid;
                npcScript.SetScale(npcData.Scale);
            }

            Debug.Log($"{npcData.Name} NPC가 맵에 추가되었습니다.");
        }

        public void ExportNpcDataToJson(string filePath, string fileName, int mapUid)
        {
            GameObject mapObject = GameObject.FindGameObjectWithTag(ConfigTags.Map);
            NpcDataList saveNpcList = new NpcDataList();

            foreach (Transform child in mapObject.transform)
            {
                if (child.CompareTag(ConfigTags.Npc))
                {
                    var npc = child.gameObject.GetComponent<Npc>();
                    if (npc == null) continue;
                    saveNpcList.npcDataList.Add(new NpcData(npc.Uid, child.position, npc.flip, mapUid, true));
                }
            }

            string json = JsonConvert.SerializeObject(saveNpcList);
            string path = Path.Combine(filePath, fileName);
            File.WriteAllText(path, json);
            Debug.Log("NPC data exported to " + path);
        }
        
        public void LoadNpcData(string regenFileName)
        {
            // JSON 파일을 읽기
            try
            {
                TextAsset textFile = Resources.Load<TextAsset>($"{regenFileName}");
                if (textFile != null)
                {
                    string content = textFile.text;
                    if (!string.IsNullOrEmpty(content))
                    {
                        NpcDataList npcDataList = JsonConvert.DeserializeObject<NpcDataList>(content);
                        npcList = npcDataList.npcDataList;
                        SpawnNpc();
                    }
                }
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"Error reading file {regenFileName}: {ex.Message}");
            }
        }

        private void SpawnNpc()
        {
            if (defaultMap == null)
            {
                Debug.LogError("_defaultMap 이 없습니다.");
                return;
            }

            foreach (NpcData npcData in npcList)
            {
                int uid = npcData.Uid;
                if (uid <= 0) continue;
                var info = tableNpc.GetNpcData(uid);
                if (info.Uid <= 0 || info.SpineUid <= 0) continue;
                GameObject npcPrefab = tableAnimation.GetPrefab(info.SpineUid);
                if (npcPrefab == null)
                {
                    GcLogger.LogError("npc 프리팹이 없습니다. spine uid: " + info.SpineUid);
                    continue;
                }
                GameObject npc = Object.Instantiate(npcPrefab, new Vector3(npcData.x, npcData.y, npcData.z), Quaternion.identity, defaultMap.gameObject.transform);
                
                // NPC의 속성을 설정하는 스크립트가 있을 경우 적용
                Npc myNpcScript = npc.GetComponent<Npc>();
                if (myNpcScript != null)
                {
                    // MapManager.cs:138 도 수정
                    myNpcScript.Uid = npcData.Uid;
                    myNpcScript.NpcData = npcData;
                    // 추가적인 속성 설정을 여기에서 수행할 수 있습니다.
                    myNpcScript.SetScale(info.Scale);
                    // SetScale 다음에 처리해야 함
                    myNpcScript.flip = npcData.Flip;
                    myNpcScript.SetFlip(npcData.Flip);
                }
            }

            Debug.Log("NPCs spawned successfully.");
        }
    }
}
