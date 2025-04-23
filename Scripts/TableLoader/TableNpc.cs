using System.Collections.Generic;
using UnityEngine;

namespace GGemCo.Scripts
{
    /// <summary>
    /// Npc 테이블 Structure
    /// </summary>
    public class StruckTableNpc
    {
        public int Uid;
        public string Name;
        public int SpineUid;
        public string DefaultSkin;
        public float Scale;
        public CharacterConstants.Grade Grade;
        public int StatMoveSpeed;
        public int InteractionUid;
        public string ImageThumbnailPath;
    }
    /// <summary>
    /// Npc 테이블
    /// </summary>
    public class TableNpc : DefaultTable
    {
        private static readonly Dictionary<string, CharacterConstants.Grade> MapGrade;

        static TableNpc()
        {
            MapGrade = new Dictionary<string, CharacterConstants.Grade>
            {
                { "Common", CharacterConstants.Grade.Common },
                { "Boss", CharacterConstants.Grade.Boss },
            };
        }

        private CharacterConstants.Grade ConvertGrade(string grade) => MapGrade.GetValueOrDefault(grade, CharacterConstants.Grade.None);

        public StruckTableNpc GetDataByUid(int uid)
        {
            if (uid <= 0)
            {
                GcLogger.LogError("uid is 0.");
                return null;
            }
            var data = GetData(uid);
            if (data == null) return null;
            return new StruckTableNpc
            {
                Uid = int.Parse(data["Uid"]),
                Name = data["Name"],
                SpineUid = int.Parse(data["SpineUid"]),
                DefaultSkin = data["DefaultSkin"],
                Scale = float.Parse(data["Scale"]),
                Grade = ConvertGrade(data["Grade"]),
                StatMoveSpeed = int.Parse(data["StatMoveSpeed"]),
                InteractionUid = int.Parse(data["InteractionUid"]),
                ImageThumbnailPath = data["ImageThumbnailPath"],
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
    }
}