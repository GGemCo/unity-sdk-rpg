namespace GGemCo.Scripts.TableLoader
{
    public class TableConfig : DefaultTable
    {
        // enum을 사용하여 컬럼 번호를 명시적으로 나타냄
        private enum ColumnIndex
        {
            PolyPlayerStatAtk = 1, // 플레이어 기본 atk
            PolyPlayerStatMoveStep = 2, // 플레이어 한걸음 이동거리
            PolyPlayerStatMoveSpeed = 3, // 플레이어 기본 speed
            StartMapUid = 4, // 게임 시작 맵
            DefaultMonsterRegenTimeSec = 5, // 디폴트 몬스터 리젠 시간
            MaxEnemyValue = 6, // 공격시 최대 피해 몬스터 개수
            DelayDestroyMonster = 7, // 몬스터 죽었을때 없어지기까지 시간
        }

        // 제네릭 메서드를 사용하여 공통 로직 처리
        private T GetConfigValue<T>(ColumnIndex index)
        {
            string data = GetDataColumn((int)index, "Value");
            if (typeof(T) == typeof(int) && int.TryParse(data, out var intValue))
                return (T)(object)intValue;
            if (typeof(T) == typeof(float) && float.TryParse(data, out var floatValue))
                return (T)(object)floatValue;
            if (typeof(T) == typeof(long) && long.TryParse(data, out var longValue))
                return (T)(object)longValue;

            return (T)(object)data;
        }

        public float GetPolyPlayerStatAtk() => GetConfigValue<float>(ColumnIndex.PolyPlayerStatAtk);
        public int GetPolyPlayerStatMoveStep() => GetConfigValue<int>(ColumnIndex.PolyPlayerStatMoveStep);
        public float GetPolyPlayerStatMoveSpeed() => GetConfigValue<float>(ColumnIndex.PolyPlayerStatMoveSpeed);
        public int GetStartMapUid() => GetConfigValue<int>(ColumnIndex.StartMapUid);
        public float GetDefaultMonsterRegenTimeSec() => GetConfigValue<float>(ColumnIndex.DefaultMonsterRegenTimeSec);
        public long GetMaxEnemyValue() => GetConfigValue<long>(ColumnIndex.MaxEnemyValue);
        public long GetDelayDestroyMonster() => GetConfigValue<long>(ColumnIndex.DelayDestroyMonster);
    }
}