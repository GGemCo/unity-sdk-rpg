using UnityEngine;

namespace GGemCo.Scripts.Characters.Monster
{
    public class MonsterData
    {
        public int Uid;
        public int MapUid;
        public float x, y, z;
        public bool IsFlip;
        public bool DefaultVisible;

        public MonsterData(int uid, Vector3 position, bool isFlip, int mapUid, bool defaultVisible)
        {
            Uid = uid;
            MapUid = mapUid;
            x = position.x;
            y = position.y;
            z = position.z;
            IsFlip = isFlip;
            DefaultVisible = defaultVisible;
        }
    }
}