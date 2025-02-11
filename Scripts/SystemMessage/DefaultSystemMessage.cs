using GGemCo.Scripts.Core;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.SystemMessage
{
    public class DefaultSystemMessage : MonoBehaviour
    {
        private int mDelay;
        private int mDuration;
        private Vector3 mMovePosition;

        private void Awake()
        {
        }

        public void Show(string message, int delay, int duration, Vector3 movePosition)
        {
            GcLogger.Log("showNotice " + delay + " / duration : " + duration + " / movePositionY : " + movePosition.y);
        }
    }
}
