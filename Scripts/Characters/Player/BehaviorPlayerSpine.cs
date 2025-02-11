﻿#if GGEMCO_USE_SPINE
using System.Collections;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.Spine2d;
using Spine.Unity;
using UnityEngine;
using Event = Spine.Event;

namespace GGemCo.Scripts.Characters.Player
{
    public class BehaviorPlayerSpine : Spine2dController
    {
        private Transform attackerTransform;
        private Player player;

        protected override void Awake()
        {
            base.Awake();
            player = GetComponent<Player>();
        }
        private void HandleInput()
        {
            Direction = Vector3.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) Direction += Vector3.up;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) Direction += Vector3.down;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) Direction += Vector3.left;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) Direction += Vector3.right;

            Direction.Normalize();
        }

        private void HandleAttack()
        {
            if (IsAttacking) return; // 공격 중이면
            if (Input.GetKeyDown(KeyCode.Space))
            {
                IsAttacking = true; // 공격 중 상태 설정
                Direction = Vector3.zero; // 움직임 멈춤
                SkeletonAnimation.AnimationState.SetAnimation(0, attackAnim, false);

                // 공격 애니메이션 완료 시 이벤트 등록
                SkeletonAnimation.state.Complete += OnAttackComplete;
            }
        }

        private void OnAttackComplete(Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == attackAnim)
            {
                IsAttacking = false; // 공격 상태 해제
                SkeletonAnimation.state.Complete -= OnAttackComplete; // 이벤트 해제
            }
        }

        private void PlayAnimation(string animationName, float scaleX)
        {
            SkeletonAnimation.AnimationName = animationName;
            player.transform.localScale = new Vector3(player.OriginalScaleX * scaleX, player.transform.localScale.y,player.transform.localScale.z);
            SkeletonAnimation.timeScale = player.CurrentMoveSpeed;
        }

        private void UpdateDirectionAnimation()
        {
            if (IsAttacking) return; // 공격 중이면
            if (player.IsStatusDead()) return; // 죽었으면
            
            // 정지 상태 처리
            if (Direction == Vector3.zero)
            {
                string idleAnim = DirectionPrev.y != 0 
                    ? (DirectionPrev.y > 0 ? waitBackwardAnim : waitForwardAnim) 
                    : waitForwardAnim;

                PlayAnimation(idleAnim, DirectionPrev.x >= 0 ? -1 : 1);
            }
            // 이동 상태 처리
            else
            {
                string moveAnim = Direction.y != 0 
                    ? (Direction.y > 0 ? walkBackwardAnim : walkForwardAnim) 
                    : walkForwardAnim;

                PlayAnimation(moveAnim, Direction.x >= 0 ? -1 : 1);
                DirectionPrev = Direction;
            }
        }

        private void Update()
        {
            if (IsAttacking) return; // 공격 중이면 움직임 차단

            HandleInput();
            HandleAttack();
            UpdateDirectionAnimation();
            player.transform.Translate(Direction * (player.CurrentMoveStep * player.CurrentMoveSpeed * Time.deltaTime));
        }

        protected override void OnSpineEventShake(Event @event) 
        {
        
        }

        protected override void OnSpineEventAttack(Event @event) 
        {
            // GcLogger.Log(@event);
            long totalDamage = SceneGame.Instance.calculateManager.GetPlayerTotalAtk();
        
            // 캡슐 콜라이더 2D와 충돌 중인 모든 콜라이더를 검색
            CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
            Vector2 size = new Vector2(capsuleCollider.size.x * Mathf.Abs(transform.localScale.x), capsuleCollider.size.y * transform.localScale.y);
            Vector2 point = (Vector2)transform.position + capsuleCollider.offset * transform.localScale;
            int hitCount = Physics2D.OverlapCapsuleNonAlloc(point, size, capsuleCollider.direction, 0f, player.hits);

            int countDamageMonster = 0;
            int maxDamageMonster = 10;
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = player.hits[i];
                if (hit.CompareTag(ConfigTags.Monster))
                {
                    Monster.Monster monster = hit.GetComponent<Monster.Monster>();
                    if (monster != null)
                    {
                        // GcLogger.Log("Player attacked the monster after animation!");
                        // 몬스터와 마주보고 있으면 공격 
                        if (player.IsFlipped() != monster.IsFlipped())
                        {
                            monster.OnDamage(totalDamage, gameObject);
                            ++countDamageMonster;
                        }
                        // 몬스터와 같은 곳을 바라보고 있으면,
                        else if (player.IsFlipped() == monster.IsFlipped())
                        {
                            // flip 일때는 
                            // monster.x >= player.x
                            if (player.IsFlipped() && monster.transform.position.x >= player.transform.position.x)
                            {
                                monster.OnDamage(totalDamage, gameObject);
                                ++countDamageMonster;
                            }
                            // flip 이 아닐때는
                            // monster.x <= player.x
                            else if (player.IsFlipped() != true && monster.transform.position.x <= player.transform.position.x)
                            {
                                monster.OnDamage(totalDamage, gameObject);
                                ++countDamageMonster; 
                            }
                        }
                        
                        // maxDamageMonster 마리 한테만 데미지 준다 
                        if (countDamageMonster > maxDamageMonster)
                        {
                            break;
                        }
                    }
                }
            }
        }

        protected override void OnSpineEventSound(Event @event) 
        {
        }

        public override IEnumerator FadeEffect(float duration, bool fadeIn)
        {
            float elapsedTime = 0f;
            float startAlpha = fadeIn ? 0 : 1;
            float endAlpha = fadeIn ? 1 : 0;

            Color color = SkeletonAnimation.Skeleton.GetColor();

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                SkeletonAnimation.Skeleton.SetColor(color);
                yield return null;
            }

            player.SetIsStartFade(false);
        }
        public override void PlayDeadAnimation()
        {
            PlayAnimation(deadAnim);
        }
    }
}
#endif