using System;
using System.Collections.Generic;
using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Items;
using GGemCo.Scripts.Maps.Objects;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.ScriptableSettings;
using GGemCo.Scripts.Spine2d;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI;
using GGemCo.Scripts.UI.Window;
using GGemCo.Scripts.Utils;
using UnityEngine;
using R3;

namespace GGemCo.Scripts.Characters.Player
{
    /// <summary>
    /// 플레이어 
    /// </summary>
    public class Player : CharacterBase
    {
        // 공격할 몬스터 
        private GameObject targetMonster;
        // 주변에 npc 가 있는지 체크 
        private bool isNpcNearby;
        private GGemCoSettings gGemCoSettings; 
        private EquipController equipController;
        private ControllerPlayer controllerPlayer;
        private UIWindowHud uiWindowHud;

        [Serializable]
        private struct StatUIBinding
        {
            public UIWindowPlayerInfo.IndexPlayerInfo textUI;
            public Func<Player, BehaviorSubject<long>> GetStat;
            public string label;
        }
        private readonly List<StatUIBinding> statBindings = new();
        protected override void Awake()
        {
            base.Awake();
            isNpcNearby = false;
        }
        protected override void Start()
        {
            base.Start();
            
            uiWindowHud = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowHud>(UIWindowManager.WindowUid.Hud);
            
            // TotalHp, Mp 가 바뀌어도 현재 값이 바뀌면 안된다.
            TotalHp
                .Subscribe(_ => SetWindowHudSliderHp(CurrentHp.Value))
                .AddTo(this);
            CurrentHp
                .Subscribe(_ => SetWindowHudSliderHp(CurrentHp.Value))
                .AddTo(this);
            TotalMp
                .Subscribe(_ => SetWindowHudSliderMp(CurrentMp.Value))
                .AddTo(this);
            CurrentMp
                .Subscribe(_ => SetWindowHudSliderMp(CurrentMp.Value))
                .AddTo(this);
            TotalMoveSpeed
                .Subscribe(UpdateAnimationMoveTimeScale)
                .AddTo(this);

            LoadEquipItems();
            InitializeStatBindings();
        }
        /// <summary>
        /// tag, sorting layer, layer 셋팅하기
        /// </summary>
        public override void InitTagSortingLayer()
        {
            base.InitTagSortingLayer();
            tag = ConfigTags.GetValue(ConfigTags.Keys.Player);
        }
        /// <summary>
        /// 캐릭터에 필요한 컴포넌트 추가하기
        /// </summary>
        protected override void InitComponents()
        {
            // AddComponent 순서 중요
            base.InitComponents();
            controllerPlayer = gameObject.AddComponent<ControllerPlayer>();
            equipController = gameObject.AddComponent<EquipController>();
            ComponentController.AddRigidbody2D(gameObject);
            Vector2 offset = Vector2.zero;
            Vector2 size = new Vector2(500, 250);
            ComponentController.AddCapsuleCollider2D(gameObject, true, offset, size);
            offset = Vector2.zero;
            size = new Vector2(264,132);
            ComponentController.AddCapsuleCollider2D(gameObject, false, offset, size,
                LayerMask.GetMask(ConfigLayer.GetValue(ConfigLayer.Keys.TileMapWall)),
                ~ (1 << LayerMask.NameToLayer(ConfigLayer.GetValue(ConfigLayer.Keys.TileMapWall))));
        }

        /// <summary>
        /// GGemCoPlayerSettings 에서 가져온 정보 셋팅
        /// </summary>
        protected override void InitializeByTable()
        {
            if (AddressableSettingsLoader.Instance == null) return;
            GGemCoPlayerSettings playerSettings = AddressableSettingsLoader.Instance.playerSettings;
            SetBaseInfos(playerSettings.statAtk, playerSettings.statDef, playerSettings.statHp, playerSettings.statMp, playerSettings.statMoveSpeed, playerSettings.statAttackSpeed);
            CurrentHp.OnNext(TotalHp.Value);
            CurrentMp.OnNext(TotalMp.Value);
            currentMoveStep = playerSettings.statMoveStep;
            originalScaleX = transform.localScale.x;
            SetScale(playerSettings.startScale);
        }
        /// <summary>
        /// 세이브 데이터에 있는 장착 아이템 정보 가져와서 장착 시키기
        /// </summary>
        private void LoadEquipItems()
        {
            Dictionary<int, StructInventoryIcon> dictionary =
                SceneGame.Instance.saveDataManager.Equip.GetAllItemCounts();
            foreach (var info in dictionary)
            {
                if (info.Value == null) continue;
                int itemUid = info.Value.ItemUid;
                int itemCount = info.Value.ItemCount;
                if (itemUid <= 0) continue;
                EquipItem(info.Key, itemUid, itemCount);
            }
        }
        protected void OnTriggerEnter2D(Collider2D collision)
        {
            // npc 일때
            if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Npc)))
            {
                isNpcNearby = true;
            }
            // 워프 일때
            else if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.MapObjectWarp)))
            {
                ObjectWarp objectWarp = collision.gameObject.GetComponent<ObjectWarp>();
                SceneGame.Instance.mapManager.LoadMapByWarp(objectWarp);
            }
            // 드랍 아이템 일때
            else if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.DropItem)))
            {
                SceneGame.Instance.ItemManager.PlayerTaken(collision.gameObject);
            }
        }
        protected void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Npc)))
            {
                isNpcNearby = false;
            }
        }
        /// <summary>
        /// 장비 장착하기
        /// </summary>
        /// <param name="partIndex"></param>
        /// <param name="itemUid"></param>
        /// <param name="itemCount"></param>
        public void EquipItem(int partIndex, int itemUid, int itemCount)
        {
            bool result = equipController.EquipItem(partIndex, itemUid);
            if (!result) return;
            if (itemUid <= 0)
            {
                UnEquipItem(partIndex);
                return;
            }
            var info = TableLoaderManager.Instance.TableItem.GetDataByUid(itemUid);
            if (info == null) return;
            
            ItemConstants.PartsType partsType = (ItemConstants.PartsType)partIndex;
            List<string> slotNames = ItemConstants.SlotNameByPartsType[partsType];

            List<StruckChangeSlotImage> changeImages = new List<StruckChangeSlotImage>();
            foreach (var slotName in slotNames)
            {
                string attachmentName = ItemConstants.AttachmentNameBySlotName[slotName];
                
                string changeSpritePath = $"Images/Parts/{ItemConstants.FolderNameByPartsType[partsType]}/{info.ImagePath}_{slotName}";
                var sprite = Resources.Load<Sprite>(changeSpritePath);

                StruckChangeSlotImage struckChangeSlotImage = new StruckChangeSlotImage(slotName, attachmentName, sprite);
                changeImages.Add(struckChangeSlotImage);
            }
            CharacterAnimationController.ChangeCharacterImageInSlot(changeImages);
        }
        /// <summary>
        /// 장비 해제 하기
        /// </summary>
        /// <param name="partIndex"></param>
        public void UnEquipItem(int partIndex)
        {
            bool result = equipController.UnEquipItem(partIndex);
            if (!result) return;
            
            ItemConstants.PartsType partsType = (ItemConstants.PartsType)partIndex;
            List<string> slotNames = ItemConstants.SlotNameByPartsType[partsType];

            List<StruckChangeSlotImage> changeImages = new List<StruckChangeSlotImage>();
            foreach (var slotName in slotNames)
            {
                string attachmentName = ItemConstants.AttachmentNameBySlotName[slotName];
                
                string changeSpritePath = $"Images/Parts/{ItemConstants.FolderNameByPartsType[partsType]}/{attachmentName}";
                var sprite = Resources.Load<Sprite>(changeSpritePath);

                StruckChangeSlotImage struckChangeSlotImage = new StruckChangeSlotImage(slotName, attachmentName, sprite);
                changeImages.Add(struckChangeSlotImage);
            }
            CharacterAnimationController.ChangeCharacterImageInSlot(changeImages);
        }
        /// <summary>
        /// attack 이벤트 처리 
        /// </summary>
        public override void OnEventAttack()
        {
            // GcLogger.Log(@event);
            long totalDamage = SceneGame.Instance.calculateManager.GetPlayerTotalAtk();
        
            // 캡슐 콜라이더 2D와 충돌 중인 모든 콜라이더를 검색
            CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
            Vector2 size = new Vector2(capsuleCollider.size.x * Mathf.Abs(transform.localScale.x), capsuleCollider.size.y * transform.localScale.y);
            Vector2 point = (Vector2)transform.position + capsuleCollider.offset * transform.localScale;
            Collider2D[] hitsCollider2D = Physics2D.OverlapCapsuleAll(point, size, capsuleCollider.direction, 0f);

            int countDamageMonster = 0;
            int maxDamageMonster = 10;
            foreach (var hit in hitsCollider2D)
            {
                if (hit.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Monster)))
                {
                    Monster.Monster monster = hit.GetComponent<Monster.Monster>();
                    if (monster != null)
                    {
                        // GcLogger.Log("Player attacked the monster after animation!");
                        // 몬스터와 마주보고 있으면 공격 
                        if (AreFacingEachOther(monster.transform))
                        {
                            monster.TakeDamage(totalDamage, gameObject);
                            ++countDamageMonster;
                        }
                        // 몬스터와 같은 곳을 바라보고 있으면,
                        else if (IsFlipped() == monster.IsFlipped())
                        {
                            // flip 일때는 
                            // monster.x >= player.x
                            if (IsFlipped() && monster.transform.position.x >= transform.position.x)
                            {
                                monster.TakeDamage(totalDamage, gameObject);
                                ++countDamageMonster;
                            }
                            // flip 이 아닐때는
                            // monster.x <= player.x
                            else if (IsFlipped() != true && monster.transform.position.x <= transform.position.x)
                            {
                                monster.TakeDamage(totalDamage, gameObject);
                                ++countDamageMonster; 
                            }
                        }
                        
                        // maxDamageMonster 마리 한테만 데미지 준다 
                        if (countDamageMonster > maxDamageMonster)
                        {
                            break;
                        }
                    }
                }
            }
        }
        private void SetWindowHudSliderHp(long value)
        {
            if (uiWindowHud == null)
            {
                GcLogger.LogError("UIWindowHud 가 없습니다.");
                return;
            }
            uiWindowHud.SetSliderHp(value, TotalHp.Value);
        }
        private void SetWindowHudSliderMp(long value)
        {
            if (uiWindowHud == null) 
            {
                GcLogger.LogError("UIWindowHud 가 없습니다.");
                return;
            }
            uiWindowHud.SetSliderMp(value, TotalMp.Value);
        }
        /// <summary>
        /// Player의 스탯과 UI를 매핑하여 리스트에 저장
        /// </summary>
        private void InitializeStatBindings()
        {
            statBindings.AddRange(new[]
            {
                new StatUIBinding { textUI = UIWindowPlayerInfo.IndexPlayerInfo.Atk, GetStat = p => p.TotalAtk, label = "공격력" },
                new StatUIBinding { textUI = UIWindowPlayerInfo.IndexPlayerInfo.Def, GetStat = p => p.TotalDef, label = "방어력" },
                new StatUIBinding { textUI = UIWindowPlayerInfo.IndexPlayerInfo.Hp, GetStat = p => p.TotalHp, label = "생명력" },
                new StatUIBinding { textUI = UIWindowPlayerInfo.IndexPlayerInfo.Mp, GetStat = p => p.TotalMp, label = "마력" },
                new StatUIBinding { textUI = UIWindowPlayerInfo.IndexPlayerInfo.MoveSpeed, GetStat = p => p.TotalMoveSpeed, label = "이동속도" },
                new StatUIBinding { textUI = UIWindowPlayerInfo.IndexPlayerInfo.AttackSpeed, GetStat = p => p.TotalAttackSpeed, label = "공격속도" },
                new StatUIBinding { textUI = UIWindowPlayerInfo.IndexPlayerInfo.CriticalDamage, GetStat = p => p.TotalCriticalDamage, label = "크리티컬 데미지" },
                new StatUIBinding { textUI = UIWindowPlayerInfo.IndexPlayerInfo.CriticalProbability, GetStat = p => p.TotalCriticalProbability, label = "크리티컬 확률" }
            });
            foreach (var binding in statBindings)
            {
                binding.GetStat(this).DistinctUntilChanged()
                    .Subscribe(value => UpdatePlayerInfoText(binding.textUI, binding.label, value))
                    .AddTo(this);
            }
        }
        /// <summary>
        /// UIWindowPlayerInfo 에 text 업데이트 하기
        /// </summary>
        /// <param name="textUI"></param>
        /// <param name="label"></param>
        /// <param name="value"></param>
        private void UpdatePlayerInfoText(UIWindowPlayerInfo.IndexPlayerInfo textUI, string label, long value)
        {
            UIWindowPlayerInfo uiWindowPlayerInfo =
                SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowPlayerInfo>(UIWindowManager.WindowUid.PlayerInfo);
            if (uiWindowPlayerInfo != null)
            {
                uiWindowPlayerInfo.UpdateText(textUI, label, value);
            }
        }
        /// <summary>
        /// 현재 생명력이 최대치인지
        /// </summary>
        /// <returns></returns>
        public bool IsMaxHp()
        {
            return CurrentHp.Value >= TotalHp.Value;
        }
        /// <summary>
        /// 현재 생명력 더하기
        /// </summary>
        /// <param name="value"></param>
        public void AddHp(int value)
        {
            long newVale = CurrentHp.Value + value;
            if (newVale > TotalHp.Value)
            {
                newVale = TotalHp.Value;
            }
            CurrentHp.OnNext(newVale);
        }
        /// <summary>
        /// 현재 마력이 최대치 인지
        /// </summary>
        /// <returns></returns>
        public bool IsMaxMp()
        {
            return CurrentMp.Value >= TotalMp.Value;
        }
        /// <summary>
        /// 현재 마력 더하기
        /// </summary>
        /// <param name="value"></param>
        public void AddMp(int value)
        {
            long newVale = CurrentMp.Value + value;
            if (newVale > TotalMp.Value)
            {
                newVale = TotalMp.Value;
            }
            CurrentMp.OnNext(newVale);
        }
        /// <summary>
        /// 버프 추가하기
        /// </summary>
        /// <param name="struckBuff"></param>
        public void AddBuff(StruckBuff struckBuff)
        {
            if (struckBuff == null) return;
            ApplyBuff(struckBuff);
        }
        /// <summary>
        /// total move speed 가 변경되었을때 wait 애니메이션의 time scale 도 변경해주기 위해서
        /// track index = 0 의 time scale 을 변경해준다.
        /// </summary>
        /// <param name="value"></param>
        private void UpdateAnimationMoveTimeScale(long value)
        {
            CharacterAnimationController.UpdateTimeScaleByTrackIndex(value/100f);
        }
        /// <summary>
        /// 플레이어 죽었을때 end 상태로 변경
        /// </summary>
        protected override void OnDead()
        {
            base.OnDead();
            SceneGame.Instance.SetState(SceneGame.GameState.End);
        }
    }
}