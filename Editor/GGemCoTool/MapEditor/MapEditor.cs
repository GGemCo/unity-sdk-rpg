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

        private const string Title = "Map 배치툴";
        private readonly string jsonFolderPath = Application.dataPath+"/Resources/Maps/";
        private string currentJsonFolderPath = "";
        private const string RESOURCES_FOLDER_PATH = "Maps/";

        // private string fileName = "regen_npc.json";
        // private string fileNameWarp = "warp.json";
        private string loadMapUid = "101";
        private const string NameTempTableLoaderManager = "TempTableLoaderManager";

        private const string FILE_NAME_TILEMAP = MapConstants.FileNameTilemap;
        private const string FILE_NAME_REGEN_NPC = MapConstants.FileNameRegenNpc;
        private const string FILE_NAME_REGEN_MONSTER = MapConstants.FileNameRegenMonster;
        private const string FILE_NAME_WARP = MapConstants.FileNameWarp;
        private const string FILE_EXT = MapConstants.FileExt;

        // private string jsonFileNameTilemap = _fileNameTilemap+_fileExt;
        private readonly string jsonFileNameRegenNpc = FILE_NAME_REGEN_NPC+FILE_EXT;
        private readonly string jsonFileNameRegenMonster = FILE_NAME_REGEN_MONSTER+FILE_EXT;
        private readonly string jsonFileNameWarp = FILE_NAME_WARP+FILE_EXT;
        
        private readonly NpcExporter npcExporter = new NpcExporter();
        private readonly MonsterExporter monsterExporter = new MonsterExporter();
        private readonly WarpExporter warpExporter = new WarpExporter();
        
        private static List<string> _npcNames; // NPC 이름 목록
        private static List<string> _monsterNames; // 몬스터 이름 목록
        
        private int selectedNpcIndex;
        private int selectedMonsterIndex;
        private const float GRID_CELL_SIZE = 64;

        [MenuItem("GGemCoTool/Map 배치툴", false, 3)]
        public static void ShowWindow()
        {
            GetWindow<MapExporter>(Title);
        }

        private void OnEnable()
        {
            selectedNpcIndex = 0;
            selectedMonsterIndex = 0;
            _tableMap = TableLoaderManager.LoadMapTable();
            
            // 타일맵을 추가할 grid
            _gridTileMap = GameObject.Find(ConfigTags.GetGridTileMap());
            if (_gridTileMap == null)
            {
                _gridTileMap = new GameObject(ConfigTags.GetGridTileMap())
                {
                    tag = ConfigTags.GetGridTileMap()
                };
                Grid grid = _gridTileMap.gameObject.AddComponent<Grid>();
                grid.cellSize = new Vector3(GRID_CELL_SIZE, GRID_CELL_SIZE, 0);
                grid.cellLayout = GridLayout.CellLayout.Rectangle;
            }

            var defaultMap = GameObject.FindObjectOfType<DefaultMap>();
            
            var tableNpc = TableLoaderManager.LoadNpcTable();
            var tableMonster = TableLoaderManager.LoadMonsterTable();
            var tableSpine = TableLoaderManager.LoadSpineTable();

            npcExporter.Initialize(tableNpc, tableSpine, defaultMap);
            monsterExporter.Initialize(tableMonster, tableSpine, defaultMap);
            warpExporter.Initialize(defaultMap);
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

            obj = GameObject.FindWithTag(ConfigTags.GetGridTileMap());
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
                npcExporter.AddNpcToMap(selectedNpcIndex);
            }
            
            GUILayout.Space(20);
            // 몬스터 추가 섹션
            GUILayout.Label("* 몬스터 추가", EditorStyles.whiteLargeLabel);
            // 몬스터 드롭다운
            selectedMonsterIndex = EditorGUILayout.Popup("몬스터 선택", selectedMonsterIndex, _monsterNames.ToArray());
            // 몬스터 추가 버튼
            if (GUILayout.Button("몬스터 추가"))
            {
                monsterExporter.AddMonsterToMap(selectedMonsterIndex);
            }
            
            GUILayout.Space(20);
            // 워프 추가 섹션
            GUILayout.Label("* 워프 추가", EditorStyles.whiteLargeLabel);
            // 워프 추가 버튼
            if (GUILayout.Button("워프 추가"))
            {
                warpExporter.AddWarpToMap();
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
            GameObject mapObject = GameObject.FindGameObjectWithTag(ConfigTags.GetMap());
        
            if (mapObject == null)
            {
                Debug.LogWarning("No GameObject with the tag 'Map' found in the scene.");
                return;
            }
            
            int mapUid = int.Parse(loadMapUid);
            npcExporter.ExportNpcDataToJson(currentJsonFolderPath, jsonFileNameRegenNpc, mapUid);
            monsterExporter.ExportMonsterDataToJson(currentJsonFolderPath, jsonFileNameRegenMonster, mapUid);
            warpExporter.ExportWarpDataToJson(currentJsonFolderPath, jsonFileNameWarp, mapUid);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(Title, "Json 저장하기 완료", "OK");
        }

        private void LoadJsonData()
        {
            int mapUid = int.Parse(loadMapUid);
            var mapData = _tableMap.GetMapData(mapUid);
            
            LoadTileData();
            npcExporter.LoadNpcData(RESOURCES_FOLDER_PATH + mapData.FolderName + "/" + FILE_NAME_REGEN_NPC);
            monsterExporter.LoadMonsterData(RESOURCES_FOLDER_PATH + mapData.FolderName + "/" + FILE_NAME_REGEN_MONSTER);
            warpExporter.LoadWarpData(RESOURCES_FOLDER_PATH + mapData.FolderName + "/" + FILE_NAME_WARP);
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

            string tilemapPath = RESOURCES_FOLDER_PATH + mapData.FolderName + "/" + FILE_NAME_TILEMAP;
            GameObject prefab = Resources.Load<GameObject>(tilemapPath);
            if (prefab == null)
            {
                GcLogger.LogError("맵 프리팹이 없습니다. mapUid : " + mapUid + " / path : "+tilemapPath);
                return;
            }

            if (_gridTileMap == null)
            {
                _gridTileMap = GameObject.Find(ConfigTags.GetGridTileMap());
            }
            currentJsonFolderPath = jsonFolderPath + mapData.FolderName + "/";
            GameObject currentMap = Instantiate(prefab, _gridTileMap.transform);
            _defaultMap = currentMap.GetComponent<MapTileCommon>();
            _defaultMap.InitComponents();
            _defaultMap.InitTagSortingLayer();
            _defaultMap.Initialize(mapData.Uid, mapData.Name, mapData.Type, mapData.Subtype);
            npcExporter.SetDefaultMap(_defaultMap);
            monsterExporter.SetDefaultMap(_defaultMap);
            warpExporter.SetDefaultMap(_defaultMap);
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
