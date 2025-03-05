using GGemCo.Scripts.Characters.Npc;
using GGemCo.Scripts.Configs;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo.Scripts.Maps
{
    public class DefaultMap : MonoBehaviour
    {
        private int chapterNumber;
        private string chapterName;
        private MapConstants.Type mapType;
        private MapConstants.SubType mapSubType;
        private Tilemap tilemap;

        protected virtual void Awake()
        {
            tilemap = GetComponent<Tilemap>();
            
            InitComponents();
            InitTagSortingLayer();
        }

        public virtual void InitComponents()
        {
            
        }

        public virtual void InitTagSortingLayer()
        {
            tag = ConfigTags.GetValue(ConfigTags.Keys.Map);
            GetComponent<TilemapRenderer>().sortingLayerName = ConfigSortingLayer.GetValue(ConfigSortingLayer.Keys.MapTerrain);
        }

        public virtual void Initialize(int uid, string name, MapConstants.Type type, MapConstants.SubType subType)
        {
            chapterNumber = uid;
            chapterName = name;
            mapType = type;
            mapSubType = subType;
        }
        /// <summary>
        /// 같은 장의 맵인지 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsSameChapter(int value)
        {
            return chapterNumber == value;
        }
        /// <summary>
        /// 현재 맵에 연결된 장 가져오기
        /// </summary>
        /// <returns></returns>
        public int GetChapterNumber()
        {
            return chapterNumber;
        }
        /// <summary>
        /// 맵 사이즈 구하기
        /// </summary>
        /// <returns></returns>
        public (float width, float height) GetMapSize()
        {
            if (tilemap == null)
            {
                tilemap = GetComponent<Tilemap>();
            }
            // 실제 타일이 배치된 경계를 추적하기 위한 변수들
            Vector3Int min = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
            Vector3Int max = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

            // 타일맵의 모든 셀을 순회하며 타일이 있는 위치를 확인
            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos))
                {
                    min = Vector3Int.Min(min, pos);
                    max = Vector3Int.Max(max, pos);
                }
            }

            // 타일이 있는 범위의 크기를 계산
            Vector3Int size = max - min + Vector3Int.one;

            // 셀 크기를 고려하여 실제 월드 공간에서의 크기 계산
            Vector3 cellSize = tilemap.cellSize;
            float totalWidth = size.x * cellSize.x;
            float totalHeight = size.y * cellSize.y;

            // Logger.Log("Total Tilemap Width: " + totalWidth + ", Total Tilemap Height: " + totalHeight);

            return (totalWidth, totalHeight);
        }
        /// <summary>
        /// 맵에 배치된 npc 중에 uid 로 가져오기
        /// </summary>
        /// <param name="npcUid"></param>
        /// <returns></returns>
        public Npc GetNpcByUid(int npcUid)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var childTransform = transform.GetChild(i);
                if (childTransform == null) continue;
                Npc npc = childTransform.GetComponent<Npc>();
                if (npc == null) continue;
                if (npc.Uid == npcUid)
                {
                    return npc;
                }
            }
        
            return null;
        }
    }
}
