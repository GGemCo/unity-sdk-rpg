using System.Collections;
using System.Collections.Generic;
using GGemCo.Scripts.TableLoader;
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
        private Dictionary<int, GameObject> iconBuffs = new Dictionary<int, GameObject>();
        protected override void Awake()
        {
            // uid 를 먼저 지정해야 한다.
            uid = UIWindowManager.WindowUid.PlayerBuffInfo;
            base.Awake();
            iconBuffs.Clear();
        }
        /// <summary>
        /// 버프 적용하기
        /// </summary>
        /// <param name="affectUid"></param>
        public void AddAffect(int affectUid)
        {
            var info = TableLoaderManager.Instance.TableAffect.GetDataByUid(affectUid);
            if (info == null)
            {
                GcLogger.LogError("affect 테이블에 없는 어펙트 입니다. affect Uid: "+affectUid);
                return;
            }
            // 적용되어 있는 버프면, duration 초기화 하기
            if (iconBuffs.ContainsKey(affectUid))
            {
                var icon = iconBuffs.GetValueOrDefault(affectUid);
                if (icon != null)
                {
                    Destroy(icon);
                }
                iconBuffs.Remove(affectUid);
            }
            // GcLogger.Log("RemoveBuffAfterDuration " + struckBuff.Uid + "/"+struckBuff.Name+"/"+struckBuff.Duration);
            GameObject iconBuff = Instantiate(prefabIconBuff, containerIcon.transform);
            iconBuff.GetComponentInChildren<UIIconBuff>()?.Initialize(affectUid);
            iconBuffs.Add(affectUid, iconBuff);
            StartCoroutine(RemoveBuffAfterDuration(affectUid, info.Duration, iconBuff));
        }
        /// <summary>
        /// 버프 제거 하기
        /// </summary>
        /// <param name="affectUid"></param>
        /// <param name="duration"></param>
        /// <param name="iconBuff"></param>
        /// <returns></returns>
        private IEnumerator RemoveBuffAfterDuration(int affectUid, float duration, GameObject iconBuff)
        {
            // GcLogger.Log("RemoveBuffAfterDuration " + struckBuff.Uid + "/"+struckBuff.Name+"/"+struckBuff.Duration);
            yield return new WaitForSeconds(duration);
            iconBuffs.Remove(affectUid);
            Destroy(iconBuff);
        }
    }
}