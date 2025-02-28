using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
using R3;
using UnityEngine;

namespace GGemCo.Scripts.SaveData
{
    public class PlayerData : DefaultData, ISaveData
    {
        private readonly int maxPlayerLevel;
        
        public readonly BehaviorSubject<int> CurrentChapter = new BehaviorSubject<int>(1);
        public readonly BehaviorSubject<int> CurrentLevel = new BehaviorSubject<int>(1);
        public readonly BehaviorSubject<long> CurrentExp = new BehaviorSubject<long>(0);
        public readonly BehaviorSubject<long> CurrentNeedExp = new BehaviorSubject<long>(0);

        private readonly TableLoaderManager tableLoaderManager;
        private TableMonster tableMonster;
        private TableExp tableExp;

        private const string PlayerPrefsKeyLevel = "GGemCo_PlayerPrefs_Player_Level";
        private const string PlayerPrefsKeyExp = "GGemCo_PlayerPrefs_Player_Exp";
        public PlayerData(TableLoaderManager loader)
        {
            tableLoaderManager = loader;
            maxPlayerLevel = tableLoaderManager.TableConfig.GetMaxLevel();
        }

        public void Initialize()
        {
            tableMonster = tableLoaderManager.TableMonster;
            tableExp = tableLoaderManager.TableExp;
            
            LoadLevel();
            LoadExp();
        }
        /// <summary>
        /// 저장된 레벨 가져오기, 레벨업 필요한 경험치 설정
        /// </summary>
        private void LoadLevel()
        {
            int level = PlayerPrefsLoadInt(PlayerPrefsKeyLevel, 1);
            SetLevel(level);
            SetNeedExp(tableExp.GetNeedExp(level + 1));
        }
        /// <summary>
        /// 저장된 경험치 가져오기
        /// </summary>
        private void LoadExp()
        {
            SetExp(PlayerPrefsLoadLong(PlayerPrefsKeyExp));
        }
        /// <summary>
        /// 경험치 추가하기
        /// </summary>
        /// <param name="monsterVid"></param>
        /// <param name="monsterUid"></param>
        /// <param name="monsterObject"></param>
        public void AddExp(int monsterVid, int monsterUid, GameObject monsterObject)
        {
            var info = tableMonster.GetDataByUid(monsterUid);
            if (info == null) return;

            long newExp = CurrentExp.Value + info.RewardExp;
            int nextLevel = CurrentLevel.Value + 1;

            if (nextLevel > maxPlayerLevel)
            {
                GcLogger.Log("현재 최대 레벨입니다. maxPlayerLevel :" + maxPlayerLevel);
                return;
            }

            long needExp = tableExp.GetNeedExp(nextLevel);
            while (newExp >= needExp && needExp > 0)
            {
                newExp -= needExp;
                int level = SetLevel(CurrentLevel.Value + 1);
                SetExp(0);

                if (level >= maxPlayerLevel)
                {
                    GcLogger.LogError("현재 최대 레벨입니다. maxPlayerLevel :" + maxPlayerLevel);
                    SetNeedExp(0);
                    break;
                }

                needExp = tableExp.GetNeedExp(level + 1);
                SetNeedExp(needExp);
            }

            SetExp(newExp);
        }
        /// <summary>
        /// 경험치 셋팅
        /// </summary>
        /// <param name="value"></param>
        private void SetExp(long value)
        {
            CurrentExp.OnNext(value);
            PlayerPrefsSaveLong(PlayerPrefsKeyExp, value);
        }
        /// <summary>
        /// 레벨 셋팅
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private int SetLevel(int value)
        {
            CurrentLevel.OnNext(value);
            PlayerPrefsSaveInt(PlayerPrefsKeyLevel, value);
            return CurrentLevel.Value;
        }
        /// <summary>
        /// 필요한 경험치 셋팅
        /// </summary>
        /// <param name="value"></param>
        private void SetNeedExp(long value)
        {
            CurrentNeedExp.OnNext(value);
        }
    }
}
