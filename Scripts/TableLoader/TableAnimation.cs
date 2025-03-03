using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.TableLoader
{
    public class StruckTableAnimation
    {
        public int Uid;
        public string Name;
        public string PrefabPath;
        public GameObject Prefab;
        public float MoveStep;
        public Vector2 ColliderSize;
    }
    public class TableAnimation : DefaultTable
    {
        public string GetPrefabPath(int uid) => GetDataColumn(uid, "PrefabPath");
        
        public StruckTableAnimation GetDataByUid(int uid)
        {
            if (uid <= 0)
            {
                GcLogger.LogError("uid is 0.");
                return new StruckTableAnimation();
            }
            var data = GetData(uid);
            return new StruckTableAnimation
            {
                Uid = int.Parse(data["Uid"]),
                Name = data["Name"],
                PrefabPath = data["PrefabPath"],
                Prefab = LoadPrefab(data["PrefabPath"]),
                MoveStep = float.Parse(data["MoveStep"]),
                ColliderSize = ConvertVector2(data["ColliderSize"]),
            };
        }
        public GameObject GetPrefab(int uid) {
            var info = GetDataByUid(uid);
            if (info.Uid == 0) return null;
            return info.Prefab;
        }
    }
}