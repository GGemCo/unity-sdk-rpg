﻿using System;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    public class CharacterController : MonoBehaviour
    {
        protected CharacterBase TargetCharacter;
        private Vector2 minBounds; // 타일맵의 최소/최대 경계
        private Vector2 maxBounds; // 타일맵의 최소/최대 경계
        private (float width, float height) mapSize;
        protected ICharacterAnimationController ICharacterAnimationController;

        protected virtual void Awake()
        {
            TargetCharacter = GetComponent<CharacterBase>();
        }
        protected virtual void Start()
        {
            // 타일맵의 경계를 가져오는 코드 (직접 설정 가능)
            minBounds = new Vector2(0f, 0f); // 좌측 하단 경계
            mapSize = SceneGame.Instance.mapManager.GetCurrentMapSize();
            ICharacterAnimationController = TargetCharacter.CharacterAnimationController;
        }
        protected void UpdateCheckMaxBounds()
        {
            if (ICharacterAnimationController == null)
            {
                GcLogger.LogError("애니메이션 컨트롤러가 없습니다.");
                return;
            }
            if (TargetCharacter.IsStatusDead()) return;
            var characterSize = ICharacterAnimationController.GetCharacterSize();
            characterSize.x *= Math.Abs(TargetCharacter.transform.localScale.x);
            characterSize.y *= TargetCharacter.transform.localScale.y;
            minBounds.x = characterSize.x / 2;
            maxBounds = new Vector2(mapSize.width - (characterSize.x/2), mapSize.height - characterSize.y);   // 우측 상단 경계
        }

        protected void UpdateDirection(float targetDirection = 0)
        {
            float scaleX = TargetCharacter.directionPrev.x >= 0 ? -1 : 1;
            if (targetDirection != 0)
            {
                scaleX = targetDirection;
            }
            TargetCharacter.transform.localScale = new Vector3(TargetCharacter.originalScaleX * scaleX,
                TargetCharacter.transform.localScale.y, TargetCharacter.transform.localScale.z);
        }
        protected virtual bool Wait()
        {
            if (TargetCharacter.IsStatusAttack()) return false;
            if (TargetCharacter.IsStatusDead()) return false;
            ICharacterAnimationController?.PlayWaitAnimation();
            return true;
        }
        /// <summary>
        /// run 애니메이션 하기
        /// </summary>
        protected virtual bool Run()
        {
            if (TargetCharacter.IsStatusAttack()) return false;
            if (TargetCharacter.IsStatusDead()) return false;
            
            UpdateDirection();
            TargetCharacter.directionPrev = TargetCharacter.direction;
            
            ICharacterAnimationController?.PlayRunAnimation();
            
            UpdateCheckMaxBounds();
            // 이동 처리
            Vector3 nextPosition = TargetCharacter.transform.position + TargetCharacter.direction * (TargetCharacter.currentMoveStep * TargetCharacter.GetCurrentMoveSpeed() * Time.deltaTime);

            // 경계 체크 (타일맵 범위를 벗어나지 않도록 제한)
            nextPosition.x = Mathf.Clamp(nextPosition.x, minBounds.x, maxBounds.x);
            nextPosition.y = Mathf.Clamp(nextPosition.y, minBounds.y, maxBounds.y);

            TargetCharacter.transform.position = nextPosition;
            return true;
        }
        /// <summary>
        /// 공격 실행
        /// </summary>
        protected virtual void Attack()
        {
            if (TargetCharacter.IsStatusAttack() || TargetCharacter.IsStatusDead()) return;

            UpdateDirection();
            TargetCharacter.SetStatusAttack();
            ICharacterAnimationController?.PlayAttackAnimation();
        }
    }
}