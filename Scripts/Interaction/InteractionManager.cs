using GGemCo.Scripts.Characters;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI;
using GGemCo.Scripts.UI.Window;
using GGemCo.Scripts.Utils;

namespace GGemCo.Scripts.Interaction
{
    public class InteractionManager
    {
        SceneGame sceneGame;
        TableNpc tableNpc;
        TableInteraction tableInteraction;
        private UIWindowInteractionDialogue uiWindowInteractionDialogue;
        public void Initialize(SceneGame scene)
        {
            sceneGame = scene;
            tableNpc = TableLoaderManager.Instance.TableNpc;
            tableInteraction = TableLoaderManager.Instance.TableInteraction;
            uiWindowInteractionDialogue =
                sceneGame.uIWindowManager.GetUIWindowByUid<UIWindowInteractionDialogue>(UIWindowManager.WindowUid.Dialogue);
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

            // 메시지가 있으면 메시지 창에서 버튼으로 선택
            var message = infoInteraction.Message;
            if (message != "")
            {
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

        public void EndInteraction()
        {
            uiWindowInteractionDialogue?.OnEndInteraction();
        }
    }
}