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
                return new StruckTableWindow();
            }
            var data = GetData(uid);
            return new StruckTableWindow
            {
                Uid = int.Parse(data["Uid"]),
                Name = data["Name"],
                DefaultActive = ConvertBoolean(data["DefaultActive"]),
            };
        }
    }
}