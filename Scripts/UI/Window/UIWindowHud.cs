using GGemCo.Scripts.Scenes;

namespace GGemCo.Scripts.UI.Window
{
    public class UIWindowHud : UIWindow
    {
        protected override void Awake()
        {
            uid = UIWindowManager.WindowUid.Hud;
            base.Awake();
        }
        /// <summary>
        /// 인벤토리 열기 
        /// </summary>
        public void OnClickShowInventory()
        {
            SceneGame.Instance.uIWindowManager.ShowWindow(UIWindowManager.WindowUid.Inventory, true);
        }
    }
}