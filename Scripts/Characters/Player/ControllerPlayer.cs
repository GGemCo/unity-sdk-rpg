using UnityEngine;

namespace GGemCo.Scripts.Characters.Player
{
    /// <summary>
    /// 플레이어 움직임 처리
    /// </summary>
    public class ControllerPlayer : MonoBehaviour
    {
        private Player player;
        private ICharacterAnimationController iCharacterAnimationController;

        private void Start()
        {
            player = GetComponent<Player>();
            iCharacterAnimationController = player.CharacterAnimationController;
        }

        private void HandleInput()
        {
            player.direction = Vector3.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) player.direction += Vector3.up;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) player.direction += Vector3.down;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) player.direction += Vector3.left;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) player.direction += Vector3.right;

            player.direction.Normalize();
        }
        private void HandleAttack()
        {
            if (player.IsAttacking) return; // 공격 중이면
            if (Input.GetKeyDown(KeyCode.Space))
            {
                player.IsAttacking = true; // 공격 중 상태 설정
                player.direction = Vector3.zero; // 움직임 멈춤
                iCharacterAnimationController?.PlayAttackAnimation();
            }
        }
        void Update()
        {
            if (player.IsAttacking) return; // 공격 중이면
            if (player.IsStatusDead()) return; // 죽었으면
            
            HandleInput();
            HandleAttack();
            iCharacterAnimationController?.UpdateAnimation();
            
            // 이동 처리
            Vector3 nextPosition = player.transform.position + player.direction * (player.GetCurrentMoveStep() * player.GetCurrentMoveSpeed() * Time.deltaTime);

            // 경계 체크 (타일맵 범위를 벗어나지 않도록 제한)
            // nextPosition.x = Mathf.Clamp(nextPosition.x, minBounds.x, maxBounds.x);
            // nextPosition.y = Mathf.Clamp(nextPosition.y, minBounds.y, maxBounds.y);

            player.transform.position = nextPosition;
        }
    }
}