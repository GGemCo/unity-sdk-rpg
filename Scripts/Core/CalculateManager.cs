using GGemCo.Scripts.Characters.Player;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.Core
{
    public class CalculateManager : MonoBehaviour
    {
        private void Start()
        {
        }

        protected long GetPlayerWeaponAtk()
        {
            return 0;
        }
        /// <summary>
        /// 플레이어 공격력 구하기
        /// </summary>
        /// <returns></returns>
        public long GetPlayerTotalAtk()
        {
            return (long)SceneGame.Instance.player.GetComponent<Player>().PlayerStat.TotalAtk;
        }
        /// <summary>
        /// 몬스터 공격력 구하기
        /// </summary>
        /// <param name="monsterUid"></param>
        /// <returns></returns>
        public long GetMonsterTotalAtk(int monsterUid)
        {
            if (monsterUid < 0)
            {
                GcLogger.LogError("몬스터 고유번호가 없습니다. Uid:"+monsterUid);
                return 0;
            }
            var info = TableLoaderManager.Instance.TableMonster.GetDataByUid(monsterUid);
            if (info == null || info.Uid <= 0)
            {
                GcLogger.LogError("몬스터 테이블에 정보가 없습니다. Uid:"+monsterUid);
                return 0;
            }

            return (long)info.StatAtk;
        }
    }
}