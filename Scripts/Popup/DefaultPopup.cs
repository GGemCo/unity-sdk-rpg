using System.Collections;
using System.Collections.Generic;
using GGemCo.Scripts.Core;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGemCo.Scripts.Popup
{
    public class PopupMetadata
    {
        public PopupType PopupType = PopupType.NormalReward;
        public string Title = "";
        public string Message = "";
        public bool ShowConfirmButton = true;
        public bool ShowCancelButton = false;
        public System.Action OnConfirm = null;
        public System.Action OnCancel = null;
        public bool ShowBgBlack = false;
        public bool DontShowCoolTimeGauage = false;
        public bool ForceShow = false;
        public long RewardDia = 0;
        public long RewardGold = 0;
        public bool IsClosableByClick = true;
        public Color MessageColor = Color.white;
        public Dictionary<int, int> DictionaryRewardItems = new Dictionary<int, int>();
    }
    
    // TextMeshPro 사용을 위해
    public class DefaultPopup : MonoBehaviour, IPointerClickHandler
    {
        protected PopupType PopupType;
        public Button buttonConfirm; // 확인 버튼
        public Button buttonCancel; // 취소 버튼
        protected CanvasGroup CanvasGroup; // 페이드 인/아웃을 위한 CanvasGroup
        public RectTransform panelContent; // 내용, 보상 아이템이 들어가는 패널

        protected bool IsClosableByClick;
        public float autoCloseTime; // 자동 닫힘 시간을 저장할 변수
        public Image gaugeBarAutoClose;
        
        public float fadeDuration;

        private void Awake()
        {
            IsClosableByClick = true;
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Initialize(PopupMetadata popupMetadata)
        {
            if (popupMetadata.PopupType != PopupType.None)
            {
                PopupType = popupMetadata.PopupType;
            }

            SetRewardDiaGold(popupMetadata.RewardDia, popupMetadata.RewardGold);

            if (popupMetadata.DictionaryRewardItems != null && SceneGame.Instance != null)
            {
                PopupManager popupManager = SceneGame.Instance.popupManager;
                if (popupManager.elementRewardItem != null && popupMetadata.DictionaryRewardItems.Count > 0)
                {
                    panelContent.gameObject.SetActive(true);
                    foreach (var info in popupMetadata.DictionaryRewardItems)
                    {
                        GameObject element = Instantiate(popupManager.elementRewardItem, panelContent);
                    }
                }
            }

            if (popupMetadata.ShowConfirmButton && buttonConfirm == null)
            {
                GcLogger.LogError("dont exist confirm button.");
            }
            if (buttonConfirm != null)
            {
                buttonConfirm.gameObject.SetActive(popupMetadata.ShowConfirmButton);
                if (popupMetadata.ShowConfirmButton)
                {
                    buttonConfirm.onClick.AddListener(() => {
                        if (popupMetadata.OnConfirm != null)
                        {
                            popupMetadata.OnConfirm.Invoke();
                        }
                        ClosePopup();
                    });
                }
            }

            if (popupMetadata.ShowCancelButton && buttonCancel == null)
            {
                GcLogger.LogError("dont exist cacnel button.");
            }
            if (buttonCancel != null)
            {
                buttonCancel.gameObject.SetActive(popupMetadata.ShowCancelButton);
                if (popupMetadata.ShowCancelButton)
                {
                    buttonCancel.onClick.AddListener(() => {
                        if (popupMetadata.OnCancel != null)
                        {
                            popupMetadata.OnCancel.Invoke();
                        }
                        ClosePopup();
                    });
                }
            }

            IsClosableByClick = popupMetadata.IsClosableByClick;

            // 확인, 닫기 버튼이 있는 경우는 선택해야 되기 때문에 자동으로 닫하지 않는다.
            if (popupMetadata.DontShowCoolTimeGauage || (popupMetadata.ShowConfirmButton && popupMetadata.ShowCancelButton))
            {
                autoCloseTime = -1;
                if (gaugeBarAutoClose != null)
                {
                    Destroy(gaugeBarAutoClose.transform.parent.gameObject);
                }
            }
            
            if (autoCloseTime > 0f)
            {
                if (gaugeBarAutoClose == null)
                {
                    // FgLogger.LogError("dont exist gauge image.");
                }
                else
                {
                    gaugeBarAutoClose.gameObject.SetActive(true);
                    gaugeBarAutoClose.fillAmount = 1;
                }

                StartCoroutine(AutoCloseCoroutine());
            }
            else
            {
                if (gaugeBarAutoClose != null)
                {
                    gaugeBarAutoClose.gameObject.SetActive(false);
                }
            }
            if (popupMetadata.ShowBgBlack)
            {
            }
            
            // 레이아웃 업데이트
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelContent);
        }

        public void SetRewardDiaGold(long rewardDia, long rewardGold)
        {
        }

        public void ShowPopup()
        {
            StartCoroutine(FadeIn());
        }

        public virtual void ClosePopup()
        {
            StartCoroutine(FadeOutAndDestroy());
        }

        protected virtual void OnFadeInStart()
        {
            
        }
        protected virtual void OnFadeInEnd()
        {
            CanvasGroup.alpha = 1.0f;
        }
        private IEnumerator FadeIn()
        {
            float elapsedTime = 0.0f;
            CanvasGroup.alpha = 0.0f;

            OnFadeInStart();
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / fadeDuration);
                CanvasGroup.alpha = Easing.EaseOutQuintic(t);
                yield return null;
            }
            OnFadeInEnd();
        }

        protected IEnumerator FadeOutAndDestroy()
        {
            float elapsedTime = 0.0f;
            CanvasGroup.alpha = 1.0f;

            OnFadeOutDestroyStart();
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / fadeDuration);
                CanvasGroup.alpha = Easing.EaseInQuintic(1.0f - t);
                yield return null;
            }

            OnFadeOutDestroyEnd();
        }
        
        protected virtual void OnFadeOutDestroyStart()
        {
        }
        protected virtual void OnFadeOutDestroyEnd()
        {
            CanvasGroup.alpha = 0.0f;
            Destroy(gameObject);
        }

        protected IEnumerator AutoCloseCoroutine()
        {
            if (gaugeBarAutoClose != null)
            {
                float elapsedTime = 0f;
                while (elapsedTime < autoCloseTime)
                {
                    elapsedTime += Time.deltaTime;
                    gaugeBarAutoClose.fillAmount = 1 - Mathf.Clamp01(elapsedTime / autoCloseTime);
                    yield return null;
                }

                gaugeBarAutoClose.fillAmount = 0;
            }
            else
            {
                yield return new WaitForSeconds(autoCloseTime);
            }

            ClosePopup();
        }
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (IsClosableByClick)
            {
                CanvasGroup.alpha = 0.0f;
                
                Destroy(gameObject);
            }
        }

        private void OnDisable()
        {
            if (buttonConfirm != null)
            {
                buttonConfirm.onClick.RemoveAllListeners();
            }

            if (buttonCancel != null)
            {
                buttonCancel.onClick.RemoveAllListeners();
            }
        }

        public void SetType(PopupType type)
        {
            PopupType = type;
        }
    }
}