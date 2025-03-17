using GGemCo.Scripts.TableLoader;

namespace GGemCo.Scripts.UI.Window
{
    public class UIWindowPlayerInfo : UIWindow
    {
        
        protected override void Awake()
        {
            uid = UIWindowManager.WindowUid.ItemInfo;
            if (TableLoaderManager.Instance == null) return;
            base.Awake();
        }
        protected override void Start()
        {
            base.Start();
        }
    }
}