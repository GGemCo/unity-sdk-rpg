using GGemCo.Scripts;
using UnityEditor;
using UnityEngine;

namespace GGemCo.Editor
{
    public class StepDrawerCollectItem : IQuestStepDrawer
    {
        public void Draw(ref float y, Rect rect, QuestStep step)
        {
            step.targetUid = EditorGUI.IntField(new Rect(rect.x, y, rect.width, 18), "타겟 Uid", step.targetUid);
            y += 20;
            step.count = EditorGUI.IntField(new Rect(rect.x, y, rect.width, 18), "타겟 Count", step.count);
            y += 20;
        }
        public float GetHeight() => 3 * 20; // 2줄
    }
}