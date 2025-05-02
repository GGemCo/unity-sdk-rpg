using GGemCo.Scripts;
using UnityEngine;

namespace GGemCo.Editor
{
    public interface IQuestStepDrawer
    {
        void Draw(ref float y, Rect rect, QuestStep step);
        float GetHeight();
    }
}