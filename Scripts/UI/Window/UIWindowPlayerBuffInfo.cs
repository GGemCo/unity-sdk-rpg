using System.Collections;
using System.Collections.Generic;
using GGemCo.Scripts.Characters;
using GGemCo.Scripts.UI.Icon;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.UI.Window
{
    /// <summary>
    /// 플레이어 버프 관리
    /// </summary>
    public class UIWindowPlayerBuffInfo : UIWindow
    {
        public GameObject prefabIconBuff;
        private List<GameObject> iconBuffs = new List<GameObject>();
        protected override void Awake()
        {
            base.Awake();
            iconBuffs.Clear();
            uid = UIWindowManager.WindowUid.PlayerBuffInfo;
        }
        /// <summary>
        /// 버프 적용하기
        /// </summary>
        /// <param name="struckBuff"></param>
        public void AddBuff(StruckBuff struckBuff)
        {
            if (struckBuff == null) return;
            // GcLogger.Log("RemoveBuffAfterDuration " + struckBuff.Uid + "/"+struckBuff.Name+"/"+struckBuff.Duration);
            GameObject iconBuff = Instantiate(prefabIconBuff, containerIcon.transform);
            iconBuff.GetComponentInChildren<UIIconBuff>()?.Initialize(struckBuff);;
            iconBuffs.Add(iconBuff);
            StartCoroutine(RemoveBuffAfterDuration(struckBuff, iconBuff));
        }
        /// <summary>
        /// 버프 제거 하기
        /// </summary>
        /// <param name="struckBuff"></param>
        /// <param name="iconBuff"></param>
        /// <returns></returns>
        private IEnumerator RemoveBuffAfterDuration(StruckBuff struckBuff, GameObject iconBuff)
        {
            // GcLogger.Log("RemoveBuffAfterDuration " + struckBuff.Uid + "/"+struckBuff.Name+"/"+struckBuff.Duration);
            float duration = struckBuff.Duration;
            yield return new WaitForSeconds(duration);
            iconBuffs.Remove(iconBuff);
            DestroyImmediate(iconBuff);
        }
    }
}