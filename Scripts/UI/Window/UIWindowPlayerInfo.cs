﻿using GGemCo.Scripts.TableLoader;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

namespace GGemCo.Scripts.UI.Window
{
    /// <summary>
    /// 플레이어 stat 정보 보여주는 윈도우
    /// </summary>
    public class UIWindowPlayerInfo : UIWindow
    {
        /// <summary>
        /// Player 클레스에서 subscribe 를 위해 사용중
        /// </summary>
        public enum IndexPlayerInfo
        {
            None,
            Atk,
            Def,
            Hp,
            Mp,
            MoveSpeed,
            AttackSpeed,
            CriticalDamage,
            CriticalProbability
        }
        
        [Header("Text 오브젝트")]
        public TextMeshProUGUI textTotalAtk;
        public TextMeshProUGUI textTotalDef;
        public TextMeshProUGUI textTotalHp;
        public TextMeshProUGUI textTotalMp;
        public TextMeshProUGUI textTotalMoveSpeed;
        public TextMeshProUGUI textTotalAttackSpeed;
        [HideInInspector] public TextMeshProUGUI textTotalCriticalDamage;
        [HideInInspector] public TextMeshProUGUI textTotalCriticalProbability;

        private Dictionary<IndexPlayerInfo, TextMeshProUGUI> playerInfos = new();
        
        protected override void Awake()
        {
            // uid 를 먼저 지정해야 한다.
            uid = UIWindowManager.WindowUid.ItemInfo;
            if (TableLoaderManager.Instance == null) return;
            base.Awake();
            
            playerInfos = new()
            {
                { IndexPlayerInfo.Atk, textTotalAtk },
                { IndexPlayerInfo.Def, textTotalDef },
                { IndexPlayerInfo.Hp, textTotalHp },
                { IndexPlayerInfo.Mp, textTotalMp },
                { IndexPlayerInfo.MoveSpeed, textTotalMoveSpeed },
                { IndexPlayerInfo.AttackSpeed, textTotalAttackSpeed },
                { IndexPlayerInfo.CriticalDamage, textTotalCriticalDamage },
                { IndexPlayerInfo.CriticalProbability, textTotalCriticalProbability }
            };
        }
        public void UpdateText(IndexPlayerInfo index, string label, long value)
        {
            if (index == IndexPlayerInfo.None) return;
            if (playerInfos.TryGetValue(index, out var textUI) && textUI != null)
            {
                textUI.text = $"{label}: {value}";
            }
        }
    }
}
