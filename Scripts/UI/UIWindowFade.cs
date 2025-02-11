using System.Collections;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.UI
{
    public class UIWindowFade : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private const float FadeDuration = 0.3f;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void StartFadeOut()
        {
            // 패널 비활성화 시 페이드 아웃
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeIn()
        {
            float elapsedTime = 0.0f;
            canvasGroup.alpha = 0.0f;
            
            gameObject.GetComponent<UIWindow>()?.OnShow(true);

            while (elapsedTime < FadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / FadeDuration);
                canvasGroup.alpha = Easing.EaseOutQuintic(t);
                yield return null;
            }

            canvasGroup.alpha = 1.0f;
        }

        private IEnumerator FadeOut()
        {
            float elapsedTime = 0.0f;
            canvasGroup.alpha = 1.0f;

            while (elapsedTime < FadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / FadeDuration);
                canvasGroup.alpha = Easing.EaseInQuintic(1.0f - t);
                yield return null;
            }

            canvasGroup.alpha = 0.0f;

            gameObject.GetComponent<UIWindow>()?.OnShow(false);
            // 페이드 아웃 완료 후 비활성화
            gameObject.SetActive(false);
        }
        public void ShowPanel()
        {
            gameObject.SetActive(true);
            // 패널 활성화 시 페이드 인
            StartCoroutine(FadeIn());
        }

        public void HidePanel()
        {
            // 페이드 아웃을 시작하도록 OnDisable을 호출
            StartFadeOut();
        }
    }
}