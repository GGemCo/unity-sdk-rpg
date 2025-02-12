using GGemCo.Scripts.Configs;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo.Scripts.Maps.Objects
{
    [ExecuteInEditMode]  // 에디터 모드에서도 생명주기 함수 실행
    public class DefaultMapObject : MonoBehaviour
    {
        protected virtual void Awake()
        {
            InitComponents();
            InitTagSortingLayer();
        }

        public virtual void InitComponents()
        {
            
        }
        public virtual void InitTagSortingLayer()
        {
            tag = ConfigTags.MapObject;
            if (GetComponent<TilemapRenderer>() != null)
            {
                GetComponent<TilemapRenderer>().sortingLayerName = ConfigSortingLayer.MapObject;
            }
        }
    }
}