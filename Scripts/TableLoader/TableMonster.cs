using System.Collections.Generic;
using GGemCo.Scripts.Characters;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.TableLoader
{
    public class StruckTableMonster
    {
        public int Uid;
        public string Name;
        public int SpineUid;
        public string DefaultSkin;
        public float Scale;
        public ICharacter.Grade Grade;
        public long StatHp;
        public long StatAtk;
        public float StatMoveSpeed;
        public float StatAttackSpeed;
        public long RewardExp;
        public long RewardGold;
    }
    public class TableMonster : DefaultTable
    {
        private static readonly Dictionary<string, ICharacter.Grade> MapGrade;

        static TableMonster()
        {
            MapGrade = new Dictionary<string, ICharacter.Grade>
            {
                { "Common", ICharacter.Grade.Common },
                { "Boss", ICharacter.Grade.Boss },
            };
        }

        private ICharacter.Grade ConvertGrade(string grade) => MapGrade.GetValueOrDefault(grade, ICharacter.Grade.None);

        public StruckTableMonster GetDataByUid(int uid)
        {
            if (uid <= 0)
            {
                GcLogger.LogError("uid is 0.");
                return null;
            }
            var data = GetData(uid);
            if (data == null) return null;
            return new StruckTableMonster
            {
                Uid = int.Parse(data["Uid"]),
                Name = data["Name"],
                SpineUid = int.Parse(data["SpineUid"]),
                DefaultSkin = data["DefaultSkin"],
                Scale = float.Parse(data["Scale"]),
                Grade = ConvertGrade(data["Grade"]),
                StatHp = long.Parse(data["StatHp"]),
                StatAtk = long.Parse(data["StatAtk"]),
                StatMoveSpeed = float.Parse(data["StatMoveSpeed"]),
                StatAttackSpeed = float.Parse(data["StatAttackSpeed"]),
                RewardExp = long.Parse(data["RewardExp"]),
                RewardGold = 0,
            };
        }
        
        public GameObject GetPrefab(int uid) {
            var info = GetDataByUid(uid);
            if (info == null) return null;
        
            string prefabPath = TableLoaderManager.Instance.TableAnimation.GetPrefabPath(info.SpineUid);
            if (prefabPath == "") {
                GcLogger.LogError("prefab 경로가 없습니다. SpineUid: "+info.SpineUid);
                return null;
            }
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab != null) return prefab;
            GcLogger.LogError("prefab 오브젝트가 없습니다. prefabPath: "+prefabPath);
            return null;
        }
        public string GetShapePath(int uid)
        {
            var info = GetDataByUid(uid);
            return info == null || info.SpineUid <= 0 ? "" : TableLoaderManager.Instance.TableAnimation.GetPrefabPath(info.SpineUid);
        }
    }
}