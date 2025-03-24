using UnityEngine;
using UnityEngine.UI;

namespace GGemCo.Scripts.UI.Icon
{
    public class UICoolTimeHandler : MonoBehaviour
    {
        // 현재 쿨타임 시간
        private float currentCoolTime;
        // true : 시계 반대 방향으로 쿨타임이 표시된다. 
        private bool isReverseFillAmount;
        // true : 현재 쿨타임이 진행중이다.
        private bool isPlayingCoolTime;
        // 쿨타임 시간 (초)
        private float coolTimeDuration;
        // 쿨타임 게이지 이미지
        private Image coolTimeGauge;

        private UIIcon icon;

        private void Awake()
        {
            icon = GetComponent<UIIcon>();
            coolTimeGauge = icon.imageCoolTimeGauge;

            if (coolTimeGauge != null)
            {
                coolTimeGauge.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            UpdateCoolTime();
        }

        public bool PlayCoolTime()
        {
            if (isPlayingCoolTime || coolTimeDuration <= 0) return false;

            isPlayingCoolTime = true;
            currentCoolTime = coolTimeDuration;

            if (coolTimeGauge != null)
            {
                coolTimeGauge.fillAmount = isReverseFillAmount ? 0 : 1;
                coolTimeGauge.gameObject.SetActive(true);
            }
            return true;
        }

        private void UpdateCoolTime()
        {
            if (!isPlayingCoolTime) return;

            currentCoolTime -= Time.deltaTime;
            if (currentCoolTime <= 0)
            {
                EndCoolTime();
                return;
            }

            if (coolTimeGauge != null)
            {
                coolTimeGauge.fillAmount = isReverseFillAmount
                    ? 1 - currentCoolTime / coolTimeDuration
                    : currentCoolTime / coolTimeDuration;
            }
        }

        public void InitializeCoolTime()
        {
            isPlayingCoolTime = false;
            currentCoolTime = 0;

            if (coolTimeGauge != null)
            {
                coolTimeGauge.gameObject.SetActive(false);
            }
        }

        private void EndCoolTime()
        {
            isPlayingCoolTime = false;
            currentCoolTime = 0;

            if (coolTimeGauge != null)
            {
                coolTimeGauge.gameObject.SetActive(false);
            }
        }

        public void SetCoolTime(float time) => coolTimeDuration = time;

        public float GetCurrentCoolTime() => currentCoolTime;

        public void SetRemainCoolTime(float remainCoolTime)
        {
            if (remainCoolTime <= 0) return;

            isPlayingCoolTime = true;
            currentCoolTime = remainCoolTime;

            if (coolTimeGauge != null)
            {
                coolTimeGauge.gameObject.SetActive(true);
            }
        }
    }
}
