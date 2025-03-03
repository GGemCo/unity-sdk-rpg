namespace GGemCo.Scripts.TableLoader
{
    public class TableExp : DefaultTable
    {
        public long GetNeedExp(int level) => long.TryParse(GetDataColumn(level, "NeedExp"), out var v) ? v : 0;
    }
}