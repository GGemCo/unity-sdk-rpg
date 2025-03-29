﻿using System.Collections;
using GGemCo.Scripts.Configs;
using UnityEngine;
using Event = UnityEngine.Event;

namespace GGemCo.Scripts.Characters.Monster
{
    /// <summary>
    /// 몬스터 선공, 후공 처리 
    /// </summary>
    public class ControllerMonster : CharacterController
    {
        private CapsuleCollider2D capsuleCollider;
        private Coroutine coroutineAttack;

        private const float DelayTimeAttack = 0f;

        protected override void Awake()
        {
            base.Awake();
            capsuleCollider = GetComponent<CapsuleCollider2D>();
        }
        
        /// <summary>
        /// 입력 처리 - 공격자 방향 계산
        /// </summary>
        private void HandleInput()
        {
            if (!TargetCharacter.IsAggro() || TargetCharacter.attackerTransform == null || TargetCharacter.IsStatusDead()) return;
            TargetCharacter.direction = (TargetCharacter.attackerTransform.position - TargetCharacter.transform.position).normalized;
        }
        private void Update()
        {
            if (TargetCharacter.IsStatusDead()) return;
            
            if (TargetCharacter.IsAggro())
            {
                if (SearchAttackerTarget())
                {
                    StartAttackCoroutine();
                }
                else
                {
                    HandleInput();
                    Run();
                }
            }
            else
            {
                Wait();
            }
        }
        /// <summary>
        /// Wait  
        /// </summary>
        /// <returns></returns>
        protected override bool Wait()
        {
            if (!base.Wait()) return false;
            StopAttackCoroutine();
            return true;
        }
        /// <summary>
        /// run 
        /// </summary>
        protected override bool Run()
        {
            if (!base.Run()) return false;
            StopAttackCoroutine();
            return true;
        }
        /// <summary>
        /// 주위에서 공격자를 검색
        /// </summary>
        private bool SearchAttackerTarget()
        {
            if (TargetCharacter.attackerTransform == null) return false;
            if (TargetCharacter.IsStatusAttack() || TargetCharacter.IsStatusDead()) return false;
            Vector2 size = new Vector2(capsuleCollider.size.x * Mathf.Abs(transform.localScale.x), capsuleCollider.size.y * transform.localScale.y);
            Collider2D[] collider2Ds = Physics2D.OverlapCapsuleAll(transform.position, size, capsuleCollider.direction, 0f);

            foreach (var hit in collider2Ds)
            {
                if (hit.CompareTag(TargetCharacter.attackerTransform.tag) && hit.GetComponent<Player.Player>() != null && hit.GetComponent<Player.Player>().IsStatusDead() == false)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// DelayTimeAttack 시간 후에 공격하기
        /// </summary>
        /// <returns></returns>
        private IEnumerator DownAttackByTime()
        {
            while (true)
            {
                Attack();
                yield return new WaitForSeconds(DelayTimeAttack);
            }
        }
        /// <summary>
        /// 방향에 따른 X 축 스케일 반환
        /// </summary>
        private int GetScaleByDirection(Vector2 direction)
        {
            return direction.x >= 0 ? -1 : 1;
        }
        /// <summary>
        /// 공격 실행
        /// </summary>
        protected override void Attack()
        {
            // 공격자가 죽었을 때
            if (TargetCharacter.IsAttackerStatusDead())
            {
                TargetCharacter.SetAttackerTarget(null);
                Stop();
                return;
            }
            if (TargetCharacter.IsStatusAttack() || TargetCharacter.IsStatusDead()) return;

            // 공격자 방향 찾기
            HandleInput();
            // 공격자 방향으로 flip 처리 하기
            UpdateDirection(GetScaleByDirection(TargetCharacter.direction));
            
            TargetCharacter.SetStatusAttack();
            ICharacterAnimationController?.PlayAttackAnimation();
        }
        /// <summary>
        /// 공격하기 코루틴 시작
        /// </summary>
        private void StartAttackCoroutine()
        {
            if (coroutineAttack != null || TargetCharacter.IsStatusAttack() || TargetCharacter.IsStatusDead()) return;

            coroutineAttack = StartCoroutine(DownAttackByTime());
        }
        /// <summary>
        /// 공격하기 코루틴 정지
        /// </summary>
        public void StopAttackCoroutine()
        {
            if (coroutineAttack == null) return;

            StopCoroutine(coroutineAttack);
            coroutineAttack = null;
        }
        /// <summary>
        /// 어그로 on 이고 공격자 transform 이 있을때 플레이어가 몬스터 가까이 가면 attack 상태 처리
        /// </summary>
        /// <param name="collision"></param>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Player)))
            {
                if (TargetCharacter.IsStatusDead()) return;
                
                if (TargetCharacter.IsAggro() && TargetCharacter.attackerTransform != null)
                {
                    TargetCharacter.SetStatusAttack();
                }
                // 선공
                else if (TargetCharacter.GetAttackType() == CharacterBase.AttackType.AggroFirst && TargetCharacter.IsAggro() == false)
                {
                    TargetCharacter.SetAggro(true);
                    TargetCharacter.SetAttackerTarget(collision.gameObject.transform);
                }
            }
        }
        /// <summary>
        /// 몬스터 공격 범위 밖으로 플레이어가 나가면 공격 상태 취소하기
        /// </summary>
        /// <param name="collision"></param>
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Player)))
            {
                StopAttackCoroutine();
            }
        }
        protected void OnSpineEventShake(Event @event) 
        {
        
        }
    }
}