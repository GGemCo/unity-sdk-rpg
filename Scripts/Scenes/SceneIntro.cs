using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo.Scripts.Scenes
{
    public class SceneIntro : MonoBehaviour
    {
        public Button buttonGameStart;

        void Awake()
        {
            InitButtons();
        }

        void InitButtons()
        {
            if (buttonGameStart == null) return;
            buttonGameStart.onClick.AddListener(OnClickGameStart);
        }
        void OnClickGameStart()
        {
            SceneManager.ChangeScene(ConfigDefine.SceneNameLoading);
        }
    }
}
