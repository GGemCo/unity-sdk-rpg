#if GGEMCO_USE_SPINE
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
        private Collider2D[] hits;
        private Coroutine coroutineAttack;

        private const float DelayAttack = 0f;

        protected override void Awake()
        {
            base.Awake();
            monster = GetComponent<Monster>();
            capsuleCollider = monster.GetComponent<CapsuleCollider2D>();
            hits = monster.hits;
            SkeletonAnimation.state.Complete += OnAttackComplete;
        }

        private void Update()
        {
            if (AttackerTransform == null || IsAttackAnimationing() || monster.IsStatusDead()) return;

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
            Direction = (AttackerTransform.position - monster.transform.position).normalized;
        }

        /// <summary>
        /// 공격 실행
        /// </summary>
        private void HandleAttack()
        {
            if (IsAttackAnimationing()) return;

            HandleInput();
            UpdateMonsterScale(Direction);
            TrackEntry attack = SkeletonAnimation.AnimationState.SetAnimation(0, attackAnim, false);
            attack.TimeScale = monster.currentAttackSpeed;
        }

        /// <summary>
        /// 공격 애니메이션 완료 이벤트
        /// </summary>
        private void OnAttackComplete(Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name != attackAnim) return;

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
            Vector2 size = new Vector2(capsuleCollider.size.x * Mathf.Abs(transform.localScale.x), capsuleCollider.size.y * transform.localScale.y);
            int hitCount = Physics2D.OverlapCapsuleNonAlloc(transform.position, size, capsuleCollider.direction, 0f, hits);

            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].CompareTag(AttackerTransform.tag) && hits[i].GetComponent<Player.Player>() != null)
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
            if (coroutineAttack != null) return;

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
            string moveAnim = Direction.y != 0 
                ? (Direction.y > 0 ? walkBackwardAnim : walkForwardAnim) 
                : walkForwardAnim;

            PlayAnimation(moveAnim);
            monster.transform.Translate(Direction * (monster.CurrentMoveStep * monster.CurrentMoveSpeed * Time.deltaTime));
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
            if (collision.CompareTag(ConfigTags.Player))
            {
                IsAttacking = false;
                StopAttackCoroutine();
            }
        }
        protected override void OnSpineEventShake(Event @event) 
        {
        
        }
        protected override void OnSpineEventAttack(Event @event) 
        {
            // GcLogger.Log(@event);
            long totalDamage = SceneGame.Instance.calculateManager.GetPlayerTotalAtk();
        
            // 캡슐 콜라이더 2D와 충돌 중인 모든 콜라이더를 검색
            CapsuleCollider2D capsuleCollider2D = GetComponent<CapsuleCollider2D>();
            Vector2 size = new Vector2(capsuleCollider2D.size.x * Mathf.Abs(transform.localScale.x), capsuleCollider2D.size.y * transform.localScale.y);
            Vector2 point = (Vector2)transform.position + capsuleCollider2D.offset * transform.localScale;
            int hitCount = Physics2D.OverlapCapsuleNonAlloc(point, size, capsuleCollider2D.direction, 0f, monster.hits);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = monster.hits[i];
                if (hit.CompareTag(ConfigTags.Player))
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
    }
}
#endif
