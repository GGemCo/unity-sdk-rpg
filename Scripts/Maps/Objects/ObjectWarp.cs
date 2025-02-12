using GGemCo.Scripts.Configs;
using UnityEngine;

namespace GGemCo.Scripts.Maps.Objects
{
    public class ObjectWarp : MonoBehaviour
    {
        public WarpData WarpData;
        public int toMapUid; // 워프할 uid
        public Vector3 toMapPlayerSpawnPosition; // 워프될 곳에 플레이어가 스폰될 위치
        private BoxCollider2D boxCollider2D;

        private void Awake()
        {
            tag = ConfigTags.MapObjectWarp;
            boxCollider2D = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (WarpData == null) return;
            if (boxCollider2D == null)
            {
                boxCollider2D = GetComponent<BoxCollider2D>();
            }
            toMapUid = WarpData.ToMapUid;
            toMapPlayerSpawnPosition = new Vector3(WarpData.ToX, WarpData.ToY, WarpData.ToZ);
            transform.position = new Vector3(WarpData.x, WarpData.y, WarpData.z);
            transform.eulerAngles = new Vector3(WarpData.RotationX, WarpData.RotationY, WarpData.RotationZ);
            boxCollider2D.size = new Vector2(WarpData.BoxColliderSizeX, WarpData.BoxColliderSizeY);
        }
    }
}