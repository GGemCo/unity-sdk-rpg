using GGemCo.Scripts;
using UnityEditor;
using UnityEngine;

namespace GGemCo.Editor
{
    public class StepDrawerTalkToNpc : IQuestStepDrawer
    {
        public void Draw(ref float y, Rect rect, QuestStep step)
        {
            step.mapUid = EditorGUI.IntField(new Rect(rect.x, y, rect.width, 18), "맵 Uid", step.mapUid);
            y += 20;
            step.targetUid = EditorGUI.IntField(new Rect(rect.x, y, rect.width, 18), "타겟 Uid", step.targetUid);
            y += 20;
        }
        public float GetHeight() => 2 * 20; // 2줄
    }
}