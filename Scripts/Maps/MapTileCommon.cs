using System.Collections.Generic;
using GGemCo.Scripts.Characters.Monster;
using GGemCo.Scripts.Characters.Npc;
using GGemCo.Scripts.Scenes;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo.Scripts.Maps
{
    public class MapTileCommon : DefaultMap
    {
        public Camera mainCamera;
        public float extraTileCount = 200f;
        private TilemapRenderer tilemapRenderer;

        // 맵에 배치된 몬스터
        private Dictionary<int, GameObject> monsters = new Dictionary<int, GameObject>();
        // 맵에 배치된 npc
        private Dictionary<int, GameObject> npcs = new Dictionary<int, GameObject>();

        private Bounds cullingBounds; // 현재 컬링 범위를 저장할 변수
        private float mainCameraZ;

        protected override void Awake()
        {
            base.Awake();
            tilemapRenderer = GetComponent<TilemapRenderer>();
        }

        void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = SceneGame.Instance.mainCamera;
            }
            mainCameraZ = mainCamera.transform.position.z;
            CalculateCullingBounds();
        }

        public override void Initialize(int uid, string chapterName, MapConstants.Type type, MapConstants.SubType subType)
        {
            base.Initialize(uid, chapterName, type, subType);
            var size = GetMapSize();
            gameObject.transform.position = new Vector3(0, 0, 0);
        }

        void LateUpdate()
        {
            // 카메라가 이동할 때마다 컬링 범위 및 오브젝트 상태 갱신
            CalculateCullingBounds();
        }

        void CalculateCullingBounds()
        {
            if (mainCamera == null || tilemapRenderer == null) return;

            // 카메라 크기 계산
            float verticalSize = mainCamera.orthographicSize;
            float horizontalSize = verticalSize * mainCamera.aspect;

            // Culling Bounds 설정
            tilemapRenderer.chunkCullingBounds = new Vector3(
                horizontalSize + extraTileCount,
                verticalSize + extraTileCount,
                0
            );

            // 카메라의 현재 위치를 기준으로 컬링 영역을 갱신
            Vector3 cameraPosition = mainCamera.transform.position;
            cullingBounds = new Bounds(cameraPosition, new Vector3(
                (horizontalSize + extraTileCount) * 2,
                (verticalSize + extraTileCount) * 2,
                0
            ));

            // 오브젝트 활성화/비활성화 처리
            UpdateObjectActivation(monsters, cullingBounds);
            UpdateObjectActivation(npcs, cullingBounds);
        }

        void UpdateObjectActivation(Dictionary<int, GameObject> objects, Bounds bounds)
        {
            foreach (var info in objects)
            {
                GameObject obj = info.Value;
                if (obj == null) continue;

                // NPC의 Z 축도 고려하여 활성화 상태 확인
                Vector3 position = obj.transform.position;
                bool isActive = bounds.Contains(new Vector3(position.x, position.y, mainCameraZ));

                // 활성화 상태 설정
                if (obj.activeSelf != isActive)
                {
                    if (isActive)
                    {
                        obj.GetComponent<Npc>()?.StartFadeIn();
                        obj.GetComponent<Monster>()?.StartFadeIn();
                    }
                    else
                    {
                        obj.GetComponent<Npc>()?.StartFadeOut();
                        obj.GetComponent<Monster>()?.StartFadeOut();
                    }
                    // obj.SetActive(isActive);
                }
            }
        }
        /// <summary>
        /// npc를 스폰하면서 List 에 추가하기
        /// </summary>
        /// <param name="vid"></param>
        /// <param name="npc"></param>
        public void AddNpc(int vid, GameObject npc)
        {
            if (npc == null) return;
            npcs.Add(vid, npc);
        }
        /// <summary>
        /// 몬스터를 스폰하면서 List 에 추가하기
        /// </summary>
        /// <param name="vid"></param>
        /// <param name="monster"></param>
        public void AddMonster(int vid, GameObject monster)
        {
            if (monster == null) return;
            monsters.Add(vid, monster);
        }
        /// <summary>
        /// 퀘스트 상태가 변경되었을때, npc 에 연결된 퀘스트 버튼 업데이트 해주기 
        /// </summary>
        public void OnChangeNpcQuestStatus()
        {
            foreach (var npc in npcs)
            {
                if (npc.Value == null) continue;
                // NpcQuestButton npcQuestButton = npc.GetComponent<NpcQuestButton>();
                // if (npcQuestButton == null) continue;
                // npcQuestButton.OnChangeQuestStatus();
            }
        }

        public MonsterData GetMonsterDataByVid(int vid)
        {
            GameObject monster = monsters.GetValueOrDefault(vid);
            if (monster == null) return null;
            Monster myMonster = monster.GetComponent<Monster>();
            if (myMonster == null) return null;
            return myMonster.MonsterData;
        }

        void OnDrawGizmos()
        {
            if (mainCamera == null) return;

            // 카메라의 가로, 세로 뷰 크기 계산
            float verticalSize = mainCamera.orthographicSize;
            float horizontalSize = verticalSize * mainCamera.aspect;

            // 카메라 뷰의 영역 시각화 (초록색)
            Vector3 cameraPosition = mainCamera.transform.position;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(
                cameraPosition,
                new Vector3(horizontalSize * 2 + extraTileCount * 2, verticalSize * 2 + extraTileCount * 2, 0)
            );

            // 컬링 영역 시각화 (빨간색)
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(cullingBounds.center, cullingBounds.size);
        }
    }
}
