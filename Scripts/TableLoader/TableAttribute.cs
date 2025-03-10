using GGemCo.Scripts.Utils;

namespace GGemCo.Scripts.TableLoader
{
    /// <summary>
    /// 속성 테이블 Structure
    /// </summary>
    public class StruckTableAttribute
    {
        public int Uid;
        public string Id;
        public string Name;
    }
    /// <summary>
    /// 속성 테이블
    /// </summary>
    public class TableAttribute : DefaultTable
    {
        public StruckTableAttribute GetDataByUid(int uid)
        {
            if (uid <= 0)
            {
                GcLogger.LogError("uid is 0.");
                return null;
            }
            var data = GetData(uid);
            if (data == null) return null;
            return new StruckTableAttribute
            {
                Uid = int.Parse(data["Uid"]),
                Id = data["Id"],
                Name = data["Name"],
            };
        }
    }
}