using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    public interface IBehaviorSpine
    {
        bool IsAttacking { get; set; }

        Vector3 Direction { get; set; }
        Vector3 DirectionPrev { get; set; }

        string WalkForwardAnim { get; set; }
        string WalkBackwardAnim { get; set; }
        string WaitForwardAnim  { get; set; }
        string WaitBackwardAnim { get; set; }
        string AttackAnim { get; set; }
    }
}