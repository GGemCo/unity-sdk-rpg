using GGemCo.Scripts.Configs;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo.Scripts.Maps.Objects
{
    public class DefaultMapObject : MonoBehaviour
    {
        protected virtual void Awake()
        {
            tag = ConfigTags.MapObject;
            if (GetComponent<TilemapRenderer>() != null)
            {
                GetComponent<TilemapRenderer>().sortingLayerName = ConfigSortingLayer.MapObject;
            }
        }
    }
}