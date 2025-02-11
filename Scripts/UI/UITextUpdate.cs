using System.Collections;
using GGemCo.Scripts.Core;
using UnityEngine;

namespace GGemCo.Scripts.UI
{
    public class UITextUpdate : MonoBehaviour
    {
        protected SaveDataManager SaveDataManager;
        protected long CurrentValue;
        protected long PreviousValue;
        private bool wasDisabled;
        
        public bool useShortText; // 줄임 단위를 사용할 것인지 

        protected void Awake() {
            wasDisabled = false;
        }
        protected virtual void Start()
        {
            SetPreviosValue();
            SetText(PreviousValue);

            StartCoroutine(nameof(UpdateText));
        }

        protected virtual void SetPreviosValue() {
            PreviousValue = 0;
        }

        protected virtual long GetCurrentValue() {
            return 0;
        }

        private void OnDisable() {
            wasDisabled = true;
        }
        private void OnEnable()
        {
            if (wasDisabled != true) return;
            StartCoroutine(nameof(UpdateText));
            wasDisabled  = false;
        }
        public virtual IEnumerator UpdateText()
        {
            while (true)
            {
                // FG_Logger.Log("UpdateText previousValue: "+previousValue);
                // 필요할 때만 갱신
                CurrentValue =  GetCurrentValue();
                if (CurrentValue != PreviousValue) {
                    SetText(CurrentValue);
                    PreviousValue = CurrentValue;
                }
                yield return new WaitForSeconds(0.3f);
            }
        }

        protected virtual void SetText(long value)
        {
            if (useShortText)
            {
            }
            else
            {
            }
        }
    }
}
