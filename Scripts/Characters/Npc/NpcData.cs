using UnityEngine;

namespace GGemCo.Scripts.Characters.Npc
{
    public class NpcData
    {
        public int Uid;
        public int MapUid;
        public float x, y, z;
        public bool Flip;
        public bool DefaultVisible;

        public NpcData(int uid, Vector3 position, bool flip, int mapUid, bool defaultVisible)
        {
            Uid = uid;
            MapUid = mapUid;
            x = position.x;
            y = position.y;
            z = position.z;
            Flip = flip;
            DefaultVisible = defaultVisible;
        }
    }
}