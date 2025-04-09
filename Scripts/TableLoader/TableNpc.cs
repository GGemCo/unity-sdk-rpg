using System.Collections.Generic;

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
        public CharacterBase.Grade Grade;
        public float StatMoveSpeed;
        public int InteractionUid;
        public string ImageThumbnailPath;
    }
    /// <summary>
    /// Npc 테이블
    /// </summary>
    public class TableNpc : DefaultTable
    {
        private static readonly Dictionary<string, CharacterBase.Grade> MapGrade;

        static TableNpc()
        {
            MapGrade = new Dictionary<string, CharacterBase.Grade>
            {
                { "Common", CharacterBase.Grade.Common },
                { "Boss", CharacterBase.Grade.Boss },
            };
        }

        private CharacterBase.Grade ConvertGrade(string grade) => MapGrade.GetValueOrDefault(grade, CharacterBase.Grade.None);

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
                StatMoveSpeed = float.Parse(data["StatMoveSpeed"]),
                InteractionUid = int.Parse(data["InteractionUid"]),
                ImageThumbnailPath = data["ImageThumbnailPath"],
            };
        }
    }
}