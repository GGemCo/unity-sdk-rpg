using System;
using System.Collections.Generic;
using GGemCo.Scripts.Characters.Player;
using GGemCo.Scripts.keyboard;
using GGemCo.Scripts.SaveData;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI.Icon;
using GGemCo.Scripts.UI.WindowSkill;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GGemCo.Scripts.UI.Window
{
    /// <summary>
    /// 퀵슬롯
    /// </summary>
    public class UIWindowQuickSlot : UIWindow, IInputHandler
    {
        public UIWindowSkill uiWindowSkill;
        public int Priority => 1;
        private QuickSlotData quickSlotData;

        private Dictionary<KeyCode, int> indexByKeyCode = new Dictionary<KeyCode, int>
        {
            { KeyCode.Alpha1, 0 },
            { KeyCode.Alpha2, 1 },
            { KeyCode.Alpha3, 2 },
            { KeyCode.Alpha4, 3 },
        };
        
        protected override void Awake()
        {
            // uid 를 먼저 지정해야 한다.
            uid = UIWindowManager.WindowUid.QuickSlot;
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            quickSlotData = SceneGame.Instance.saveDataManager.QuickSlot;
            LoadIcons();
        }
        /// <summary>
        /// 저장되어있는 스킬 정보로 아이콘 셋팅하기
        /// 스킬창이 열려있지 않으면 업데이트 하지 않음
        /// </summary>
        private void LoadIcons()
        {
            if (!gameObject.activeSelf) return;
            var datas = SceneGame.Instance.saveDataManager.QuickSlot.GetAllDatas();
            if (datas == null) return;
            for (int index = 0; index < maxCountIcon; index++)
            {
                if (index >= icons.Length) continue;
                var icon = icons[index];
                if (icon == null) continue;
                UIIconSkill uiIcon = icon.GetComponent<UIIconSkill>();
                if (uiIcon == null) continue;
                SaveDataIcon structSkillIcon = datas.GetValueOrDefault(index);
                if (structSkillIcon == null) continue;
                
                int skillUid = structSkillIcon.Uid;
                int skillCount = structSkillIcon.Count;
                int skillLevel = structSkillIcon.Level;
                uiIcon.ChangeInfoByUid(skillUid, skillCount, skillLevel);
            }
        }
        protected void OnEnable()
        {
            if (SceneGame.Instance == null) return;
            SceneGame.Instance.KeyboardManager.RegisterInputHandler(this);
        }

        protected void OnDisable()
        {
            if (SceneGame.Instance == null) return;
            SceneGame.Instance.KeyboardManager.RemoveInputHandler(this);
        }

        public bool HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                OnKeyDownSkill(KeyCode.Alpha1);
                return true;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                OnKeyDownSkill(KeyCode.Alpha2);
                return true;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                OnKeyDownSkill(KeyCode.Alpha3);
                return true;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                OnKeyDownSkill(KeyCode.Alpha4);
                return true;
            }

            return false;
        }
        /// <summary>
        /// 키보드로 스킬 사용하기
        /// </summary>
        /// <param name="keyCode"></param>
        private void OnKeyDownSkill(KeyCode keyCode)
        {
            if (SceneGame.Instance.player == null)
            {
                GcLogger.LogError("플레이어가 없습니다.");
                return ;
            }
            // GcLogger.Log("UIWindowQuickSlot Key pressed Alpha1");
            UIIcon icon = GetIconByIndex(indexByKeyCode.GetValueOrDefault(keyCode));
            if (icon == null || icon.uid <= 0) return;
            if (!icon.IsSkill()) return;
            var info = TableLoaderManager.Instance.TableSkill.GetDataByUidLevel(icon.uid, icon.GetLevel());
            if (info == null)
            {
                GcLogger.LogError("스킬 테이블에 없는 스킬입니다. uid: " + icon.uid);
                return;
            }

            if (SceneGame.Instance.player.GetComponent<Player>().CheckNeedMp(info.NeedMp) == false)
            {
                SceneGame.Instance.systemMessageManager.ShowMessageWarning("마력이 부족합니다.");
                return;
            }

            if (!icon.PlayCoolTime(info.CoolTime)) return;
            
            SceneGame.Instance.player.GetComponent<Player>().UseSkill(icon.uid, icon.GetLevel());
        }
        
        /// <summary>
        ///  window 밖에 드래그앤 드랍 했을때 처리 
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="droppedIcon"></param>
        /// <param name="targetIcon"></param>
        /// <param name="originalPosition"></param>
        public override void OnEndDragOutWindow(PointerEventData eventData, GameObject droppedIcon, GameObject targetIcon, Vector3 originalPosition)
        {
            // GcLogger.Log("OnEndDragOutWindow");
            UIIcon droppedUIIcon = droppedIcon.GetComponent<UIIcon>();
            UIWindow droppedWindow = droppedUIIcon.window;
            UIWindowManager.WindowUid droppedWindowUid = droppedUIIcon.windowUid;
            int dropIconSlotIndex = droppedUIIcon.slotIndex;
            int dropIconUid = droppedUIIcon.uid;
            int dropIconCount = droppedUIIcon.GetCount();
            droppedWindow.DetachIcon(dropIconSlotIndex);
            
            GoBackToSlot(droppedIcon);
        }
        
        /// <summary>
        /// 아이콘 프리팹을 미리 만들어 놓기 때문에 무조건 아이콘 위에서 드래그가 끝나면 GameObject 는 존재한다. 
        /// </summary>
        /// <param name="droppedIcon">드랍한 한 아이콘</param>
        /// <param name="targetIcon">드랍되는 곳에 있는 아이콘</param>
        public override void OnEndDragInIcon(GameObject droppedIcon, GameObject targetIcon)
        {
            // GcLogger.Log("skill window. OnEndDragInIcon");
            UIIcon droppedUIIcon = droppedIcon.GetComponent<UIIcon>();
            UIWindow droppedWindow = droppedUIIcon.window;
            UIWindowManager.WindowUid droppedWindowUid = droppedUIIcon.windowUid;
            int dropIconSlotIndex = droppedUIIcon.slotIndex;
            int dropIconUid = droppedUIIcon.uid;
            int dropIconCount = droppedUIIcon.GetCount();
            int dropIconLevel = droppedUIIcon.GetLevel();
            bool dropIconIsLearn = droppedUIIcon.IsLearn();
            if (dropIconUid <= 0)
            {
                GoBackToSlot(droppedIcon);
                return;
            }
            
            UIIcon targetUIIcon = targetIcon.GetComponent<UIIcon>();
            // 드래그앤 드랍 한 곳에 아무것도 없을때 
            if (targetUIIcon == null)
            {
                GoBackToSlot(droppedIcon);
                return;
            }
            UIWindow targetWindow = targetUIIcon.window;
            UIWindowManager.WindowUid targetWindowUid = targetUIIcon.windowUid;
            int targetIconSlotIndex = targetUIIcon.slotIndex;
            int targetIconUid = targetUIIcon.uid;
            int targetIconCount = targetUIIcon.GetCount();

            // 다른 윈도우에서 Skill로 드래그 앤 드랍 했을 때 
            if (droppedWindowUid != targetWindowUid)
            {
                switch (droppedWindowUid)
                {
                    case UIWindowManager.WindowUid.Skill:
                        if (droppedUIIcon.CheckRequireLevel())
                        {
                            targetWindow.SetIconCount(targetIconSlotIndex, dropIconUid, dropIconCount, dropIconLevel, dropIconIsLearn);
                        }

                        break;
                    case UIWindowManager.WindowUid.None:
                    case UIWindowManager.WindowUid.Hud:
                    case UIWindowManager.WindowUid.Inventory:
                    case UIWindowManager.WindowUid.ItemInfo:
                    case UIWindowManager.WindowUid.Equip:
                    case UIWindowManager.WindowUid.PlayerInfo:
                    case UIWindowManager.WindowUid.ItemSplit:
                    case UIWindowManager.WindowUid.PlayerBuffInfo:
                    case UIWindowManager.WindowUid.QuickSlot:
                    case UIWindowManager.WindowUid.SkillInfo:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                if (targetIconSlotIndex < maxCountIcon)
                {
                }
            }
            GoBackToSlot(droppedIcon);
        }
        /// <summary>
        /// 아이콘 우클릭했을때 처리 
        /// </summary>
        /// <param name="icon"></param>
        public override void OnRightClick(UIIcon icon)
        {
            if (icon == null) return;
            
            float time = SceneGame.Instance.uIIconCoolTimeManager.GetCurrentCoolTime(uid, icon.uid);
            if (time > 0)
            {
                SceneGame.Instance.systemMessageManager.ShowMessageWarning("쿨타임 중에는 사용할 수 없습니다.");
                return;
            }
            // 스킬 창이 열려있을때는 해제 하기
            if (!uiWindowSkill.IsOpen()) return;
            DetachIcon(icon.slotIndex);
        }
        protected override void OnSetIcon(int slotIndex, int iconUid, int iconCount, int iconLevel = 0, bool iconLearn = false)
        {
            base.OnSetIcon(slotIndex, iconUid, iconCount, iconLevel, iconLearn);
            UIIcon uiIcon = GetIconByIndex(slotIndex);
            if (uiIcon == null) return;
            quickSlotData.SetSkill(slotIndex, iconUid, iconCount, iconLevel, iconLearn);
        }

        protected override void OnDetachIcon(int slotIndex)
        {
            base.OnDetachIcon(slotIndex);
            UIIcon uiIcon = GetIconByIndex(slotIndex);
            if (uiIcon == null) return;
            UIIconSkill uiIconSkill = uiIcon.GetComponent<UIIconSkill>();
            if (uiIconSkill == null) return;
            quickSlotData.RemoveSkill(slotIndex);
        }
    }
}