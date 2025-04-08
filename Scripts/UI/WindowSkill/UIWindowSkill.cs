using System.Collections.Generic;
using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.SaveData;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI.Icon;
using GGemCo.Scripts.UI.Window;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GGemCo.Scripts.UI.WindowSkill
{
    /// <summary>
    /// 플레이어 스킬
    /// </summary>
    public class UIWindowSkill : UIWindow
    {
        [Tooltip("스킬 정보 윈도우")]
        public UIWindowSkillInfo uIWindowSkillInfo;
        [Tooltip("스킬 element 프리팹")]
        public GameObject prefabUIElementSkill;
        
        private TableSkill tableSkill;
        private readonly Dictionary<int, UIElementSkill> uiElementSkills = new Dictionary<int, UIElementSkill>();
        private SkillData skillData;
        private QuickSlotData quickSlotData;
        
        protected override void Awake()
        {
            uiElementSkills.Clear();
            uid = UIWindowManager.WindowUid.Skill;
            if (TableLoaderManager.Instance == null) return;
            tableSkill = TableLoaderManager.Instance.TableSkill;
            maxCountIcon = tableSkill.GetSkills().Count;
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            skillData = SceneGame.Instance.saveDataManager.Skill;
            quickSlotData = SceneGame.Instance.saveDataManager.QuickSlot;
        }
        /// <summary>
        /// skill 테이블을 읽어서 개수만큼 풀을 확장하여 추가 생성.
        /// </summary>
        protected override void ExpandPool(int amount)
        {
            if (AddressableSettingsLoader.Instance == null || containerIcon == null) return;
            if (prefabUIElementSkill == null)
            {
                GcLogger.LogError("UIElementSkill 프리팹이 없습니다.");
                return;
            }
            var datas = tableSkill.GetSkills();
            maxCountIcon = datas.Count;
            if (datas.Count <= 0) return;
            
            GameObject iconSkill = AddressablePrefabLoader.Instance.GetPreLoadGamePrefabByName(ConfigAddressables.KeyPrefabIconSkill);
            GameObject slot = AddressablePrefabLoader.Instance.GetPreLoadGamePrefabByName(ConfigAddressables.KeyPrefabSlot);
            if (iconSkill == null) return;

            int index = 0;
            foreach (var data in datas)
            {
                int skillUid = data.Key;
                if (skillUid <= 0) continue;
                var info = data.Value;

                GameObject parent = gameObject;
                // UI Element 프리팹이 있으면 만든다.
                if (prefabUIElementSkill != null)
                {
                    parent = Instantiate(prefabUIElementSkill, containerIcon.gameObject.transform);
                    if (parent == null) continue;
                    UIElementSkill uiElementSkill = parent.GetComponent<UIElementSkill>();
                    if (uiElementSkill == null) continue;
                    uiElementSkill.Initialize(this, index, info);
                    uiElementSkills.TryAdd(index, uiElementSkill);
                }

                GameObject slotObject = Instantiate(slot, parent.transform);
                UISlot uiSlot = slotObject.GetComponent<UISlot>();
                if (uiSlot == null) continue;
                uiSlot.Initialize(this, uid, index, slotSize);
                SetPositionUiSlot(uiSlot, index);
                slots[index] = slotObject;
                
                GameObject icon = Instantiate(iconSkill, slotObject.transform);
                UIIconSkill uiIcon = icon.GetComponent<UIIconSkill>();
                if (uiIcon == null) continue;
                uiIcon.Initialize(this, uid, index, index, iconSize, slotSize);
                // count, 레벨 1로 초기화
                uiIcon.ChangeInfoByUid(skillUid, 1, 1);
                uiIcon.SetRaycastTarget(false);
                
                icons[index] = icon;
                index++;
            }
            // GcLogger.Log($"풀 확장: {amount}개 아이템 추가 (총 {poolDropItem.Count}개)");
        }
        /// <summary>
        /// 슬롯 위치 정해주기
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="index"></param>
        private void SetPositionUiSlot(UISlot slot, int index)
        {
            UIElementSkill uiElementSkill = uiElementSkills[index];
            if (uiElementSkill == null) return;
            Vector3 position = uiElementSkill.GetIconPosition();
            if (position == Vector3.zero) return;
            slot.transform.localPosition = position;
        }

        public override void OnShow(bool show)
        {
            if (SceneGame.Instance == null || TableLoaderManager.Instance == null) return;
            if (!show)
            {
                uIWindowSkillInfo?.Show(false);
                return;
            }
            LoadIcons();
        }
        
        /// <summary>
        /// 저장되어있는 스킬 정보로 아이콘 셋팅하기
        /// 스킬창이 열려있지 않으면 업데이트 하지 않음
        /// </summary>
        private void LoadIcons()
        {
            if (!gameObject.activeSelf) return;
            var datas = SceneGame.Instance.saveDataManager.Skill.GetAllDatas();
            if (datas == null) return;
            for (int index = 0; index < maxCountIcon; index++)
            {
                if (index >= icons.Length) continue;
                var icon = icons[index];
                if (icon == null) continue;
                UIIconSkill uiIcon = icon.GetComponent<UIIconSkill>();
                if (uiIcon == null) continue;
                SaveDataIcon saveDataIcon = datas.GetValueOrDefault(index);
                if (saveDataIcon == null) continue;
                
                int skillUid = saveDataIcon.Uid;
                int skillCount = saveDataIcon.Count;
                int skillLevel = saveDataIcon.Level;
                bool skillIsLearned = saveDataIcon.IsLearned;
                var info = TableLoaderManager.Instance.TableSkill.GetDataByUidLevel(skillUid, skillLevel);
                if (info == null) continue;
                uiIcon.ChangeInfoByUid(skillUid, skillCount, skillLevel, skillIsLearned);
                UIElementSkill uiElementSkill = uiElementSkills[index];
                if (uiElementSkill != null)
                {
                    uiElementSkill.UpdateInfos(info, saveDataIcon);
                }
            }
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
            UIIconSkill droppedUIIcon = droppedIcon.GetComponent<UIIconSkill>();
            UIWindow droppedWindow = droppedUIIcon.window;
            UIWindowManager.WindowUid droppedWindowUid = droppedUIIcon.windowUid;
            int dropIconSlotIndex = droppedUIIcon.slotIndex;
            int dropIconUid = droppedUIIcon.uid;
            int dropIconCount = droppedUIIcon.GetCount();
            if (dropIconUid <= 0)
            {
                GoBackToSlot(droppedIcon);
                return;
            }
            
            UIIconSkill targetUIIcon = targetIcon.GetComponent<UIIconSkill>();
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
                SceneGame.Instance.systemMessageManager.ShowMessageWarning("쿨타임 중에는 바꿀 수 없습니다.");
                return;
            }
            if (!icon.IsLearn())
            {
                SceneGame.Instance.systemMessageManager.ShowMessageWarning("배운 후 사용할 수 있습니다.");
                return;
            }
            if (!icon.CheckRequireLevel()) return;
            UIWindowQuickSlot uiWindowQuickSlot =
                SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowQuickSlot>(UIWindowManager.WindowUid
                    .QuickSlot);
            if (uiWindowQuickSlot == null) return;
            // 퀵슬롯에 하나 넣기
            var result = quickSlotData.AddSkill(icon.uid, icon.GetCount(), icon.GetLevel(), icon.IsLearn());
            uiWindowQuickSlot.SetIcons(result);
        }
        protected override void OnSetIcon(int slotIndex, int iconUid, int iconCount, int iconLevel = 0, bool iconLearn = false)
        {
            base.OnSetIcon(slotIndex, iconUid, iconCount, iconLevel, iconLearn);
            UIIcon uiIcon = GetIconByIndex(slotIndex);
            if (uiIcon == null) return;
            skillData.SetSkill(slotIndex, iconUid, iconCount, iconLevel, iconLearn);
            UIElementSkill uiElementSkill = uiElementSkills[slotIndex];
            if (uiElementSkill != null)
            {
                UIIconSkill uiIconSkill = uiIcon.GetComponent<UIIconSkill>();
                uiElementSkill.UpdateInfos(uiIconSkill.GetTableInfo(), uiIconSkill.GetSaveDataInfo());
            }
        }
    }
}