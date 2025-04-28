namespace GGemCo.Editor
{
    public class ConfigEditor
    {
        private const string NameToolGGemCo = "GGemCoTool/";
        // 기본 셋팅하기
        public const string NameToolDefaultSetting = NameToolGGemCo + "기본 셋팅하기";
        
        // etc
        private const string NameToolEtc = NameToolGGemCo + "기타/";
        public const string NameToolPlayerPrefs = NameToolEtc + "PlayerPrefs 데이터 관리";
        public const string NameToolOpenSaveDataFolder = NameToolEtc + "게임 데이터 저장 폴더 열기";
        
        // 개발툴
        private const string NameToolDevelopment = NameToolGGemCo + "개발툴/";
        public const string NameToolMapExporter = NameToolDevelopment + "맵배치툴";
        public const string NameToolCutscene = NameToolDevelopment + "연출툴";
        public const string NameToolCreateItem = NameToolDevelopment + "아이템 생성툴";
    }
}