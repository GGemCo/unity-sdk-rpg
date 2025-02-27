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
        public float StatHp;
        public float StatAtk;
        public float StatMoveSpeed;
        public float StatAttackSpeed;
        public int RewardExp;
        public int RewardGold;
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
                return new StruckTableMonster();
            }
            var data = GetData(uid);
            return new StruckTableMonster
            {
                Uid = int.Parse(data["Uid"]),
                Name = data["Name"],
                SpineUid = int.Parse(data["SpineUid"]),
                DefaultSkin = data["DefaultSkin"],
                Scale = float.Parse(data["Scale"]),
                Grade = ConvertGrade(data["Grade"]),
                StatHp = float.Parse(data["StatHp"]),
                StatAtk = float.Parse(data["StatAtk"]),
                StatMoveSpeed = float.Parse(data["StatMoveSpeed"]),
                StatAttackSpeed = float.Parse(data["StatAttackSpeed"]),
                RewardExp = 0,
                RewardGold = 0,
            };
        }
        
        public GameObject GetPrefab(int uid) {
            var info = GetDataByUid(uid);
            if (info.SpineUid == 0) return null;
        
            string prefabPath = TableLoaderManager.Instance.TableAnimation.GetPrefabPath(info.SpineUid);
            if (prefabPath == "") {
                GcLogger.Log("prefabPath is ''. shape: "+info.SpineUid);
                return null;
            }
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null) {
                GcLogger.Log("prefab is null. prefabPath: "+prefabPath);
                return null;
            }
            return prefab;
        }
        public string GetShapePath(int uid)
        {
            var info = GetDataByUid(uid);
            return info.SpineUid <= 0 ? "" : TableLoaderManager.Instance.TableAnimation.GetPrefabPath(info.SpineUid);
        }
    }
}