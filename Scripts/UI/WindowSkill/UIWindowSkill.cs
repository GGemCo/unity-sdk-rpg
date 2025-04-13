using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GGemCo.Scripts
{
    /// <summary>
    /// 플레이어 스킬
    /// </summary>
    public class UIWindowSkill : UIWindow
    {
        [Tooltip("스킬 element 프리팹")]
        public GameObject prefabUIElementSkill;

        public TableSkill TableSkill;
        public readonly Dictionary<int, UIElementSkill> UIElementSkills = new Dictionary<int, UIElementSkill>();
        private SkillData skillData;
        private QuickSlotData quickSlotData;
        
        private UIWindowQuickSlot uiWindowQuickSlot;
        private UIWindowSkillInfo uIWindowSkillInfo;
        
        protected override void Awake()
        {
            UIElementSkills.Clear();
            uid = UIWindowManager.WindowUid.Skill;
            if (TableLoaderManager.Instance == null) return;
            TableSkill = TableLoaderManager.Instance.TableSkill;
            maxCountIcon = TableSkill.GetSkills().Count;
            base.Awake();
            SetSetIconHandler(new SetIconHandlerSkill());
        }

        protected override void Start()
        {
            base.Start();
            skillData = SceneGame.saveDataManager.Skill;
            quickSlotData = SceneGame.saveDataManager.QuickSlot;
            uIWindowSkillInfo =
                SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowSkillInfo>(UIWindowManager.WindowUid
                    .SkillInfo);
            uiWindowQuickSlot =
                SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowQuickSlot>(UIWindowManager.WindowUid
                    .QuickSlot);
        }
        /// <summary>
        /// 슬롯 위치 정해주기
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="index"></param>
        public void SetPositionUiSlot(UISlot slot, int index)
        {
            UIElementSkill uiElementSkill = UIElementSkills[index];
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
            var datas = SceneGame.saveDataManager.Skill.GetAllDatas();
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
                UIElementSkill uiElementSkill = UIElementSkills[index];
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
            
            float time = SceneGame.uIIconCoolTimeManager.GetCurrentCoolTime(uid, icon.uid);
            if (time > 0)
            {
                SceneGame.systemMessageManager.ShowMessageWarning("쿨타임 중에는 바꿀 수 없습니다.");
                return;
            }
            if (!icon.IsLearn())
            {
                SceneGame.systemMessageManager.ShowMessageWarning("배운 후 사용할 수 있습니다.");
                return;
            }
            if (!icon.CheckRequireLevel()) return;
            if (uiWindowQuickSlot == null) return;
            // 퀵슬롯에 하나 넣기
            var result = quickSlotData.AddSkill(icon.uid, icon.GetCount(), icon.GetLevel(), icon.IsLearn());
            uiWindowQuickSlot.SetIcons(result);
        }
        public override UIElementSkill GetElementSkillByIndex(int slotIndex)
        {
            return UIElementSkills[slotIndex];
        }
    }
}