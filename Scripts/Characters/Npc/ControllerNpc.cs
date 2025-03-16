using GGemCo.Scripts.Scenes;
using UnityEngine;

namespace GGemCo.Scripts.Characters.Npc
{
    /// <summary>
    /// Npc 움직임 처리
    /// </summary>
    public class ControllerNpc : MonoBehaviour
    {
        private Npc npc;
        private ICharacterAnimationController iCharacterAnimationController;
        private Vector2 minBounds, maxBounds; // 타일맵의 최소/최대 경계
        private (float width, float height) mapSize;
        
        private void Awake()
        {
            npc = GetComponent<Npc>();
            // 타일맵의 경계를 가져오는 코드 (직접 설정 가능)
            minBounds = new Vector2(0f, 0f); // 좌측 하단 경계
        }

        private void Start()
        {
            iCharacterAnimationController = npc.CharacterAnimationController;
            mapSize = SceneGame.Instance.mapManager.GetCurrentMapSize();
            iCharacterAnimationController?.PlayWaitAnimation();
        }
    }
}