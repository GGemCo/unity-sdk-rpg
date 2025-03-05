namespace GGemCo.Scripts.Configs
{
    /// <summary>
    /// 게임에서 사용되는 tag 관리
    /// </summary>
    public abstract class ConfigTags : DefaultConfig<ConfigTags.Keys>
    {
        public enum Keys
        {
            // 메인 카메라
            MainCamera,
            // 플레이어
            Player,
            // 몬스터
            Monster,
            Npc,
            Map,
            // 맵에 배치되는 오브젝트
            MapObject,
            // 맵에 배치되는 워프
            MapObjectWarp,
            // 타일 맵이 들어가는 부모 grid object
            GridTileMap,
            // 맵 이동시 화면 interaction 을 막는 canvas
            CanvasBlockInteraction,
            // npc 머리 위에 나오는 퀘스트 버튼
            ButtonNpcQuest,
            // 월드 position 으로 보여지는 UI 용 canvas. 예) 데미지 텍스트, npc 이름 
            CanvasUIByWorld,
            // UI가 포함될 canvas
            CanvasUI,
            // 드랍된 아이템 
            DropItem,
            // window manager
            WindowManager
        }
    }
}