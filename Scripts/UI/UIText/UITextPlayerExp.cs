using GGemCo.Scripts.SaveData;
using GGemCo.Scripts.Scenes;
using R3;
using TMPro;
using UnityEngine;

namespace GGemCo.Scripts.UI.UIText
{
    /// <summary>
    /// EXP 데이터 변경 시 UI 업데이트
    /// </summary>
    public class UITextPlayerExp : MonoBehaviour
    {
        private TextMeshProUGUI textExp;
        private PlayerData playerData;

        private void Awake()
        {
            textExp = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            playerData = SceneGame.Instance.saveDataManager.Player;
            playerData.CurrentExp
                .Merge(playerData.CurrentNeedExp) // 두 개의 Observable을 합침
                .Subscribe(_ => UpdateExpText()) // 값이 변경될 때마다 UI 업데이트
                .AddTo(this);
        }

        /// <summary>
        /// 경험치 UI를 업데이트하는 메서드
        /// </summary>
        private void UpdateExpText()
        {
            textExp.text = $"{playerData.CurrentExp.Value}/{playerData.CurrentNeedExp.Value}";
        }
    }
}