using GGemCo.Scripts.Scenes;
using R3;
using TMPro;
using UnityEngine;

namespace GGemCo.Scripts.UI.UIText
{
    /// <summary>
    /// Level 데이터 변경 시 UI 업데이트
    /// </summary>
    public class UITextPlayerLevel : MonoBehaviour
    {
        private TextMeshProUGUI levelText;
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        private void Awake()
        {
            levelText = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            SceneGame.Instance.saveDataManager.Player.CurrentLevel
                .Subscribe(newLevel =>
                {
                    levelText.text = $"Lv. {newLevel}";
                })
                .AddTo(disposables);
        }
        private void OnDestroy()
        {
            disposables.Dispose();
        }
    }
}