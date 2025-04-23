using System.Collections;
using UnityEngine;

namespace GGemCo.Scripts
{
    /// <summary>
    /// 연출 - 카메라 이동하기
    /// </summary>
    public class CameraMoveController : CutsceneDefaultController, ICutsceneController
    {
        private Camera cam;
        private Vector2 startPosition, endPosition;
        private float duration;
        private float timer;
        private bool isMoving;
        private Easing.EaseType easing;

        private Transform followTarget;

        public CameraMoveController(CutsceneManager manager)
        {
            CutsceneManager = manager;
            cam = SceneGame.Instance.mainCamera;
        }

        public IEnumerator Ready(CutsceneEvent evt)
        {
            // 캐릭터 타겟 찾기 같은 준비
            yield return null;
        }
        public void Trigger(CutsceneEvent evt)
        {
            if (evt.type != EventType.CameraMove) return;
            var data = evt.cameraMove;

            startPosition = data.startPosition.ToVector2();
            endPosition = data.endPosition.ToVector2();
            duration = data.duration;
            easing = data.easing;

            timer = 0f;
            isMoving = true;
        }

        public void Update()
        {
            if (!isMoving) return;

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float easedT = Easing.Apply(t, easing);

            Vector2 basePos = followTarget ? followTarget.position : Vector2.Lerp(startPosition, endPosition, easedT);
            cam.transform.position= new Vector3(basePos.x, basePos.y, cam.transform.position.z);

            if (t >= 1f)
            {
                Stop();
            }
        }

        public void Stop()
        {
            isMoving = false;
        }
        public void End()
        {
            isMoving = false;
        }
    }
}
