using UnityEngine;

namespace GGemCo.Scripts
{
    public class ObjectiveHandlerCollectItem : ObjectiveHandlerBase
    {
        private QuestStep currentStep;
        private int currentCount = 0;
        private int currentStepIndex = 0;

        protected override void StartObjectiveTyped(int questUid, QuestStep step, int stepIndex, int npcUid)
        {
            currentStep = step;
            currentCount = 0;
            currentStepIndex = stepIndex;
            GameEventManager.OnItemCollected += OnItemCollected;
        }

        protected override bool IsObjectiveCompleteTyped(QuestStep step)
        {
            return currentCount >= step.count;
        }
        private void OnItemCollected(int itemUid, int count)
        {
            if (itemUid == currentStep.targetUid)
            {
                currentCount += count;
                Debug.Log($"[CollectObjective] {itemUid} 수집: {currentCount}/{currentStep.count}");
            }
        }

        public override void OnDispose()
        {
            GameEventManager.OnItemCollected -= OnItemCollected;
        }
    }
}