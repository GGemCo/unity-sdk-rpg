using System.Collections.Generic;
using GGemCo.Scripts.Characters;
using GGemCo.Scripts.Utils;

namespace GGemCo.Scripts.TableLoader
{
    public class StruckTableNpc
    {
        public int Uid;
        public string Name;
        public int SpineUid;
        public string DefaultSkin;
        public float Scale;
        public ICharacter.Grade Grade;
        public float StatMoveSpeed;
    }
    public class TableNpc : DefaultTable
    {
        private static readonly Dictionary<string, ICharacter.Grade> MapGrade;

        static TableNpc()
        {
            MapGrade = new Dictionary<string, ICharacter.Grade>
            {
                { "Common", ICharacter.Grade.Common },
                { "Boss", ICharacter.Grade.Boss },
            };
        }

        private ICharacter.Grade ConvertGrade(string grade) => MapGrade.GetValueOrDefault(grade, ICharacter.Grade.None);

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
            };
        }
    }
}