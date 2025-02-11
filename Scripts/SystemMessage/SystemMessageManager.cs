using UnityEngine;

namespace GGemCo.Scripts.SystemMessage
{
    public class SystemMessageManager : MonoBehaviour
    {
        public GameObject prefabNotice;
        public Vector3 noticeOriginPosition;

        public void ShowNotice(string message, int delay, int duration, Vector3 movePosition)
        {
            GameObject mainCanvas = GameObject.Find("Canvas");
            GameObject mprefabNotice = Instantiate(prefabNotice, mainCanvas.transform, false);
            mprefabNotice.transform.localPosition = noticeOriginPosition;
            mprefabNotice.GetComponent<DefaultSystemMessage>().Show(message, delay, duration, movePosition);
        }
    }
}
