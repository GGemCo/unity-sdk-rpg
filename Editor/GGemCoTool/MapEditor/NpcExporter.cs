using System;
using System.Collections.Generic;
using System.IO;
using GGemCo.Scripts;
using Newtonsoft.Json;
using UnityEngine;

namespace GGemCo.Editor
{
    /// <summary>
    /// 맵 배치툴 > Npc 배치, 내보내기
    /// </summary>
    public class NpcExporter
    {
        private List<NpcData> npcList;
        private TableNpc tableNpc;
        private TableAnimation tableAnimation;
        private DefaultMap defaultMap;
        private CharacterManager characterManager;
        
        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="pTableNpc"></param>
        /// <param name="pTableAnimation"></param>
        /// <param name="pDefaultMap"></param>
        public void Initialize(TableNpc pTableNpc, TableAnimation pTableAnimation, DefaultMap pDefaultMap)
        {
            tableNpc = pTableNpc;
            tableAnimation = pTableAnimation;
            defaultMap = pDefaultMap;
            characterManager = new CharacterManager();
            characterManager.Initialize();
        }
        /// <summary>
        /// 배치할 맵 셋팅
        /// </summary>
        /// <param name="pDefaultMap"></param>
        public void SetDefaultMap(DefaultMap pDefaultMap)
        {
            defaultMap = pDefaultMap;
        }
        /// <summary>
        /// 맵에 npc 추가하기
        /// </summary>
        /// <param name="selectedNpcIndex"></param>
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
                    npcData = tableNpc.GetDataByUid(outerPair.Key);
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

            GameObject npc = characterManager.CreateNpc(npcPrefab, Vector3.zero, defaultMap.gameObject.transform);
            var npcScript = npc.GetComponent<Npc>();
            if (npcScript != null)
            {
                npcScript.uid = npcData.Uid;
                npcScript.SetScale(npcData.Scale);
                npcScript.InitTagSortingLayer();
            }

            Debug.Log($"{npcData.Name} NPC가 맵에 추가되었습니다.");
        }
        /// <summary>
        /// 배치한 정보 json 으로 내보내기
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <param name="mapUid"></param>
        public void ExportNpcDataToJson(string filePath, string fileName, int mapUid)
        {
            GameObject mapObject = GameObject.FindGameObjectWithTag(ConfigTags.GetValue(ConfigTags.Keys.Map));
            NpcDataList saveNpcList = new NpcDataList();

            foreach (Transform child in mapObject.transform)
            {
                if (child.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Npc)))
                {
                    var npc = child.gameObject.GetComponent<Npc>();
                    if (npc == null) continue;
                    saveNpcList.npcDataList.Add(new NpcData(npc.uid, child.position, npc.isFlip, mapUid, true));
                }
            }

            string json = JsonConvert.SerializeObject(saveNpcList);
            string path = Path.Combine(filePath, fileName);
            File.WriteAllText(path, json);
            Debug.Log("NPC data exported to " + path);
        }
        /// <summary>
        /// json 에서 npc 정보 불러오기
        /// </summary>
        /// <param name="regenFileName"></param>
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
        /// <summary>
        /// npc 생성하기
        /// </summary>
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
                var info = tableNpc.GetDataByUid(uid);
                if (info.Uid <= 0 || info.SpineUid <= 0) continue;
                GameObject npcPrefab = tableAnimation.GetPrefab(info.SpineUid);
                if (npcPrefab == null)
                {
                    GcLogger.LogError("npc 프리팹이 없습니다. spine uid: " + info.SpineUid);
                    continue;
                }
                GameObject npc = characterManager.CreateNpc(npcPrefab, new Vector3(npcData.x, npcData.y, npcData.z), defaultMap.gameObject.transform);
                
                // NPC의 속성을 설정하는 스크립트가 있을 경우 적용
                Npc myNpcScript = npc.GetComponent<Npc>();
                if (myNpcScript != null)
                {
                    // MapManager.cs:138 도 수정
                    myNpcScript.uid = npcData.Uid;
                    myNpcScript.NpcData = npcData;
                    // 추가적인 속성 설정을 여기에서 수행할 수 있습니다.
                    myNpcScript.SetScale(info.Scale);
                    // SetScale 다음에 처리해야 함
                    myNpcScript.isFlip = npcData.Flip;
                    myNpcScript.SetFlip(npcData.Flip);
                    myNpcScript.InitTagSortingLayer();
                }
            }

            Debug.Log("NPCs spawned successfully.");
        }
    }
}
