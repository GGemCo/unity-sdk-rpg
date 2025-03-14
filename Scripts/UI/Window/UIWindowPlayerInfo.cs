using GGemCo.Scripts.TableLoader;

namespace GGemCo.Scripts.UI.Window
{
    public class UIWindowPlayerInfo : UIWindow
    {
        
        protected override void Awake()
        {
            base.Awake();
            if (TableLoaderManager.Instance == null) return;
            uid = UIWindowManager.WindowUid.PlayerInfo;
        }
        protected override void Start()
        {
            base.Start();
        }
    }
}