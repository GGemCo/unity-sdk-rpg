using System.Linq;
using GGemCo.Scripts.Items;
using GGemCo.Scripts.TableLoader;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace GGemCo.Scripts.UI.WindowSkill
{
    /// <summary>
    /// 플레이어 스킬 정보
    /// </summary>
    public class UIWindowSkillInfo : UIWindow
    {
        [Header("기본정보")] 
        [Tooltip("스킬 이름")]
        public TextMeshProUGUI textName;
        [Tooltip("필요 레벨")]
        public TextMeshProUGUI textNeedLevel;
        [Tooltip("스킬 타겟")]
        public TextMeshProUGUI textTarget;
        [FormerlySerializedAs("textDamageStatusID")] [Tooltip("데미지 타입")]
        public TextMeshProUGUI textDamageStatus;
        [Tooltip("소모 Mp")]
        public TextMeshProUGUI textNeedMp;
        [Tooltip("재사용 쿨타입(초)")]
        public TextMeshProUGUI textCoolTime;
        						
        [Header("옵션")]
        [Tooltip("옵션 ID")]
        public TextMeshProUGUI textOptionID1;
        // [Tooltip("옵션 수치")]
        // public TextMeshProUGUI textOptionValue1;
        // [Tooltip("옵션 지속시간")]
        // public TextMeshProUGUI textOptionDuration1;
        // [Tooltip("옵션 확률")]
        // public TextMeshProUGUI textOptionRate1;
        
        private StruckTableSkill struckTableSkill;
        private TableStatus tableStatus;
        private TableSkill tableSkill;
        
        protected override void Awake()
        {
            uid = UIWindowManager.WindowUid.SkillInfo;
            if (TableLoaderManager.Instance == null) return;
            tableSkill = TableLoaderManager.Instance.TableSkill;
            tableStatus = TableLoaderManager.Instance.TableStatus;
            base.Awake();
        }
        public void SetSkillUid(int skillUid, int skillLevel)
        {
            if (skillUid <= 0) return;
            struckTableSkill = tableSkill.GetDataByUidLevel(skillUid, skillLevel);
            if (struckTableSkill is not { Uid: > 0 }) return;
            
            SetBasicInfo();
            SetOptionInfo();
            Show(true);
        }
        /// <summary>
        /// 이름 설정하기
        /// </summary>
        private void SetBasicInfo()
        {
            if (struckTableSkill == null) return;
            textName.text = $"이름: {struckTableSkill.Name}";
            textNeedLevel.text = $"필요레벨: {struckTableSkill.NeedPlayerLevel}";
            textTarget.text = $"타겟: {struckTableSkill.Target}";
            if (struckTableSkill.DamageValue > 0)
            {
                textDamageStatus.gameObject.SetActive(true);
                textDamageStatus.text =
                    $"{GetStatusName(struckTableSkill.DamageStatusID)} : {GetValueText(struckTableSkill.DamageStatusID, struckTableSkill.DamageValue)}";
            }
            else
            {
                textDamageStatus.gameObject.SetActive(false);
            }

            textNeedMp.text = $"소모Mp: {struckTableSkill.NeedMp}";
            textCoolTime.text = $"쿨타임: {struckTableSkill.CoolTime}";
        }

        private string GetValueText(string statusId, float value)
        {
            string valueText = $"{value}";
            foreach (var suffix in ItemConstants.StatusSuffixFormats.Keys)
            {
                if (statusId.EndsWith(suffix))
                {
                    valueText = string.Format(ItemConstants.StatusSuffixFormats[suffix], value);
                    break; // 첫 번째로 매칭된 값만 적용
                }
            }

            return valueText;
        }

        private string GetStatusName(string statusId)
        {
            if (string.IsNullOrEmpty(statusId)) return "";
            string cleanedId = ItemConstants.StatusSuffixFormats.Aggregate(statusId, (current, suffix) => current.Replace(suffix.Key, ""));
            var info = tableStatus.GetDataById(cleanedId);
            return info?.Name ?? "";
        }

        private void SetOptionInfo()
        {
            if (struckTableSkill.OptionID1 == "")
            {
                textOptionID1.gameObject.SetActive(false);
                return;
            }
            textOptionID1.gameObject.SetActive(true);
            string option =
                $"{struckTableSkill.OptionRate1}% 확률로 {GetStatusName(struckTableSkill.OptionID1)} {GetValueText(struckTableSkill.OptionID1, struckTableSkill.OptionValue1)} 가 {struckTableSkill.OptionDuration1} 초 동안 발동합니다.";
            textOptionID1.text = option;
        }

    }
}