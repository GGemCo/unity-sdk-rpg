using System;
using System.Collections;
using System.Collections.Generic;
using GGemCo.Scripts.Characters.Monster;
using GGemCo.Scripts.Characters.Npc;
using GGemCo.Scripts.Characters.Player;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Core;
using GGemCo.Scripts.Maps.Objects;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GGemCo.Scripts.Maps
{
    /// <summary>
    /// 맵 매니저
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        public GameObject gridTileMap;
        public GameObject bgBlackForMapLoading;  // 페이드 인에 사용할 검정색 스프라이트 오브젝트
        public float fadeDuration = 0.3f;  // 페이드 인 지속 시간
        public float gridCellSize = 64;
        
        private MapConstants.State currentState = MapConstants.State.None;
        private bool isLoadComplete;

        private int currentMapUid; // 현재 맵 uid

        private SceneGame sceneGame;
        private SaveDataManager saveDataManager;
        private Vector3 playSpawnPosition;
        private float defaultMonsterRegenTimeSec;
        private int characterVid;

        public UnityEvent onLoadCompleteMap;
        
        private List<NpcData> npcList;
        private List<MonsterData> monsterList;
        private List<WarpData> warpDatas;
        private StruckTableMap resultChapterData;
        
        private MapTileCommon mapTileCommon;
        private UnityEvent onLoadTileMap;
        protected void Awake()
        {
            isLoadComplete = false;
            characterVid = 1;
            
            CreateGrid();
            InitializeCanvasBlockInteraction();
        }
        /// <summary>
        /// 타일맵을 추가할 grid 오브젝트 만들기
        /// </summary>
        private void CreateGrid()
        {
            gridTileMap = new GameObject(ConfigTags.GridTileMap);
            gridTileMap.tag = ConfigTags.GridTileMap;
            Grid grid = gridTileMap.gameObject.AddComponent<Grid>();
            grid.cellSize = new Vector3(gridCellSize, gridCellSize, 0);
            grid.cellLayout = GridLayout.CellLayout.Rectangle;
        }
        /// <summary>
        /// 맵 이동시 보여줄 검정 로딩 canvas
        /// </summary>
        void InitializeCanvasBlockInteraction()
        {
            GameObject gameObjectCanvasBlockInteraction = GameObject.FindWithTag(ConfigTags.CanvasBlockInteraction);
            bgBlackForMapLoading = gameObjectCanvasBlockInteraction.transform.GetChild(0).gameObject;
        }
        protected void Start()
        {
            sceneGame = SceneGame.Instance;
            saveDataManager = sceneGame.saveDataManager;
            defaultMonsterRegenTimeSec = TableLoaderManager.instance.TableConfig.GetDefaultMonsterRegenTimeSec();
        }

        protected void Reset()
        {
            isLoadComplete = false;
        }
        public void LoadMap(int mapUid = 0)
        {
            if (IsPossibleLoad() != true)
            {
                // FgLogger.LogError($"map state: {currentState}");
                return;
            }
            // FgLogger.Log("LoadMap start");
            Reset();
            currentState = MapConstants.State.FadeIn;
            currentMapUid = mapUid;
            characterVid = 1;
            StartCoroutine(UpdateState());
        }

        IEnumerator UpdateState()
        {
            while (currentState != MapConstants.State.Complete)
            {
                switch (currentState)
                {
                    case MapConstants.State.FadeIn:
                        yield return StartCoroutine(FadeIn());
                        currentState = MapConstants.State.UnloadPreviousStage;
                        break;
                    case MapConstants.State.UnloadPreviousStage:
                        yield return StartCoroutine(UnloadPreviousStage());
                        currentState = MapConstants.State.LoadTilemapPrefab;
                        break;
                    case MapConstants.State.LoadTilemapPrefab:
                        yield return StartCoroutine(LoadTilemap());
                        currentState = MapConstants.State.LoadPlayerPrefabs;
                        break;
                    // 플레이어를 먼저 로드해야 카메라 위치가 정해지고 캐릭터 culling 이 자연스럽게 된다 
                    case MapConstants.State.LoadPlayerPrefabs:
                        yield return StartCoroutine(LoadPlayer());
                        currentState = MapConstants.State.LoadMonsterPrefabs;
                        break;
                    case MapConstants.State.LoadMonsterPrefabs:
                        yield return StartCoroutine(LoadMonsters());
                        currentState = MapConstants.State.LoadNpcPrefabs;
                        break;
                    case MapConstants.State.LoadNpcPrefabs:
                        yield return StartCoroutine(LoadNpcs());
                        currentState = MapConstants.State.LoadWarpPrefabs;
                        break;
                    case MapConstants.State.LoadWarpPrefabs:
                        yield return StartCoroutine(LoadWarps());
                        currentState = MapConstants.State.FadeOut;
                        break;
                    case MapConstants.State.FadeOut:
                        yield return StartCoroutine(FadeOut());
                        currentState = MapConstants.State.Complete;
                        break;
                }

                yield return null;
            }

            OnMapLoadComplete();
        }

        IEnumerator FadeIn()
        {
            if (bgBlackForMapLoading == null)
            {
                GcLogger.LogError("Fade Sprite가 설정되지 않았습니다.");
                yield break;
            }
            // 이미 활성화 되어있으면 (인게임 처음 시작했을때) 건너뛰기.
            if (bgBlackForMapLoading.activeSelf)
            {
                yield break;
            }

            bgBlackForMapLoading.SetActive(true);
            Image spriteRenderer = bgBlackForMapLoading.GetComponent<Image>();
            spriteRenderer.color = new Color(0, 0, 0, 0);
            float elapsedTime = 0.0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / fadeDuration);
                float alpha = Mathf.Lerp(0, 1, Easing.EaseOutQuintic(t));
                spriteRenderer.color = new Color(0, 0, 0, alpha);
                yield return null;
            }

            // Fade in이 완료된 후에 완전히 불투명하게 설정
            spriteRenderer.color = new Color(0, 0, 0, 1);

            // Logger.Log("Fade In 완료");
        }

        private void DestroyByTag(string pTag)
        {
            GameObject[] maps = GameObject.FindGameObjectsWithTag(pTag);
            foreach (GameObject map in maps)
            {
                if (map == null) continue;
                Destroy(map);
            }
        }
        IEnumerator UnloadPreviousStage()
        {
            // 현재 씬에 있는 모든 몬스터 오브젝트를 삭제
            DestroyByTag(ConfigTags.Monster);
            DestroyByTag(ConfigTags.Npc);

            DestroyOthers();
            // 잠시 대기하여 오브젝트가 완전히 삭제되도록 보장
            yield return null;

            // 사용되지 않는 메모리 해제
            yield return Resources.UnloadUnusedAssets();

            // 필요시 가비지 컬렉션을 강제로 실행
            GC.Collect();

            // FgLogger.Log("이전 스테이지의 몬스터 프리팹이 메모리에서 해제되었습니다.");
        }

        private void DestroyOthers()
        {
            DestroyByTag(ConfigTags.Map);
            DestroyByTag(ConfigTags.ButtonNpcQuest);
        }

        /// <summary>
        /// MapEditor.cs:152
        /// </summary>
        private IEnumerator LoadTilemap()
        {
            if (gridTileMap == null)
            {
                GcLogger.Log($"dont exist grid tile map");
                yield break;
            }
            // SceneGame.Instance.player?.GetComponent<Player>().Stop();

            if (currentMapUid == 0)
            {
                currentMapUid = saveDataManager.CurrentChapter;
            }
            resultChapterData = TableLoaderManager.instance.TableMap.GetMapData(currentMapUid);
            string path = GetFilePath(MapConstants.FileNameTilemap);
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                GcLogger.Log($"dont exist prefab path. path: {path} / currentMapUid: {currentMapUid}");
                yield break;
            }
            // bgm 플레이
            if (resultChapterData.BgmUid > 0)
            {
            }

            if (mapTileCommon != null)
            {
                Destroy(mapTileCommon.gameObject);
            }
            GameObject currentMap = Instantiate(prefab, gridTileMap.transform);
            mapTileCommon = currentMap.GetComponent<MapTileCommon>();
            mapTileCommon.Initialize(resultChapterData.Uid, resultChapterData.Name, resultChapterData.Type, resultChapterData.Subtype);
            var result = mapTileCommon.GetMapSize();

            // 로드된 맵에 맞게 맵 영역 사이즈 갱신하기 
            SceneGame.Instance.cameraManager.ChangeMapSize(result.width, result.height);
            
            onLoadTileMap?.Invoke();
            // Logger.Log("타일맵 프리팹 로드 완료");
            yield return null;
        }

        private IEnumerator LoadMonsters()
        {
            string regenFileName = GetFilePath(MapConstants.FileNameRegenMonster);

            try
            {
                TextAsset textFile = Resources.Load<TextAsset>($"{regenFileName}");
                if (textFile != null)
                {
                    string content = textFile.text;
                    if (!string.IsNullOrEmpty(content))
                    {
                        MonsterDataList monsterDataList = JsonConvert.DeserializeObject<MonsterDataList>(content);
                        monsterList = monsterDataList.monsterDataList;
                        SpawnMonsters();
                    }
                }
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"Error reading file {regenFileName}: {ex.Message}");
                yield break;
            }

            yield return null;
        }

        private IEnumerator LoadWarps()
        {
            string rPathWarp = GetFilePath(MapConstants.FileNameWarp);

            try
            {
                TextAsset textFile = Resources.Load<TextAsset>($"{rPathWarp}");
                if (textFile != null)
                {
                    string content = textFile.text;
                    if (!string.IsNullOrEmpty(content))
                    {
                        WarpDataList warpDataList = JsonConvert.DeserializeObject<WarpDataList>(content);
                        warpDatas = warpDataList.warpDataList;
                        SpawnWarps();
                    }
                }
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"Error reading file {rPathWarp}: {ex.Message}");
                yield break;
            }
            yield return null;
        }

        private IEnumerator LoadNpcs()
        {
            string regenFileName = GetFilePath(MapConstants.FileNameRegenNpc);

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
                        SpawnNpcs();
                    }
                }
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"Error reading file {regenFileName}: {ex.Message}");
                yield break;
            }

            yield return null;
        }
        private IEnumerator LoadPlayer()
        {
            if (SceneGame.Instance.player == null)
            {
                GameObject prefabPlayer = Resources.Load<GameObject>(ConfigCommon.pathPlayerPrefab);
                if (prefabPlayer == null)
                {
                    GcLogger.LogError("플레이어 프리팹이 없습니다. path:"+ConfigCommon.pathPlayerPrefab);
                    yield break;
                }
                GameObject player = Instantiate(prefabPlayer, new Vector3(0, 0, 0), Quaternion.identity);
                SceneGame.Instance.player = player;
            }
            
            // 플레이어 위치
            Vector3 spawnPosition = resultChapterData.PlayerSpawnPosition;
            if (playSpawnPosition != Vector3.zero)
            {
                spawnPosition = playSpawnPosition;
            }
            SceneGame.Instance.player.GetComponent<Player>().MoveForce(spawnPosition.x, spawnPosition.y);
            // SceneGame.Instance.cameraManager.MoveCameraPosition(spawnPosition.x, spawnPosition.y);

            // yield return new WaitForSeconds(ConfigCommon.CharacterFadeSec/2);
            yield return null;
        }
        IEnumerator FadeOut()
        {
            if (bgBlackForMapLoading == null)
            {
                GcLogger.LogError("Fade Sprite가 설정되지 않았습니다.");
                yield break;
            }

            Image spriteRenderer = bgBlackForMapLoading.GetComponent<Image>();
            spriteRenderer.color = new Color(0, 0, 0, 1);
            float elapsedTime = 0.0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / fadeDuration);
                float alpha = Mathf.Lerp(1, 0, Easing.EaseInQuintic(t));
                spriteRenderer.color = new Color(0, 0, 0, alpha);
                yield return null;
            }

            // Fade in이 완료된 후에 완전히 불투명하게 설정
            spriteRenderer.color = new Color(0, 0, 0, 0);
            bgBlackForMapLoading.SetActive(false);

            // Logger.Log("Fade Out 완료");
        }

        private void SpawnNpcs()
        {
            TableNpc tableNpc = TableLoaderManager.instance.TableNpc;
            TableAnimation tableAnimation = TableLoaderManager.instance.TableAnimation;
            
            foreach (NpcData npcData in npcList)
            {
                int uid = npcData.Uid;
                if (uid <= 0) continue;
                var info = tableNpc.GetNpcData(uid);
                if (info.Uid <= 0 || info.SpineUid <= 0) continue;
                GameObject npcPrefab = tableAnimation.GetPrefab(info.SpineUid);
                if (npcPrefab == null)
                {
                    GcLogger.LogError("프리팹이 없습니다. spine uid: " + info.SpineUid);
                    continue;
                }
                GameObject npc = Instantiate(npcPrefab, new Vector3(npcData.x, npcData.y, npcData.z), Quaternion.identity, mapTileCommon.gameObject.transform);
            
                // NPC의 이름과 기타 속성 설정
                Npc myNpcScript = npc.GetComponent<Npc>();
                if (myNpcScript != null)
                {
                    // npcExporter.cs:158 도 수정
                    myNpcScript.Vid = characterVid;
                    myNpcScript.Uid = npcData.Uid;
                    myNpcScript.NpcData = npcData;
                    
                    mapTileCommon.AddNpc(characterVid, npc);
                    characterVid++;
                }
            }
        }
        private void SpawnMonsters()
        {
            TableMonster myTableMonster = TableLoaderManager.instance.TableMonster;
            TableAnimation tableAnimation = TableLoaderManager.instance.TableAnimation;

            foreach (MonsterData monsterData in monsterList)
            {
                int uid = monsterData.Uid;
                if (uid <= 0) continue;
                var info = myTableMonster.GetMonsterData(uid);
                if (info.Uid <= 0 || info.SpineUid <= 0) continue;
                GameObject monsterPrefab = tableAnimation.GetPrefab(info.SpineUid);
                if (monsterPrefab == null)
                {
                    GcLogger.LogError("프리팹이 없습니다. spine uid: " + info.SpineUid);
                    continue;
                }
                GameObject monster = Instantiate(monsterPrefab, new Vector3(monsterData.x, monsterData.y, monsterData.z), Quaternion.identity, mapTileCommon.gameObject.transform);
            
                // 몬스터의 이름과 기타 속성 설정
                Monster myMonsterScript = monster.GetComponent<Monster>();
                if (myMonsterScript != null)
                {
                    // monsterExporter.cs:158 도 수정
                    myMonsterScript.Vid = characterVid;
                    myMonsterScript.Uid = monsterData.Uid;
                    myMonsterScript.MonsterData = monsterData;
                    
                    mapTileCommon.AddMonster(characterVid, monster);
                    characterVid++;
                }
            }
        }
        private void SpawnWarps()
        {
            GameObject warpPrefab = Resources.Load<GameObject>(MapConstants.PathPrefabWarp);
            if (warpPrefab == null)
            {
                GcLogger.LogError("워프 프리팹이 없습니다. path:"+MapConstants.PathPrefabWarp);
                return;
            }
            foreach (WarpData warpData in warpDatas)
            {
                int toMapUid = warpData.ToMapUid;
                if (toMapUid <= 0) continue;
                GameObject warp = Instantiate(warpPrefab, new Vector3(warpData.x, warpData.y, warpData.z), Quaternion.identity, mapTileCommon.gameObject.transform);
            
                // 워프의 이름과 기타 속성 설정
                ObjectWarp objectWarp = warp.GetComponent<ObjectWarp>();
                if (objectWarp != null)
                {
                    // warpExporter.cs:128 도 수정
                    objectWarp.WarpData = warpData;
                }
            }
        }
        void OnMapLoadComplete()
        {
            StopAllCoroutines();

            // SceneGame.saveDataManager.SetChapter(currentMapUid);
            isLoadComplete = true;
            playSpawnPosition = Vector3.zero;
            
            onLoadCompleteMap?.Invoke();
            // Logger.Log("맵 로드 완료");
        }
        private bool IsPossibleLoad()
        {
            return (currentState == MapConstants.State.Complete || currentState == MapConstants.State.None);
        }

        public bool IsLoadComplete()
        {
            return isLoadComplete;
        }

        string GetFilePath(string fileName)
        {
            return MapConstants.ResourceMapPath + resultChapterData.FolderName + "/" + fileName;
        }

        public void SetPlaySpawnPosition(Vector3 position)
        {
            playSpawnPosition = position;
        }
        public Vector3 GetPlaySpawnPosition()
        {
            return playSpawnPosition;
        }
        /// <summary>
        /// 몬스터 죽었을때 리젠 처리 
        /// </summary>
        /// <param name="vid"></param>
        public void OnDeadMonster(int vid)
        {
            if (vid <= 0) return;
            MonsterData monsterData = mapTileCommon.GetMonsterDataByVid(vid);
            if (monsterData == null) return;
            
            StartCoroutine(RegenMonster(monsterData));
        }
        /// <summary>
        /// 몬스터 리젠하기 
        /// </summary>
        /// <param name="monsterData"></param>
        private IEnumerator RegenMonster(MonsterData monsterData)
        {
            yield return new WaitForSeconds(defaultMonsterRegenTimeSec);
            if (monsterData == null) yield break;
            int uid = monsterData.Uid;
            if (uid <= 0) yield break;
            TableMonster myTableMonster = TableLoaderManager.instance.TableMonster;
            TableAnimation tableAnimation = TableLoaderManager.instance.TableAnimation;
            var info = myTableMonster.GetMonsterData(uid);
            if (info.Uid <= 0 || info.SpineUid <= 0) yield break;
            GameObject monsterPrefab = tableAnimation.GetPrefab(info.SpineUid);
            if (monsterPrefab == null)
            {
                GcLogger.LogError("프리팹이 없습니다. spine uid: " + info.SpineUid);
                yield break;
            }
            GameObject monster = Instantiate(monsterPrefab, new Vector3(monsterData.x, monsterData.y, monsterData.z), Quaternion.identity, mapTileCommon.gameObject.transform);
            
            // 몬스터의 이름과 기타 속성 설정
            Monster myMonsterScript = monster.GetComponent<Monster>();
            if (myMonsterScript != null)
            {
                // monsterExporter.cs:158 도 수정
                myMonsterScript.Vid = characterVid;
                myMonsterScript.Uid = monsterData.Uid;
                myMonsterScript.MonsterData = monsterData;

                mapTileCommon.AddMonster(characterVid, monster);
                characterVid++;
            }
        }
    }
}
