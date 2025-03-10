using GGemCo.Scripts.Utils;

namespace GGemCo.Scripts.TableLoader
{
    public class StruckTableWindow
    {
        public int Uid;
        public string Name;
        public bool DefaultActive;
    }
    public class TableWindow : DefaultTable
    {
        public StruckTableWindow GetDataByUid(int uid)
        {
            if (uid <= 0)
            {
                GcLogger.LogError("uid is 0.");
                return null;
            }
            var data = GetData(uid);
            if (data == null) return null;
            return new StruckTableWindow
            {
                Uid = int.Parse(data["Uid"]),
                Name = data["Name"],
                DefaultActive = ConvertBoolean(data["DefaultActive"]),
            };
        }
    }
}