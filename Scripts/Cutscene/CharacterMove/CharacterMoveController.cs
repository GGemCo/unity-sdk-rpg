using System.Collections;
using UnityEngine;

namespace GGemCo.Scripts
{
    /// <summary>
    /// 연출 - 캐릭터 이동
    /// </summary>
    public class CharacterMoveController : CutsceneDefaultController, ICutsceneController
    {
        private Camera cam;
        private Vector2 startPosition, endPosition;
        private float step;
        private float speed;
        private float distance;
        private float timer;
        private bool isMoving;
        private bool isFollowTarget;

        private Transform newTarget;
        private CharacterBase newTargetCharacter;

        public CharacterMoveController(CutsceneManager manager)
        {
            CutsceneManager = manager;
        }

        public IEnumerator Ready(CutsceneEvent evt)
        {
            if (evt.type != EventType.CharacterMove) yield break;
            var data = evt.characterMove;
            
            Transform character = GetTargetTransform(data.characterType, data.characterUid);
            // 현재 맵에서 없으면 스폰한다 
            if (character == null)
            {
                character = CutsceneManager.GetCharacter(data.characterType, data.characterUid);
                if (character == null)
                {
                    character = SceneGame.Instance.CharacterManager.CreateCharacter(data.characterType,
                        data.characterUid,
                        startPosition, SceneGame.Instance.mapManager.GetCurrentMap())?.transform;
                    if (character == null) yield break;
                    character.position = startPosition;
                    character.gameObject.SetActive(false);
                    character.GetComponent<CharacterBase>()?.SetCurrentMoveSpeed(data.characterMoveSpeed);
                    CutsceneManager.AddCharacter(data.characterType, data.characterUid, character.gameObject);
                }
            }
            
            // 캐릭터 타겟 찾기 같은 준비
            yield return null;
        }

        public void Trigger(CutsceneEvent evt)
        {
            if (evt.type != EventType.CharacterMove) return;
            var data = evt.characterMove;
            isFollowTarget = data.isFollowTarget;
            newTarget = GetTargetTransform(data.characterType, data.characterUid);
            if (newTarget == null)
            {
                newTarget = CutsceneManager.GetCharacter(data.characterType, data.characterUid);
                if (newTarget == null)
                {
                    GcLogger.LogError("이동 시킬 캐릭터가 없습니다. type: " + data.characterType + "/ uid: " + data.characterUid);
                    return;
                }
            }
            if (newTarget.gameObject.activeSelf == false)
            {
                newTarget.gameObject.SetActive(true);
            }

            startPosition = data.startPosition.ToVector2();
            endPosition = data.endPosition.ToVector2();
            if (startPosition == Vector2.zero)
            {
                startPosition = newTarget.position;
            }
            speed = data.speed > 0f ? data.speed : 1f; // 데이터에서 속도 받기, 기본값 보정
            distance = Vector2.Distance(startPosition, endPosition);
            
            if (newTarget != null)
            {
                newTargetCharacter = newTarget.GetComponent<CharacterBase>();
                // step 적용
                step = AddressableSettingsLoader.Instance.playerSettings.statMoveStep;
                if (data.characterType != CharacterConstants.Type.Player)
                {
                    step = TableLoaderManager.Instance.GetCharacterMoveStep(data.characterType, data.characterUid);
                }
                // 크기 조정
                if (data.characterScale > 0)
                {
                    newTarget.localScale = new Vector3(data.characterScale, data.characterScale, 0);
                }
                // 카메라가 따라가야하는 타겟 설정
                if (isFollowTarget)
                {
                    SceneGame.Instance.cameraManager.SetFollowTarget(newTarget);
                }
                newTargetCharacter.SetStatusMoveForce();
                newTargetCharacter.CharacterAnimationController?.PlayRunAnimation();
            }
            timer = 0f;
            UpdateFacing();
            isMoving = true;
        }
        public void Update()
        {
            if (newTarget == null || !isMoving) return;

            timer += Time.deltaTime;
            float t = (speed > 0f) ? (timer * step * speed / distance) : 1f;
            t = Mathf.Clamp01(t);

            Vector2 interpolated = Vector2.Lerp(startPosition, endPosition, t);
            newTarget.position = new Vector3(interpolated.x, interpolated.y, newTarget.position.z);

            if (t >= 1f)
            {
                Stop();
            }
        }
        private void UpdateFacing()
        {
            if (newTarget == null) return;

            Vector2 direction = endPosition - startPosition;

            // 좌우만 처리하는 경우
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                bool movingRight = direction.x > 0f;
                bool defaultIsRight = newTargetCharacter?.characterFacing == CharacterConstants.CharacterFacing.Right;

                bool shouldFlip = (movingRight != defaultIsRight);
                newTargetCharacter?.SetFlip(shouldFlip);
            }

            // 상하 전환도 필요하다면 추가 구현 가능
        }
        public void Stop()
        {
            newTargetCharacter?.Stop();
            isMoving = false;
        }
        public void End()
        {
        }
    }
}
