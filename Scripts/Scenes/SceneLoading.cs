using System;
using TMPro;

namespace GGemCo.Scripts.Scenes
{
    public class SceneLoading : DefaultScene
    {
        public TextMeshProUGUI textLoadingPercent;

        private void Start() {
            if (textLoadingPercent != null)
            {
                textLoadingPercent.text = "0";
            }
        }
        public void SetTextLoadingPercent(float progress) {
            if (textLoadingPercent != null)
            {
                textLoadingPercent.text = $"데이터 불러오는 중...{Math.Floor(progress)}%";
            }
        }
    }
}