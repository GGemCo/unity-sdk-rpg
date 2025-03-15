using System.Collections.Generic;
using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Maps.Objects;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.ScriptableSettings;
using GGemCo.Scripts.Spine2d;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.Characters.Player
{
    public class Player : CharacterBase
    {
        // 공격할 몬스터 
        private GameObject targetMonster;
        // 주변에 npc 가 있는지 체크 
        private bool isNpcNearby;
        private GGemCoSettings gGemCoSettings; 
        private EquipController equipController;
        
        protected override void Awake()
        {
            base.Awake();
            isNpcNearby = false;
        }
        protected override void Start()
        {
            base.Start();
            equipController = GetComponent<EquipController>();
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
            base.InitComponents();
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
            {
                GGemCoPlayerSettings playerSettings = AddressableSettingsLoader.Instance.playerSettings;
                SetBaseInfos(playerSettings.statAtk, playerSettings.statDef, playerSettings.statHp, playerSettings.statMp, playerSettings.statMoveSpeed, playerSettings.statAttackSpeed);
                CurrentHp = TotalHp;
                CurrentMoveStep = playerSettings.statMoveStep;
                OriginalScaleX = transform.localScale.x;
                SetScale(playerSettings.startScale);
            }
        }
        protected void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Npc)))
            {
                isNpcNearby = true;
            }
            else if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.MapObjectWarp)))
            {
                ObjectWarp objectWarp = collision.gameObject.GetComponent<ObjectWarp>();
                SceneGame.Instance.mapManager.LoadMapByWarp(objectWarp);
            }
            else if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.DropItem)))
            {
                SceneGame.Instance.itemManager.PlayerTaken(collision.gameObject);
            }
        }
        protected void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Npc)))
            {
                isNpcNearby = false;
            }
        }
        public void EquipItem(int partIndex, int itemUid)
        {
            bool result = equipController.EquipItem(partIndex, itemUid);
            if (!result) return;
            
            var info = TableLoaderManager.Instance.TableItem.GetDataByUid(itemUid);
            if (info == null) return;
            
            EquipController.PartsType partsType = (EquipController.PartsType)partIndex;
            List<string> slotNames = EquipController.SlotNameByPartsType[partsType];

            List<StruckChangeSlotImage> changeImages = new List<StruckChangeSlotImage>();
            foreach (var slotName in slotNames)
            {
                string attachmentName = EquipController.AttachmentNameBySlotName[slotName];
                
                string changeSpritePath = $"Images/Parts/{info.SubCategory}/{info.ImagePath}_{slotName}";
                var sprite = Resources.Load<Sprite>(changeSpritePath);

                StruckChangeSlotImage struckChangeSlotImage = new StruckChangeSlotImage(slotName, attachmentName, sprite);
                changeImages.Add(struckChangeSlotImage);
            }
            CharacterAnimationController.ChangeCharacterImageInSlot(changeImages);
        }
        public void UnEquipItem(int partIndex)
        {
            bool result = equipController.UnEquipItem(partIndex);
        }

        public override void OnSpineEventAttack()
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
    }
}