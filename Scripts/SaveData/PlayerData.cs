using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using R3;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace GGemCo.Scripts.SaveData
{
    public class PlayerData : DefaultData, ISaveData
    {
        private int maxPlayerLevel;
        private TableLoaderManager tableLoaderManager;
        private readonly CompositeDisposable disposables = new CompositeDisposable();
        
        private readonly BehaviorSubject<int> currentChapter = new(1);
        private readonly BehaviorSubject<int> currentLevel = new(1);
        private readonly BehaviorSubject<long> currentExp = new(0);
        private readonly BehaviorSubject<long> currentNeedExp = new(0);
        
        public int CurrentChapter
        {
            get => currentChapter.Value;
            set => currentChapter.OnNext(value);
        }

        public int CurrentLevel
        {
            get => currentLevel.Value;
            set => currentLevel.OnNext(value);
        }

        public long CurrentExp
        {
            get => currentExp.Value;
            set => currentExp.OnNext(value);
        }
        // json 에 포함되지 않도록 함수화 
        public Observable<int> OnCurrentLevelChanged()
        {
            return currentLevel.DistinctUntilChanged();
        }

        public Observable<int> OnCurrentChapterChanged()
        {
            return currentChapter.DistinctUntilChanged();
        }

        public Observable<long> OnCurrentExpChanged()
        {
            return currentExp.DistinctUntilChanged();
        }

        public Observable<long> OnCurrentNeedExpChanged()
        {
            return currentNeedExp.DistinctUntilChanged();
        }

        [JsonIgnore] public long CurrentNeedExp => currentNeedExp.Value;

        private TableMonster tableMonster;
        private TableExp tableExp;

        /// <summary>
        /// 초기화 (저장된 데이터를 불러오거나 새로운 데이터 생성)
        /// </summary>
        public void Initialize(TableLoaderManager loader, SaveDataContainer saveDataContainer = null)
        {
            tableLoaderManager = loader;
            maxPlayerLevel = AddressableSettingsLoader.Instance.playerSettings.maxLevel;
            // 최대 레벨이 없을때는 경험치 테이블에서 가져온다
            if (maxPlayerLevel <= 0)
            {
                maxPlayerLevel = loader.TableExp.GetLastLevel();
            }

            tableMonster = tableLoaderManager.TableMonster;
            tableExp = tableLoaderManager.TableExp;

            // 저장된 데이터가 있을 경우 불러오기
            LoadPlayerData(saveDataContainer);

            // 저장 이벤트 구독
            InitializeSubscriptions();
        }

        /// <summary>
        /// 변경 감지를 통해 자동으로 저장
        /// </summary>
        private void InitializeSubscriptions()
        {
            currentLevel.DistinctUntilChanged()
                .CombineLatest(currentChapter, currentExp, (_, _, _) => Unit.Default)
                .Subscribe(_ => SavePlayerData())
                .AddTo(disposables);
        }

        /// <summary>
        /// 데이터 저장
        /// </summary>
        private void SavePlayerData()
        {
            SceneGame.Instance.saveDataManager.StartSaveData();
        }

        /// <summary>
        /// 저장된 데이터를 불러와서 적용
        /// </summary>
        private void LoadPlayerData(SaveDataContainer saveDataContainer)
        {
            if (saveDataContainer?.PlayerData != null)
            {
                CurrentChapter = saveDataContainer.PlayerData.CurrentChapter;
                CurrentLevel = saveDataContainer.PlayerData.CurrentLevel;
                CurrentExp = saveDataContainer.PlayerData.CurrentExp;
            }

            // 필요 경험치 업데이트
            UpdateRequiredExp(tableExp.GetNeedExp(CurrentLevel + 1));
        }
        /// <summary>
        /// 몬스터 처치 시 경험치 추가
        /// </summary>
        /// <param name="monsterVid"></param>
        /// <param name="monsterUid"></param>
        /// <param name="monsterObject"></param>
        public void AddExp(int monsterVid, int monsterUid, GameObject monsterObject)
        {
            var monsterData = tableMonster.GetDataByUid(monsterUid);
            if (monsterData == null) return;

            long newExp = CurrentExp + monsterData.RewardExp;
            int nextLevel = CurrentLevel;
            while (nextLevel < maxPlayerLevel && newExp >= tableExp.GetNeedExp(nextLevel + 1))
            {
                newExp -= tableExp.GetNeedExp(nextLevel + 1);
                nextLevel++;
            }

            // 최종 값 업데이트
            CurrentLevel = Mathf.Min(nextLevel, maxPlayerLevel);
            CurrentExp = nextLevel < maxPlayerLevel ? newExp : 0;
            UpdateRequiredExp(nextLevel < maxPlayerLevel ? tableExp.GetNeedExp(nextLevel + 1) : 0);
        }

        /// <summary>
        /// 필요 경험치 업데이트
        /// </summary>
        private void UpdateRequiredExp(long value)
        {
            currentNeedExp.OnNext(value);
        }
    }
}
