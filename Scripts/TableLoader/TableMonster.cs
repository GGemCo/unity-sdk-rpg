using System.Collections.Generic;
using UnityEngine;

namespace GGemCo.Scripts
{
    /// <summary>
    /// 몬스터 테이블 Structure
    /// </summary>
    public class StruckTableMonster
    {
        public int Uid;
        public string Name;
        public int SpineUid;
        public string DefaultSkin;
        public float Scale;
        public CharacterBase.Grade Grade;
        public int Level;
        public int StatHp;
        public int StatAtk;
        public int StatDef;
        public int StatMoveSpeed;
        public int StatAttackSpeed;
        public long RewardExp;
        public int RewardGold;
        public int RegistFire;
        public int RegistCold;
        public int RegistLightning;
    }
    /// <summary>
    /// 몬스터 테이블
    /// </summary>
    public class TableMonster : DefaultTable
    {
        private static readonly Dictionary<string, CharacterBase.Grade> MapGrade;

        static TableMonster()
        {
            MapGrade = new Dictionary<string, CharacterBase.Grade>
            {
                { "Common", CharacterBase.Grade.Common },
                { "Boss", CharacterBase.Grade.Boss },
            };
        }

        private CharacterBase.Grade ConvertGrade(string grade) => MapGrade.GetValueOrDefault(grade, CharacterBase.Grade.None);

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
                Level = int.Parse(data["Level"]),
                StatHp = int.Parse(data["StatHp"]),
                StatAtk = int.Parse(data["StatAtk"]),
                StatDef = int.Parse(data["StatDef"]),
                StatMoveSpeed = int.Parse(data["StatMoveSpeed"]),
                StatAttackSpeed = int.Parse(data["StatAttackSpeed"]),
                RewardExp = long.Parse(data["RewardExp"]),
                RegistFire = int.Parse(data["RegistFire"]),
                RegistCold = int.Parse(data["RegistCold"]),
                RegistLightning = int.Parse(data["RegistLightning"]),
                RewardGold = int.Parse(data["RewardGold"]),
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