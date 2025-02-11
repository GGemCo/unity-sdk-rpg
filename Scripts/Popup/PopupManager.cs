using System.Collections.Generic;
using GGemCo.Scripts.Core;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.Popup
{
    // 인트로 씬의 ErrorManager 에서 NormalButtons 타입을 사용하고 있다 
    public enum PopupType
    {
        None,
        NormalReward, // 타이틀, 보상 아이템이 있는 타입
        BossBattleWin, // 보스전 승리 타입
        BossBattleLose, // 보스전 실패 타입
        OnlyMessage, // 메시지만 있는 타입 
        DungeonWin, // 던전 승리
        DungeonLose, // 던전 실패
        DungeonTotalDamageComplete, // 랭킹 도전 완료 
        NormalButtons // 메시지, 확인, 취소 버튼 있는 타입
    }
    public class PopupManager : MonoBehaviour
    {
        public GameObject[] popupTypePrefabs;
        public Transform canvasPopup; // 팝업이 들어갈 canvas
        public GameObject elementRewardItem;
    
        private readonly Queue<DefaultPopup> popupQueue = new Queue<DefaultPopup>();
        private DefaultPopup currentDefaultPopup;

        public void ShowPopupDungeonBattleLose()
        {
            PopupType popupType = PopupType.DungeonLose;
            GameObject prefab = GetPopupPrefab(popupType);
            if (prefab == null)
            {
                GcLogger.LogError("dont exist popup prefab. type: "+popupType);
                return;
            }
            DefaultPopup newDefaultPopup = Instantiate(prefab, canvasPopup).GetComponent<DefaultPopup>();
            if (newDefaultPopup == null)
            {
                GcLogger.LogError("dont make new popup.");
                return;
            }

            PopupMetadata popupMetadata = new PopupMetadata();
            popupMetadata.Message = "던전 도전 실패";
            popupMetadata.ShowConfirmButton = false;
            popupMetadata.ShowBgBlack = true;
            popupMetadata.PopupType = popupType;
            newDefaultPopup.Initialize(popupMetadata);
            popupQueue.Enqueue(newDefaultPopup);
            ShowNextPopup();
        }
        public void ShowPopupDungeonTotalDamageComplete()
        {
            PopupType popupType = PopupType.DungeonTotalDamageComplete;
            GameObject prefab = GetPopupPrefab(popupType);
            if (prefab == null)
            {
                GcLogger.LogError("dont exist popup prefab. type: "+popupType);
                return;
            }
            DefaultPopup newDefaultPopup = Instantiate(prefab, canvasPopup).GetComponent<DefaultPopup>();
            if (newDefaultPopup == null)
            {
                GcLogger.LogError("dont make new popup.");
                return;
            }

            PopupMetadata popupMetadata = new PopupMetadata();
            popupMetadata.Message = "랭킨 던전 도전 완료";
            popupMetadata.ShowConfirmButton = false;
            popupMetadata.ShowBgBlack = true;
            popupMetadata.PopupType = popupType;
            newDefaultPopup.Initialize(popupMetadata);
            popupQueue.Enqueue(newDefaultPopup);
            ShowNextPopup();
        }
        public void ShowPopupBossBattleLose()
        {
            PopupType popupType = PopupType.BossBattleLose;
            GameObject prefab = GetPopupPrefab(popupType);
            if (prefab == null)
            {
                GcLogger.LogError("dont exist popup prefab. type: "+popupType);
                return;
            }
            DefaultPopup newDefaultPopup = Instantiate(prefab, canvasPopup).GetComponent<DefaultPopup>();
            if (newDefaultPopup == null)
            {
                GcLogger.LogError("dont make new popup.");
                return;
            }

            PopupMetadata popupMetadata = new PopupMetadata();
            popupMetadata.Message = "보스전 실패";
            popupMetadata.ShowConfirmButton = false;
            popupMetadata.ShowBgBlack = true;
            popupMetadata.PopupType = popupType;
            newDefaultPopup.Initialize(popupMetadata);
            popupQueue.Enqueue(newDefaultPopup);
            ShowNextPopup();
        }

        public void ShowPopupDungeonBattleWin(long rewardDia, long rewardGold, Dictionary<int, int> dictionaryRewardItems)
        {
            if (rewardDia == 0 && rewardGold == 0)
            {
                GcLogger.LogError("reward dia, reward gold is zero.");
                return;
            }
            PopupType popupType = PopupType.DungeonWin;
            GameObject prefab = GetPopupPrefab(popupType);
            if (prefab == null)
            {
                GcLogger.LogError("dont exist popup prefab. type: "+popupType);
                return;
            }
            DefaultPopup newDefaultPopup = Instantiate(prefab, canvasPopup).GetComponent<DefaultPopup>();
            if (newDefaultPopup == null)
            {
                GcLogger.LogError("dont make new popup.");
                return;
            }

            PopupMetadata popupMetadata = new PopupMetadata();
            popupMetadata.ShowConfirmButton = false;
            popupMetadata.ShowBgBlack = true;
            popupMetadata.RewardDia = rewardDia;
            popupMetadata.RewardGold = rewardGold;
            popupMetadata.PopupType = popupType;
            popupMetadata.DictionaryRewardItems = dictionaryRewardItems;
            newDefaultPopup.Initialize(popupMetadata);
            popupQueue.Enqueue(newDefaultPopup);
            ShowNextPopup();
        }
        public void ShowPopupBossBattleWin(long rewardDia, long rewardGold)
        {
            if (rewardDia == 0 && rewardGold == 0)
            {
                GcLogger.LogError("reward dia, reward gold is zero.");
                return;
            }
            PopupType popupType = PopupType.BossBattleWin;
            GameObject prefab = GetPopupPrefab(popupType);
            if (prefab == null)
            {
                GcLogger.LogError("dont exist popup prefab. type: "+popupType);
                return;
            }
            DefaultPopup newDefaultPopup = Instantiate(prefab, canvasPopup).GetComponent<DefaultPopup>();
            if (newDefaultPopup == null)
            {
                GcLogger.LogError("dont make new popup.");
                return;
            }

            PopupMetadata popupMetadata = new PopupMetadata();
            popupMetadata.ShowConfirmButton = false;
            popupMetadata.ShowBgBlack = true;
            popupMetadata.RewardDia = rewardDia;
            popupMetadata.RewardGold = rewardGold;
            popupMetadata.PopupType = popupType;
            newDefaultPopup.Initialize(popupMetadata);
            popupQueue.Enqueue(newDefaultPopup);
            ShowNextPopup();
        }

        public void ShowPopupOnlyMessage(string message, params object[] parameters)
        {
            PopupType popupType = PopupType.OnlyMessage;
            GameObject prefab = GetPopupPrefab(popupType);
            if (prefab == null)
            {
                GcLogger.LogError("dont exist popup prefab. type: "+popupType);
                return;
            }
            DefaultPopup newDefaultPopup = Instantiate(prefab, canvasPopup).GetComponent<DefaultPopup>();
            if (newDefaultPopup == null)
            {
                GcLogger.LogError("dont make new popup.");
                return;
            }
            message = string.Format(message, parameters);

            PopupMetadata popupMetadata = new PopupMetadata();
            popupMetadata.Message = message;
            popupMetadata.ShowConfirmButton = false;
            popupMetadata.ShowCancelButton = false;
            popupMetadata.PopupType = popupType;
            newDefaultPopup.Initialize(popupMetadata);
            popupQueue.Enqueue(newDefaultPopup);
            ShowNextPopup();
        }

        public void ShowPopup(PopupMetadata popupMetadata)
        {
            GameObject prefab = GetPopupPrefab(popupMetadata.PopupType);
            if (prefab == null)
            {
                GcLogger.LogError("dont exist popup prefab. type: "+popupMetadata.PopupType);
                return;
            }
            DefaultPopup newDefaultPopup = Instantiate(prefab, canvasPopup).GetComponent<DefaultPopup>();
            if (newDefaultPopup == null)
            {
                GcLogger.LogError("dont make new popup.");
                return;
            }
            newDefaultPopup.Initialize(popupMetadata);
            
            if (popupMetadata.ForceShow)
            {
                newDefaultPopup.ShowPopup();
            }
            else
            {
                popupQueue.Enqueue(newDefaultPopup);
                ShowNextPopup();
            }
        }

        private void ShowNextPopup()
        {
            if (currentDefaultPopup != null)
            {
                currentDefaultPopup.ClosePopup();
            }
            if (popupQueue.Count <= 0) return;
            
            currentDefaultPopup = popupQueue.Dequeue();
            currentDefaultPopup.ShowPopup();
            DefaultPopup fgDefaultPopup = currentDefaultPopup.GetComponent<DefaultPopup>();
            if (fgDefaultPopup.buttonConfirm!= null) 
            {
                fgDefaultPopup.buttonConfirm.onClick.AddListener(OnPopupClosed);
            }

            if (fgDefaultPopup.buttonCancel != null)
            {
                fgDefaultPopup.buttonCancel.onClick.AddListener(OnPopupClosed);
            }
        }

        private void OnPopupClosed()
        {
            currentDefaultPopup = null;
            ShowNextPopup();
        }

        public GameObject GetPopupPrefab(PopupType popupType)
        {
            return popupTypePrefabs[(int)popupType];
        }
        /// <summary>
        /// 다이아 부족할때 보여주는 팝업
        /// </summary>
        public void ShowPopupNeedDia()
        {
            PopupMetadata popupMetadata = new PopupMetadata();
            popupMetadata.Title = "다이아 구매하기";
            popupMetadata.Message = "다이아가 부족합니다.\n다이아를 구매하시겠습니까?";
            popupMetadata.ShowCancelButton = true;
            popupMetadata.PopupType = PopupType.NormalButtons;
            popupMetadata.OnConfirm = () =>
            {
                GcLogger.Log("확인 버튼 클릭됨");
                // SceneGame.Instance.uIWindowManager.CloseAll();
                // SceneGame.Instance.uIWindowManager.ShowWindow(UIWindowManager.WindowUid.CashShop, true);
            };
            popupMetadata.OnCancel = () =>
            {
                GcLogger.Log("취소 버튼 클릭됨");
            };
            ShowPopup(popupMetadata);
        }
    }
}