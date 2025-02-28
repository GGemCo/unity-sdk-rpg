using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GGemCo.Scripts.Maps.Objects
{
    public class ObjectWall : DefaultMapObject
    {
        protected override void InitComponents()
        {
            base.InitComponents();
            if (GetComponent<TilemapCollider2D>() == null)
            {
                ComponentController.AddTileMapCollider2D(gameObject, false, Vector2.zero);
            }
        }
        protected override void InitTagSortingLayer()
        {
            base.InitTagSortingLayer();
            gameObject.layer = LayerMask.NameToLayer(ConfigLayer.GetTileMapWall());
        }
    }
}