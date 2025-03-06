#if GGEMCO_USE_SPINE
using System;
using System.Collections;
using GGemCo.Scripts.Spine2d;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Scenes;
using Spine;
using Spine.Unity;
using UnityEngine;
using Event = Spine.Event;

namespace GGemCo.Scripts.Characters.Monster.Behavior
{
    /// <summary>
    /// 방어 행동 클래스
    /// </summary>
    public class BehaviorPassiveDefense : Spine2dController
    {
        private Monster monster;
        private CapsuleCollider2D capsuleCollider;
        private Coroutine coroutineAttack;

        private const float DelayAttack = 0f;
        private Vector2 minBounds, maxBounds; // 타일맵의 최소/최대 경계
        private (float width, float height) mapSize;

        protected override void Awake()
        {
            base.Awake();
            monster = GetComponent<Monster>();
            capsuleCollider = monster.GetComponent<CapsuleCollider2D>();
            SkeletonAnimation.state.Complete += OnAttackComplete;
        }
        protected override void Start()
        {
            base.Start();
            // 타일맵의 경계를 가져오는 코드 (직접 설정 가능)
            minBounds = new Vector2(0f, 0f); // 좌측 하단 경계
            mapSize = SceneGame.Instance.mapManager.GetCurrentMapSize();
        }

        private void Update()
        {
            if (IsAttackAnimationing() || monster.IsStatusDead()) return;

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
                PlayIdleAnimation();
            }
        }

        #region Attack Handling

        /// <summary>
        /// 입력 처리 - 공격자 방향 계산
        /// </summary>
        private void HandleInput()
        {
            if (!monster.isAggro || AttackerTransform == null || monster.IsStatusDead()) return;
            Direction = (AttackerTransform.position - monster.transform.position).normalized;
        }

        /// <summary>
        /// 공격 실행
        /// </summary>
        private void HandleAttack()
        {
            if (IsAttackAnimationing() || monster.IsStatusDead()) return;

            HandleInput();
            UpdateMonsterScale(Direction);
            TrackEntry attack = SkeletonAnimation.AnimationState.SetAnimation(0, attackAnim, false);
            attack.TimeScale = monster.currentAttackSpeed;
        }

        /// <summary>
        /// 공격 애니메이션 완료 이벤트
        /// </summary>
        private void OnAttackComplete(TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name != attackAnim || monster.IsStatusDead()) return;

            PlayIdleAnimation();
        }

        private void OnDestroy()
        {
            SkeletonAnimation.state.Complete -= OnAttackComplete;
        }

        /// <summary>
        /// 주위에서 공격자를 검색
        /// </summary>
        private bool SearchAttackerTarget()
        {
            if (AttackerTransform == null || monster.IsStatusDead()) return false;
            Vector2 size = new Vector2(capsuleCollider.size.x * Mathf.Abs(transform.localScale.x), capsuleCollider.size.y * transform.localScale.y);
            Collider2D[] collider2Ds = Physics2D.OverlapCapsuleAll(transform.position, size, capsuleCollider.direction, 0f);

            foreach (var hit in collider2Ds)
            {
                if (hit.CompareTag(AttackerTransform.tag) && hit.GetComponent<Player.Player>() != null)
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

        private void StopAttackCoroutine()
        {
            if (coroutineAttack == null) return;

            StopCoroutine(coroutineAttack);
            coroutineAttack = null;
        }

        #endregion

        #region Animation Handling

        /// <summary>
        /// 현재 애니메이션 이름이 공격 애니메이션인지 확인
        /// </summary>
        private bool IsAttackAnimationing()
        {
            return GetCurrentAnimation() == attackAnim;
        }

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

        /// <summary>
        /// 정지 상태 애니메이션
        /// </summary>
        private void PlayIdleAnimation()
        {
            string idleAnim = DirectionPrev.y != 0 
                ? (DirectionPrev.y > 0 ? waitBackwardAnim : waitForwardAnim) 
                : waitForwardAnim;

            PlayAnimation(idleAnim);
        }

        /// <summary>
        /// 이동 상태 애니메이션
        /// </summary>
        private void PlayRunAnimation()
        {
            if (monster.IsStatusDead()) return;
            
            string moveAnim = Direction.y != 0 
                ? (Direction.y > 0 ? walkBackwardAnim : walkForwardAnim) 
                : walkForwardAnim;

            PlayAnimation(moveAnim);
            
            UpdateCheckMaxBounds();
            // 이동 처리
            Vector3 nextPosition = monster.transform.position + Direction * (monster.CurrentMoveStep * monster.CurrentMoveSpeed * Time.deltaTime);

            // 경계 체크 (타일맵 범위를 벗어나지 않도록 제한)
            nextPosition.x = Mathf.Clamp(nextPosition.x, minBounds.x, maxBounds.x);
            nextPosition.y = Mathf.Clamp(nextPosition.y, minBounds.y, maxBounds.y);

            monster.transform.position = nextPosition;
        }

        /// <summary>
        /// 애니메이션 실행
        /// </summary>
        private void PlayAnimation(string animationName)
        {
            if (monster.IsStatusDead()) return;
            SkeletonAnimation.AnimationName = animationName;
            UpdateMonsterScale(Direction);
            SkeletonAnimation.timeScale = monster.CurrentMoveSpeed;
            DirectionPrev = Direction;
        }

        #endregion

        #region Fade Effect

        public override IEnumerator FadeEffect(float duration, bool fadeIn)
        {
            float elapsedTime = 0f;
            float startAlpha = fadeIn ? 0 : 1;
            float endAlpha = fadeIn ? 1 : 0;

            var color = SkeletonAnimation.Skeleton.GetColor();

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                SkeletonAnimation.Skeleton.SetColor(color);
                yield return null;
            }

            monster.SetIsStartFade(false);
        }

        #endregion

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Player)))
            {
                if (monster.IsStatusDead()) return;
                
                IsAttacking = false;
                StopAttackCoroutine();
            }
        }
        protected override void OnSpineEventShake(Event @event) 
        {
        
        }
        protected override void OnSpineEventAttack(Event @event) 
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
                        player.OnDamage(totalDamage, gameObject);
                        break;
                    }
                }
            }
        }
        protected override void OnSpineEventSound(Event @event) 
        {
        }
        public override void PlayDeadAnimation()
        {
            StopCoroutine(coroutineAttack);
            coroutineAttack = null;
            SkeletonAnimation.AnimationState.SetAnimation(0, deadAnim, false);
        }
        private void UpdateCheckMaxBounds()
        {
            if (monster.IsStatusDead()) return;
            var characterSize = GetCharacterSize();
            characterSize.x *= Math.Abs(monster.transform.localScale.x);
            characterSize.y *= monster.transform.localScale.y;
            minBounds.x = characterSize.x / 2;
            maxBounds = new Vector2(mapSize.width - (characterSize.x/2), mapSize.height - characterSize.y);   // 우측 상단 경계
        }
    }
}
#endif
