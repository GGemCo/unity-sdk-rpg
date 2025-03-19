using GGemCo.Scripts.Scenes;
using TMPro;
using UnityEngine.UI;

namespace GGemCo.Scripts.UI.Window
{
    public class UIWindowHud : UIWindow
    {
        // exp 는 UITextPlayerExp 에서 처리한다.
        public Slider sliderHp;
        public Slider sliderMp;
        
        public TextMeshProUGUI textHp;
        public TextMeshProUGUI textMp;
        
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
        public void SetSliderHp(long currentValue, long totalHp)
        {
            sliderHp.value = (float)currentValue / totalHp;
            textHp.text = $"{currentValue} / {totalHp}";
        }
        public void SetSliderMp(long currentValue, long totalMp)
        {
            sliderMp.value = (float)currentValue / totalMp;
            textMp.text = $"{currentValue} / {totalMp}";
        }
    }
}