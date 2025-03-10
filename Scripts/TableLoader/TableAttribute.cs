using GGemCo.Scripts.Utils;

namespace GGemCo.Scripts.TableLoader
{
    public class StruckTableAttribute
    {
        public int Uid;
        public string Id;
        public string Name;
    }
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