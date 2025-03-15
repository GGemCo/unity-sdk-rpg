using System;
using System.Collections;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.Utils;
using UnityEngine;
using Event = UnityEngine.Event;

namespace GGemCo.Scripts.Characters.Monster
{
    /// <summary>
    /// 몬스터 선공, 후공 처리 
    /// </summary>
    public class ControllerMonster : MonoBehaviour
    {
        private Monster monster;
        private ICharacterAnimationController iCharacterAnimationController;

        private CapsuleCollider2D capsuleCollider;
        private Coroutine coroutineAttack;

        private const float DelayAttack = 0f;
        private Vector2 minBounds, maxBounds; // 타일맵의 최소/최대 경계
        private (float width, float height) mapSize;

        private void Awake()
        {
            capsuleCollider = GetComponent<CapsuleCollider2D>();
        }
        private void Start()
        {
            monster = GetComponent<Monster>();
            iCharacterAnimationController = monster.CharacterAnimationController;
            // 타일맵의 경계를 가져오는 코드 (직접 설정 가능)
            minBounds = new Vector2(0f, 0f); // 좌측 하단 경계
            mapSize = SceneGame.Instance.mapManager.GetCurrentMapSize();
        }
        
        private void Update()
        {
            if (monster.IsAttacking || monster.IsStatusDead()) return;

            HandleInput();

            if (monster.isAggro)
            {
                if (SearchAttackerTarget())
                {
                    StartAttackCoroutine();
                }
                else
                {
                    StopAttackCoroutine();
                    PlayRunAnimation();
                }
            }
            else
            {
                StopAttackCoroutine();
                iCharacterAnimationController?.PlayWaitAnimation();
            }
        }

        #region Attack Handling
        private void PlayRunAnimation()
        {
            if (monster.IsAttacking) return;
            if (monster.IsStatusDead()) return;
            
            iCharacterAnimationController?.PlayRunAnimation();
            
            UpdateCheckMaxBounds();
            // 이동 처리
            Vector3 nextPosition = monster.transform.position + monster.direction * (monster.CurrentMoveStep * monster.GetCurrentMoveSpeed() * Time.deltaTime);

            // 경계 체크 (타일맵 범위를 벗어나지 않도록 제한)
            nextPosition.x = Mathf.Clamp(nextPosition.x, minBounds.x, maxBounds.x);
            nextPosition.y = Mathf.Clamp(nextPosition.y, minBounds.y, maxBounds.y);

            monster.transform.position = nextPosition;
        }

        /// <summary>
        /// 입력 처리 - 공격자 방향 계산
        /// </summary>
        private void HandleInput()
        {
            if (!monster.isAggro || monster.AttackerTransform == null || monster.IsStatusDead()) return;
            monster.direction = (monster.AttackerTransform.position - monster.transform.position).normalized;
        }

        /// <summary>
        /// 공격 실행
        /// </summary>
        private void HandleAttack()
        {
            if (monster.IsAttacking || monster.IsStatusDead()) return;

            HandleInput();
            UpdateMonsterScale(monster.direction);
            monster.IsAttacking = true;
            iCharacterAnimationController?.PlayAttackAnimation();
        }
        /// <summary>
        /// 주위에서 공격자를 검색
        /// </summary>
        private bool SearchAttackerTarget()
        {
            if (monster.AttackerTransform == null || monster.IsStatusDead()) return false;
            Vector2 size = new Vector2(capsuleCollider.size.x * Mathf.Abs(transform.localScale.x), capsuleCollider.size.y * transform.localScale.y);
            Collider2D[] collider2Ds = Physics2D.OverlapCapsuleAll(transform.position, size, capsuleCollider.direction, 0f);

            foreach (var hit in collider2Ds)
            {
                if (hit.CompareTag(monster.AttackerTransform.tag) && hit.GetComponent<Player.Player>() != null)
                {
                    return true;
                }
            }
            return false;
        }

        private IEnumerator DownAttackByTime()
        {
            while (true)
            {
                HandleAttack();
                yield return new WaitForSeconds(DelayAttack);
            }
        }

        private void StartAttackCoroutine()
        {
            if (coroutineAttack != null || monster.IsStatusDead()) return;

            coroutineAttack = StartCoroutine(DownAttackByTime());
        }

        public void StopAttackCoroutine()
        {
            if (coroutineAttack == null) return;

            StopCoroutine(coroutineAttack);
            coroutineAttack = null;
        }

        #endregion

        #region Animation Handling

        /// <summary>
        /// 몬스터의 방향에 따른 스케일 설정
        /// </summary>
        private void UpdateMonsterScale(Vector2 direction)
        {
            if (!monster.isAggro || monster.IsStatusDead()) return;
            monster.transform.localScale = new Vector3(monster.OriginalScaleX * GetScaleByDirection(direction), 
                                                       monster.transform.localScale.y, 
                                                       monster.transform.localScale.z);
        }

        /// <summary>
        /// 방향에 따른 X 축 스케일 반환
        /// </summary>
        private int GetScaleByDirection(Vector2 direction)
        {
            return direction.x >= 0 ? -1 : 1;
        }
        #endregion

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Player)))
            {
                if (monster.IsStatusDead()) return;
                
                monster.IsAttacking = false;
                StopAttackCoroutine();
            }
        }
        protected void OnSpineEventShake(Event @event) 
        {
        
        }
        protected void OnSpineEventAttack(Event @event) 
        {
            if (monster.IsStatusDead()) return;
            // GcLogger.Log(@event);
            long totalDamage = SceneGame.Instance.calculateManager.GetMonsterTotalAtk(monster.Uid);
        
            // 캡슐 콜라이더 2D와 충돌 중인 모든 콜라이더를 검색
            CapsuleCollider2D capsuleCollider2D = GetComponent<CapsuleCollider2D>();
            Vector2 size = new Vector2(capsuleCollider2D.size.x * Mathf.Abs(transform.localScale.x), capsuleCollider2D.size.y * transform.localScale.y);
            Vector2 point = (Vector2)transform.position + capsuleCollider2D.offset * transform.localScale;
            Collider2D[] collider2Ds = Physics2D.OverlapCapsuleAll(point, size, capsuleCollider2D.direction, 0f);

            foreach (var hit in collider2Ds)
            {
                if (hit.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Player)))
                {
                    Player.Player player = hit.GetComponent<Player.Player>();
                    if (player != null)
                    {
                        // GcLogger.Log("Player attacked the monster after animation!");
                        player.TakeDamage(totalDamage, gameObject);
                        break;
                    }
                }
            }
        }
        private void UpdateCheckMaxBounds()
        {
            if (iCharacterAnimationController == null)
            {
                GcLogger.LogError("애니메이션 컨트롤러가 없습니다.");
                return;
            }
            if (monster.IsStatusDead()) return;
            var characterSize = iCharacterAnimationController.GetCharacterSize();
            characterSize.x *= Math.Abs(monster.transform.localScale.x);
            characterSize.y *= monster.transform.localScale.y;
            minBounds.x = characterSize.x / 2;
            maxBounds = new Vector2(mapSize.width - (characterSize.x/2), mapSize.height - characterSize.y);   // 우측 상단 경계
        }
    }
}