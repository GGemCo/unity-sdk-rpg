using System.Collections.Generic;
using GGemCo.Scripts.Characters.Monster;
using GGemCo.Scripts.Characters.Npc;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Maps;
using GGemCo.Scripts.Maps.Objects;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
using UnityEditor;
using UnityEngine;
using TableLoaderManager = GGemCo.Editor.GGemCoTool.TableLoader.TableLoaderManager;

namespace GGemCo.Editor.GGemCoTool.MapEditor
{
    public class MapExporter : EditorWindow
    {
        private List<NpcData> npcList;
        private List<MonsterData> monsterDatas;
        private List<WarpData> warpDatas;
        private static MapTileCommon _defaultMap;
        private static GameObject _gridTileMap;
        private static GameObject _player;
        
        private static TableMap _tableMap;
        private static TableNpc _tableNpc;
        private static TableMonster _tableMonster;
        private static TableAnimation _tableAnimation;

        private string jsonFolderPath = Application.dataPath+"/Resources/Maps/";
        private string currentJsonFolderPath = "";
        private string resourcesFolderPath = "Maps/";
        // private string fileName = "regen_npc.json";
        // private string fileNameWarp = "warp.json";
        private string loadMapUid = "101";
        private const string NameTempTableLoaderManager = "TempTableLoaderManager";

        private static string _fileNameTilemap = MapConstants.FileNameTilemap;
        private static string _fileNameRegenNpc = MapConstants.FileNameRegenNpc;
        private static string _fileNameRegenMonster = MapConstants.FileNameRegenMonster;
        private static string _fileNameWarp = MapConstants.FileNameWarp;
        private static string _fileExt = MapConstants.FileExt;
        private string jsonFileNameTilemap = _fileNameTilemap+_fileExt;
        private string jsonFileNameRegenNpc = _fileNameRegenNpc+_fileExt;
        private string jsonFileNameRegenMonster = _fileNameRegenMonster+_fileExt;
        private string jsonFileNameWarp = _fileNameWarp+_fileExt;
        
        private NpcExporter _npcExporter = new NpcExporter();
        private MonsterExporter _monsterExporter = new MonsterExporter();
        private WarpExporter _warpExporter = new WarpExporter();
        
        private static List<string> _npcNames; // NPC 이름 목록
        private static List<string> _monsterNames; // 몬스터 이름 목록
        
        private int selectedNpcIndex = 0;
        private int selectedMonsterIndex = 0;
        private float gridCellSize = 64;

        [MenuItem("GGemCoTool/Map 배치툴")]
        public static void ShowWindow()
        {
            GetWindow<MapExporter>("Map 배치툴");
        }

        private void OnEnable()
        {
            _tableMap = TableLoaderManager.LoadMapTable();
            
            // 타일맵을 추가할 grid
            _gridTileMap = GameObject.Find(ConfigTags.GridTileMap);
            if (_gridTileMap == null)
            {
                _gridTileMap = new GameObject(ConfigTags.GridTileMap);
                _gridTileMap.tag = ConfigTags.GridTileMap;
                Grid grid = _gridTileMap.gameObject.AddComponent<Grid>();
                grid.cellSize = new Vector3(gridCellSize, gridCellSize, 0);
                grid.cellLayout = GridLayout.CellLayout.Rectangle;
            }

            var defaultMap = GameObject.FindObjectOfType<DefaultMap>();
            
            var tableNpc = TableLoaderManager.LoadNpcTable();
            var tableMonster = TableLoaderManager.LoadMonsterTable();
            var tableSpine = TableLoaderManager.LoadSpineTable();

            _npcExporter.Initialize(tableNpc, tableSpine, defaultMap);
            _monsterExporter.Initialize(tableMonster, tableSpine, defaultMap);
            _warpExporter.Initialize(defaultMap);
             LoadNpcInfoData();
             LoadMonsterInfoData();
        }

        private void OnDestroy()
        {
            GameObject obj = GameObject.Find(NameTempTableLoaderManager);
            if (obj)
            {
                DestroyImmediate(obj);
            }

            obj = GameObject.FindWithTag(ConfigTags.GridTileMap);
            if (obj)
            {
                DestroyImmediate(obj);
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("* 맵 배치 불러오기", EditorStyles.whiteLargeLabel);
            // 파일 경로 및 파일명 입력
            loadMapUid = EditorGUILayout.TextField("Map Uid", loadMapUid);
            // 불러오기 버튼
            if (GUILayout.Button("불러오기"))
            {
                LoadJsonData();
            }
            
            GUILayout.Space(20);
            // NPC 추가 섹션
            GUILayout.Label("* NPC 추가", EditorStyles.whiteLargeLabel);
            // NPC 드롭다운
            selectedNpcIndex = EditorGUILayout.Popup("NPC 선택", selectedNpcIndex, _npcNames.ToArray());
            // NPC 추가 버튼
            if (GUILayout.Button("NPC 추가"))
            {
                _npcExporter.AddNpcToMap(selectedNpcIndex);
            }
            
            GUILayout.Space(20);
            // 몬스터 추가 섹션
            GUILayout.Label("* 몬스터 추가", EditorStyles.whiteLargeLabel);
            // 몬스터 드롭다운
            selectedMonsterIndex = EditorGUILayout.Popup("몬스터 선택", selectedMonsterIndex, _monsterNames.ToArray());
            // 몬스터 추가 버튼
            if (GUILayout.Button("몬스터 추가"))
            {
                _monsterExporter.AddMonsterToMap(selectedMonsterIndex);
            }
            
            GUILayout.Space(20);
            // 워프 추가 섹션
            GUILayout.Label("* 워프 추가", EditorStyles.whiteLargeLabel);
            // 워프 추가 버튼
            if (GUILayout.Button("워프 추가"))
            {
                _warpExporter.AddWarpToMap();
            }
            
            GUILayout.Space(20);
            GUILayout.Label("* 맵 배치 저장하기", EditorStyles.whiteLargeLabel);
            currentJsonFolderPath = EditorGUILayout.TextField("저장 위치", currentJsonFolderPath);
            // _fileNameRegen = EditorGUILayout.TextField("리젠 파일 이름", _fileNameRegen);
            // _fileNameWarp = EditorGUILayout.TextField("Warp File Name", _fileNameWarp);
            if (GUILayout.Button("Json 으로 저장하기"))
            {
                ExportDataToJson();
            }
        }
        private void ExportDataToJson()
        {
            // 태그가 'Map'인 오브젝트를 찾습니다.
            GameObject mapObject = GameObject.FindGameObjectWithTag(ConfigTags.Map);
        
            if (mapObject == null)
            {
                Debug.LogWarning("No GameObject with the tag 'Map' found in the scene.");
                return;
            }
            
            int mapUid = int.Parse(loadMapUid);
            var mapData = _tableMap.GetMapData(mapUid);
            _npcExporter.ExportNpcDataToJson(currentJsonFolderPath, jsonFileNameRegenNpc, mapUid);
            _monsterExporter.ExportMonsterDataToJson(currentJsonFolderPath, jsonFileNameRegenMonster, mapUid);
            _warpExporter.ExportWarpDataToJson(currentJsonFolderPath, jsonFileNameWarp, mapUid);
            AssetDatabase.Refresh();
        }

        private void LoadJsonData()
        {
            int mapUid = int.Parse(loadMapUid);
            var mapData = _tableMap.GetMapData(mapUid);
            
            LoadTileData();
            _npcExporter.LoadNpcData(resourcesFolderPath + mapData.FolderName + "/" + _fileNameRegenNpc);
            _monsterExporter.LoadMonsterData(resourcesFolderPath + mapData.FolderName + "/" + _fileNameRegenMonster);
            _warpExporter.LoadWarpData(resourcesFolderPath + mapData.FolderName + "/" + _fileNameWarp);
        }
        /// <summary>
        /// MapManager.cs:25
        /// </summary>
        private void LoadTileData()
        {
            int mapUid = int.Parse(loadMapUid);
            var mapData = _tableMap.GetMapData(mapUid);
            if (mapData.Uid <= 0)
            {
                GcLogger.LogError("맵 데이터가 없거나 리젠 파일명이 없습니다.");
                return;
            }
            
            if (_defaultMap != null)
            {
                DestroyImmediate(_defaultMap.gameObject);
            }

            string tilemapPath = resourcesFolderPath + mapData.FolderName + "/" + _fileNameTilemap;
            GameObject prefab = Resources.Load<GameObject>(tilemapPath);
            if (prefab == null)
            {
                GcLogger.LogError("맵 프리팹이 없습니다. mapUid : " + mapUid + " / path : "+tilemapPath);
                return;
            }

            if (_gridTileMap == null)
            {
                _gridTileMap = GameObject.Find(ConfigTags.GridTileMap);
            }
            currentJsonFolderPath = jsonFolderPath + mapData.FolderName + "/";
            GameObject currentMap = Instantiate(prefab, _gridTileMap.transform);
            _defaultMap = currentMap.GetComponent<MapTileCommon>();
            _defaultMap.Initialize(mapData.Uid, mapData.Name, mapData.Type, mapData.Subtype);
            _npcExporter.SetDefaultMap(_defaultMap);
            _monsterExporter.SetDefaultMap(_defaultMap);
            _warpExporter.SetDefaultMap(_defaultMap);
        }
        /// <summary>
        /// npc 정보 불러오기
        /// </summary>
        private static void LoadNpcInfoData()
        {
            _tableNpc = TableLoaderManager.LoadNpcTable();
             
            Dictionary<int, Dictionary<string, string>> npcDictionary = _tableNpc.GetDatas();
             
            _npcNames = new List<string>();
            // foreach 문을 사용하여 딕셔너리 내용을 출력
            foreach (KeyValuePair<int, Dictionary<string, string>> outerPair in npcDictionary)
            {
                var info = _tableNpc.GetNpcData(outerPair.Key);
                if (info.Uid <= 0) continue;
                _npcNames.Add($"{info.Uid} - {info.Name}");
            }
        }
        /// <summary>
        ///  몬스터 정보 불러오기
        /// </summary>
        private static void LoadMonsterInfoData()
        {
            _tableMonster = TableLoaderManager.LoadMonsterTable();
             
            Dictionary<int, Dictionary<string, string>> monsterDictionary = _tableMonster.GetDatas();
             
            _monsterNames = new List<string>();
            // foreach 문을 사용하여 딕셔너리 내용을 출력
            foreach (KeyValuePair<int, Dictionary<string, string>> outerPair in monsterDictionary)
            {
                var info = _tableMonster.GetMonsterData(outerPair.Key);
                if (info.Uid <= 0) continue;
                _monsterNames.Add($"{info.Uid} - {info.Name}");
            }
        }
    }
}
