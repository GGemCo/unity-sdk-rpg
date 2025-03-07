using System;
using System.Collections;
using System.Collections.Generic;
using GGemCo.Scripts.Characters.Monster;
using GGemCo.Scripts.Characters.Npc;
using GGemCo.Scripts.Characters.Player;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Maps.Objects;
using GGemCo.Scripts.SaveData;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
using GGemCo.Scripts.Addressable;
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

        private Coroutine coroutineRegenMonster;
        private TableLoaderManager tableLoaderManager;
        protected void Awake()
        {
            isLoadComplete = false;
            characterVid = 1;
            
            CreateGrid();
        }
        /// <summary>
        /// 타일맵을 추가할 grid 오브젝트 만들기
        /// </summary>
        private void CreateGrid()
        {
            gridTileMap = new GameObject(ConfigTags.GetValue(ConfigTags.Keys.GridTileMap))
            {
                tag = ConfigTags.GetValue(ConfigTags.Keys.GridTileMap)
            };
            Grid grid = gridTileMap.gameObject.AddComponent<Grid>();
            Vector2 tilemapGridSize = AddressableSettingsLoader.Instance.GetTilemapGridSize();
            grid.cellSize = new Vector3(tilemapGridSize.x, tilemapGridSize.y, 0);
            grid.cellLayout = GridLayout.CellLayout.Rectangle;
        }
        protected void Start()
        {
            sceneGame = SceneGame.Instance;
            saveDataManager = sceneGame.saveDataManager;
            tableLoaderManager = TableLoaderManager.Instance;
            defaultMonsterRegenTimeSec = tableLoaderManager.TableConfig.GetDefaultMonsterRegenTimeSec();
        }
        /// <summary>
        /// 초기 셋팅
        /// </summary>
        /// <param name="pbgBlackForMapLoading"></param>
        public void Initialize(GameObject pbgBlackForMapLoading)
        {
            bgBlackForMapLoading = pbgBlackForMapLoading;
        }
        protected void Reset()
        {
            isLoadComplete = false;
            if (coroutineRegenMonster != null)
            {
                StopCoroutine(coroutineRegenMonster);
            }
        }
        public void LoadMap(int mapUid = 0)
        {
            if (IsPossibleLoad() != true)
            {
                // GcLogger.LogError($"map state: {currentState}");
                return;
            }

            if (mapUid <= 0) return;
            // GcLogger.Log("LoadMap start");
            Reset();
            currentState = MapConstants.State.FadeIn;
            currentMapUid = mapUid;
            characterVid = 1;
            StartCoroutine(UpdateState());
        }

        private IEnumerator UpdateState()
        {
            while (currentState != MapConstants.State.Complete && currentState != MapConstants.State.Failed)
            {
                switch (currentState)
                {
                    case MapConstants.State.FadeIn:
                        yield return StartCoroutineSafe(FadeIn());
                        if (currentState == MapConstants.State.Failed) yield break;
                        currentState = MapConstants.State.UnloadPreviousStage;
                        break;

                    case MapConstants.State.UnloadPreviousStage:
                        yield return StartCoroutineSafe(UnloadPreviousStage());
                        if (currentState == MapConstants.State.Failed) yield break;
                        currentState = MapConstants.State.LoadTilemapPrefab;
                        break;

                    case MapConstants.State.LoadTilemapPrefab:
                        yield return StartCoroutineSafe(LoadTilemap());
                        if (currentState == MapConstants.State.Failed) yield break;
                        currentState = MapConstants.State.LoadPlayerPrefabs;
                        break;

                    case MapConstants.State.LoadPlayerPrefabs:
                        yield return StartCoroutineSafe(LoadPlayer());
                        if (currentState == MapConstants.State.Failed) yield break;
                        currentState = MapConstants.State.LoadMonsterPrefabs;
                        break;

                    case MapConstants.State.LoadMonsterPrefabs:
                        yield return StartCoroutineSafe(LoadMonsters());
                        if (currentState == MapConstants.State.Failed) yield break;
                        currentState = MapConstants.State.LoadNpcPrefabs;
                        break;

                    case MapConstants.State.LoadNpcPrefabs:
                        yield return StartCoroutineSafe(LoadNpcs());
                        if (currentState == MapConstants.State.Failed) yield break;
                        currentState = MapConstants.State.LoadWarpPrefabs;
                        break;

                    case MapConstants.State.LoadWarpPrefabs:
                        yield return StartCoroutineSafe(LoadWarps());
                        if (currentState == MapConstants.State.Failed) yield break;
                        currentState = MapConstants.State.FadeOut;
                        break;

                    case MapConstants.State.FadeOut:
                        yield return StartCoroutineSafe(FadeOut());
                        if (currentState == MapConstants.State.Failed) yield break;
                        currentState = MapConstants.State.Complete;
                        break;
                }

                yield return null;
            }

            if (currentState == MapConstants.State.Complete)
            {
                OnMapLoadComplete();
            }
            else
            {
                Debug.LogError("맵 로드 실패");
            }
        }
        /// <summary>
        /// 실패 시 즉시 종료되는 안전한 코루틴 실행 함수
        /// </summary>
        private IEnumerator StartCoroutineSafe(IEnumerator routine)
        {
            yield return StartCoroutine(routine);

            if (currentState == MapConstants.State.Failed)
            {
                yield break;
            }
        }
        /// <summary>
        /// 실패 시 currentState를 Failed로 설정하고 코루틴 종료
        /// </summary>
        private void SetLoadFailed(string errorMessage)
        {
            Debug.LogError($"맵 로드 실패: {errorMessage}");
            StartCoroutine(FadeOut());
            currentState = MapConstants.State.Failed;
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
            DestroyByTag(ConfigTags.GetValue(ConfigTags.Keys.Monster));
            DestroyByTag(ConfigTags.GetValue(ConfigTags.Keys.Npc));

            DestroyOthers();
            // 잠시 대기하여 오브젝트가 완전히 삭제되도록 보장
            yield return null;

            // 사용되지 않는 메모리 해제
            yield return Resources.UnloadUnusedAssets();

            // 필요시 가비지 컬렉션을 강제로 실행
            GC.Collect();

            // GcLogger.Log("이전 스테이지의 몬스터 프리팹이 메모리에서 해제되었습니다.");
        }

        private void DestroyOthers()
        {
            DestroyByTag(ConfigTags.GetValue(ConfigTags.Keys.Map));
        }

        /// <summary>
        /// MapEditor.cs:152
        /// </summary>
        private IEnumerator LoadTilemap()
        {
            if (gridTileMap == null)
            {
                SetLoadFailed($"Grid 오브젝트가 없습니다.");
                yield break;
            }
            // SceneGame.Instance.player?.GetComponent<Player>().Stop();

            if (currentMapUid == 0)
            {
                currentMapUid = saveDataManager.Player.CurrentChapter.Value;
            }
            if (tableLoaderManager.TableMap.GetCount() <= 0)
            {
                SetLoadFailed("맵 테이블에 내용이 없습니다.");
                yield break;
            }
            resultChapterData = tableLoaderManager.TableMap.GetDataByUid(currentMapUid);
            if (resultChapterData == null)
            {
                SetLoadFailed($"맵 테이블에서 찾을 수 없습니다. Uid: {currentMapUid}");
                yield break;
            }
            string path = GetFilePath(MapConstants.FileNameTilemap);
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                SetLoadFailed($"타일맵 prefab 이 없습니다. path: {path} / currentMapUid: {currentMapUid}");
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
                GcLogger.LogError($"몬스터 regen json 파싱중 오류. file {regenFileName}: {ex.Message}");
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
                GcLogger.LogError($"워프 json 파싱중 오류. file {rPathWarp}: {ex.Message}");
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
                GcLogger.LogError($"npc json 파싱중 오류. file {regenFileName}: {ex.Message}");
                yield break;
            }

            yield return null;
        }
        private IEnumerator LoadPlayer()
        {
            if (SceneGame.Instance.player == null)
            {
                GameObject prefabPlayer = Resources.Load<GameObject>(ConfigCommon.PathPlayerPrefab);
                if (prefabPlayer == null)
                {
                    GcLogger.LogError("플레이어 프리팹이 없습니다. path:"+ConfigCommon.PathPlayerPrefab);
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
            TableNpc tableNpc = tableLoaderManager.TableNpc;
            TableAnimation tableAnimation = tableLoaderManager.TableAnimation;
            
            foreach (NpcData npcData in npcList)
            {
                int uid = npcData.Uid;
                if (uid <= 0) continue;
                var info = tableNpc.GetDataByUid(uid);
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
            TableMonster myTableMonster = tableLoaderManager.TableMonster;
            TableAnimation tableAnimation = tableLoaderManager.TableAnimation;

            foreach (MonsterData monsterData in monsterList)
            {
                int uid = monsterData.Uid;
                if (uid <= 0) continue;
                var info = myTableMonster.GetDataByUid(uid);
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

        private void SetPlaySpawnPosition(Vector3 position)
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
            
            coroutineRegenMonster = StartCoroutine(RegenMonster(monsterData));
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
            int mapUid = monsterData.MapUid;
            if (mapUid != currentMapUid) yield break;
            if (uid <= 0) yield break;
            TableMonster myTableMonster = tableLoaderManager.TableMonster;
            TableAnimation tableAnimation = tableLoaderManager.TableAnimation;
            var info = myTableMonster.GetDataByUid(uid);
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
        /// <summary>
        /// 현재 맵 사이즈 가져오기
        /// </summary>
        /// <returns></returns>
        public (float width, float height) GetCurrentMapSize()
        {
            if (mapTileCommon == null) return (0, 0);
            return mapTileCommon.GetMapSize();
        }
        /// <summary>
        /// 워프로 맵 이동하기
        /// </summary>
        /// <param name="objectWarp"></param>
        public void LoadMapByWarp(ObjectWarp objectWarp)
        {
            if (objectWarp == null)
            {
                GcLogger.LogError("ObjectWarp.cs 가 없습니다.");
                return;
            }

            if (objectWarp.toMapUid <= 0)
            {
                GcLogger.LogError("이동할 워프 정보가 없습니다.");
                return;
            }
            SetPlaySpawnPosition(objectWarp.toMapPlayerSpawnPosition);
            LoadMap(objectWarp.toMapUid);
        }
    }
}