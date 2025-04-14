
using System.Collections.Generic;

namespace GGemCo.Scripts
{
    public class InteractionManager
    {
        private SceneGame sceneGame;
        private TableNpc tableNpc;
        private TableInteraction tableInteraction;
        private UIWindowInteractionDialogue uiWindowInteractionDialogue;
        private CharacterBase currentNpc;
        
        public void Initialize(SceneGame scene)
        {
            sceneGame = scene;
            tableNpc = TableLoaderManager.Instance.TableNpc;
            tableInteraction = TableLoaderManager.Instance.TableInteraction;
            uiWindowInteractionDialogue =
                sceneGame.uIWindowManager?.GetUIWindowByUid<UIWindowInteractionDialogue>(UIWindowManager.WindowUid.Dialogue);
                sceneGame.uIWindowManager?.GetUIWindowByUid<UIWindowShopSale>(UIWindowManager.WindowUid.ShopSale);
        }

        public void SetInfo(CharacterBase characterBase)
        {
            if (characterBase == null)
            {
                GcLogger.LogError("Npc 스크립트가 없습니다.");
                return;
            }

            var infoNpc = tableNpc.GetDataByUid(characterBase.uid);
            if (infoNpc == null)
            {
                GcLogger.LogError("npc 테이블에 정보가 없습니다. npc uid: "+characterBase.uid);
                return;
            }

            if (infoNpc.InteractionUid <= 0) return;
            var infoInteraction = tableInteraction.GetDataByUid(infoNpc.InteractionUid);
            if (infoInteraction == null)
            {
                GcLogger.LogError("interaction 테이블에 정보가 없습니다. interaction uid: "+infoNpc.InteractionUid);
                return;
            }
            currentNpc = characterBase;

            // 메시지가 있으면 메시지 창에서 버튼으로 선택
            var message = infoInteraction.Message;
            if (message != "")
            {
                // 다른 윈도우가 열려있으면 닫아주기
                sceneGame.uIWindowManager.CloseAll(new List<UIWindowManager.WindowUid>
                    { UIWindowManager.WindowUid.Dialogue });
                ShowDialogue(infoNpc, infoInteraction);
            }
            // 메시지가 없으면 type별로 npc 머리 위로?
            // type 이 1개면 바로 실행
        }

        private void ShowDialogue(StruckTableNpc struckTableNpc, StruckTableInteraction struckTableInteraction)
        {
            uiWindowInteractionDialogue?.SetInfos(struckTableNpc, struckTableInteraction);
            uiWindowInteractionDialogue?.Show(true);
        }

        public void RemoveCurrentNpc()
        {
            currentNpc = null;
        }
        /// <summary>
        /// interaction 종료하기
        /// </summary>
        public void EndInteraction()
        {
            // npc 가 interaction 범위면 다시 열기
            if (currentNpc != null)
            {
                sceneGame?.uIWindowManager?.CloseAll(new List<UIWindowManager.WindowUid>
                    { UIWindowManager.WindowUid.Dialogue });
                SetInfo(currentNpc);
                return;
            }
            sceneGame?.uIWindowManager?.CloseAll();
            uiWindowInteractionDialogue?.OnEndInteraction();
        }
        public bool IsInteractioning()
        {
            return currentNpc != null;
        }
    }
}