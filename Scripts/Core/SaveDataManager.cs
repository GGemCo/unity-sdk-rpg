using System.Collections.Generic;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.Core
{
    public class SaveDataManager : MonoBehaviour
    {
        // 최대 장수
        private int maxChapter;
        // 챕터 1개당 스테이지 개수
        private int maxStage;

        private int currentLevel;
        public int CurrentLevel {
            get { return currentLevel; }
        }
        private long currentExp;
        public long CurrentExp {
            get { return currentExp; }
        }
        private long currentMaxExp;
        public long CurrentMaxExp {
            get { return currentMaxExp; }
        }
        private long currentGold;
        public long CurrentGold {
            get { return currentGold; }
        }
        private long currentDia;
        public long CurrentDia {
            get { return currentDia; }
        }
        private int currentChapter;
        public int CurrentChapter {
            get { return currentChapter; }
        }
        private int currentStage;
        public int CurrentStage {
            get { return currentStage; }
        }
        public int CurrentLastChapter { get; private set; }
        public int CurrentLastStage { get; private set; }
        
        private readonly Dictionary<int, int> currentItemCount = new Dictionary<int, int>();
        
        private int gradeLevelMax;
        
        private void Awake() {
            Initialize();
            InitializeServerData();
        }
        private void Initialize()
        {
            currentGold = 0;
            currentDia = 0;
            currentChapter = 1;
            currentStage = 1;
            currentLevel = 1;
            currentExp = 0;
        }

        private int maxPlayerLevel;

        private void Start()
        {
        }
        /// <summary>
        /// 서버에서 받은 데이터 적용하기
        /// </summary>
        private void InitializeServerData()
        {
        }
        /// <summary>
        /// 골드 저장하기
        /// </summary>
        /// <returns></returns>
        public long AddGold(long value, string itemName = "")
        {
            // FG_Logger.Log("SaveGold:" + value);
            currentGold += value;
            return currentGold;
        }
        /// <summary>
        /// 다이아 저장하기
        /// </summary>
        /// <returns></returns>
        public long AddDia(long value, string itemName = "")
        {
            currentDia += value;
            return currentDia;
        }
        /// <summary>
        /// 챕터 저장하기
        /// </summary>
        /// <returns></returns>
        public int SetChapter(int value)
        {
            if (value <= 0) return currentChapter;
            currentChapter = value;
            return currentChapter;
        }
        /// <summary>
        /// 스테이지 저장하기
        /// </summary>
        /// <returns></returns>
        public int SetStage(int value)
        {
            // FG_Logger.Log("SetStage value: " + value);
            if (value <= 0) return currentStage;
            currentStage = value;
            return currentStage;
        }
        /// <summary>
        /// 챕터 저장하기
        /// </summary>
        /// <returns></returns>
        public int SetLastChapter(int value)
        {
            if (value <= 0) return CurrentLastChapter;
            CurrentLastChapter = value;
            return CurrentLastChapter;
        }
        /// <summary>
        /// 스테이지 저장하기
        /// </summary>
        /// <returns></returns>
        public int SetLastStage(int value)
        {
            // FG_Logger.Log("SetStage value: " + value);
            if (value <= 0) return CurrentLastStage;
            CurrentLastStage = value;
            return CurrentLastStage;
        }
        /// <summary>
        /// 보스 클리어 했을때, 다음 스테이지로 이동하기
        /// </summary>
        /// <returns></returns>
        public int SetNextStage()
        {
            // FG_Logger.Log("GoNextStage CurrentChapter: "+CurrentChapter+" / CurrentStage: "+CurrentStage);
            int valueStage = currentStage + 1;
            int valueChapter = currentChapter;
        
            if (valueStage > maxStage) 
            {
                valueChapter = currentChapter + 1;
                valueStage = 1;
                if (valueChapter > maxChapter)
                {
                    valueChapter = 1;
                }
                else
                {
                }
                // 맵 바꾸는건 PopupBossChallenge 에서 처리 
            }
            SetChapter(valueChapter);
            SetStage(valueStage);
            SetLastChapter(valueChapter);
            SetLastStage(valueStage);
            // FG_Logger.Log("GoNextStage valueChapter: "+valueChapter+" / valueStage: "+valueStage);
            return currentStage;
        }
        /// <summary>
        /// 경험치 추가하기
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public long AddExp(long value)
        {
            currentExp = currentExp + value;
            
            // 다음 레벨에 필요한 경험치 가져오기
            int nextLevel = currentLevel + 1;
            // 최대 레벨 일때 처리
            if (nextLevel > maxPlayerLevel)
            {
                GcLogger.Log("current Level is max. maxPlayerLevel :"+maxPlayerLevel);
                return 0;
            }
            
            return currentExp;
        }
        /// <summary>
        /// 캐릭터 레벨 셋팅
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int SetLevel(int value)
        {
            currentLevel = value;
            return currentLevel;
        }
        /// <summary>
        /// 현재 다음 레벨업에 필요한 경험치 셋팅하기
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private long SetMaxExp(long value)
        {
            currentMaxExp = value;
            return currentMaxExp;
        }
        /// <summary>
        /// 아이템 개수 더해주기
        /// </summary>
        /// <param name="itemUid"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public (int itemUid, int count) AddItemCount(int itemUid, int value)
        {
            if (itemUid <= 0) return (0, 0);
            currentItemCount.TryAdd(itemUid, 0);
            currentItemCount[itemUid] += value;
            return (itemUid, currentItemCount[itemUid]);
        }
        /// <summary>
        /// 아이템 개수 가져오기
        /// </summary>
        /// <param name="itemUid"></param>
        /// <returns></returns>
        public int GetItemCountByUid(int itemUid)
        {
            return currentItemCount.GetValueOrDefault(itemUid, 0);
        }
    }
}