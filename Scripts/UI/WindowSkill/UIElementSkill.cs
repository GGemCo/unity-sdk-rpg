using GGemCo.Scripts.Characters.Player;
using GGemCo.Scripts.SaveData;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI.Window;
using GGemCo.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGemCo.Scripts.UI.WindowSkill
{
    /// <summary>
    /// 스킬 리스트 element
    /// </summary>
    public class UIElementSkill : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler 
    {
        public Vector3 iconPosition;
        public TextMeshProUGUI textName;
        public TextMeshProUGUI textLevel;
        public TextMeshProUGUI textNeedLevel;
        public Button buttonLearn;
        public Button buttonLevelUp;
        
        private UIWindowSkill uiWindowSkill;
        private UIWindowSkillInfo uiWindowSkillInfo;
        private StruckTableSkill struckTableSkill;
        private SaveDataIcon saveDataIcon;
        private int slotIndex;
        
        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="puiWindowSkill"></param>
        /// <param name="pslotIndex"></param>
        /// <param name="pstruckTableSkill"></param>
        /// <param name="pstructSkillIcon"></param>
        public void Initialize(UIWindowSkill puiWindowSkill, int pslotIndex, StruckTableSkill pstruckTableSkill, SaveDataIcon pstructSkillIcon = null)
        {
            slotIndex = pslotIndex;
            struckTableSkill = pstruckTableSkill;
            saveDataIcon = pstructSkillIcon;
            if (buttonLearn != null)
            {
                buttonLearn.gameObject.SetActive(true);
                buttonLearn.onClick.AddListener(OnClickLearn);
            }
            if (buttonLevelUp != null)
            {
                buttonLevelUp.gameObject.SetActive(false);
                buttonLevelUp.onClick.AddListener(OnClickLevelUp);
            }

            uiWindowSkill = puiWindowSkill;
            uiWindowSkillInfo =
                SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowSkillInfo>(
                    UIWindowManager.WindowUid.SkillInfo);
            
            if (textName != null) textName.text = struckTableSkill.Name;
            UpdateInfos(pstruckTableSkill, saveDataIcon);
        }

        /// <summary>
        /// slotIndex 로 아이템 정보를 가져온다.
        /// SaveDataIcon 정보에 따라 버튼 visible 업데이트
        /// </summary>
        public void UpdateInfos(StruckTableSkill pstruckTableSkill, SaveDataIcon psaveDataIcon)
        {
            struckTableSkill = pstruckTableSkill;
            saveDataIcon = psaveDataIcon;
            if (struckTableSkill == null)
            {
                GcLogger.LogError($"스킬 테이블에 없는 스킬입니다. struckTableSkill is null");
                return;
            }

            int level = saveDataIcon?.Level ?? 1;
            if (textLevel != null) textLevel.text = $"Lv.{level}";
            if (textNeedLevel != null) textNeedLevel.text = $"필요레벨 : {struckTableSkill.NeedPlayerLevel}";

            // 최대 레벨
            if (saveDataIcon != null && struckTableSkill != null && saveDataIcon.Level >= struckTableSkill.Maxlevel)
            {
                buttonLearn.gameObject.SetActive(false);
                buttonLevelUp.GetComponentInChildren<TextMeshProUGUI>().text = "최대레벨";
                buttonLevelUp.gameObject.SetActive(true);
                buttonLevelUp.interactable = false;
            }
            else if (saveDataIcon is { IsLearned: true })
            {
                buttonLearn.gameObject.SetActive(false);
                buttonLevelUp.gameObject.SetActive(true);
            }
            else
            {
                buttonLearn.gameObject.SetActive(true);
                buttonLevelUp.gameObject.SetActive(false); 
            }
        }
        /// <summary>
        /// 레벨업
        /// </summary>
        private void OnClickLevelUp()
        {
            bool result = SceneGame.Instance.player.GetComponent<Player>().IsRequireLevel(struckTableSkill.NeedPlayerLevel);
            if (!result) return;
            // 다음 레벨 있는지 체크, 아니면 최대 레벨
            int nextLevel = struckTableSkill.Level + 1;
            var info = TableLoaderManager.Instance.TableSkill.GetDataByUidLevel(struckTableSkill.Uid, nextLevel);
            if (info == null)
            {
                SceneGame.Instance.systemMessageManager.ShowMessageWarning("최대 레벨입니다.");
                return;
            }
            var result2 =SceneGame.Instance.saveDataManager.Skill.SetSkillLevelUp(slotIndex, struckTableSkill.Uid, 1, nextLevel, true);
            uiWindowSkill.SetIcons(result2);
        }
        /// <summary>
        /// 배우기
        /// </summary>
        private void OnClickLearn()
        {
            // GcLogger.Log("click learn");
            bool result = SceneGame.Instance.player.GetComponent<Player>().IsRequireLevel(struckTableSkill.NeedPlayerLevel);
            if (!result) return;

            var result2 = SceneGame.Instance.saveDataManager.Skill.SetSkillLearn(slotIndex, struckTableSkill.Uid, 1, struckTableSkill.Level, true);
            uiWindowSkill.SetIcons(result2);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            uiWindowSkillInfo.SetSkillUid(struckTableSkill.Uid, struckTableSkill.Level);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            uiWindowSkillInfo.Show(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
        }
        public Vector3 GetIconPosition() => iconPosition;
    }
}